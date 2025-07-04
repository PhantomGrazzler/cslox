﻿using System.Diagnostics.CodeAnalysis;

namespace cslox;

/// <summary>
/// 
/// </summary>
/// <param name="token"></param>
/// <param name="message"></param>
public sealed class RuntimeError(Token token, string message) : Exception(message)
{
    /// <summary>
    /// 
    /// </summary>
    public Token Token { get; } = token;
}

internal class ClockCallable : ILoxCallable
{
    public int Arity() => 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interpreter">Unused</param>
    /// <param name="arguments">Unused</param>
    /// <returns>Ticks in seconds.</returns>
    public object Call(Interpreter interpreter, IEnumerable<object?> arguments)
    {
        return DateTime.UtcNow.Ticks / 10_000_000.0;
    }

    public override string ToString() => "<native fn>";
}

/// <summary>
/// 
/// </summary>
public class Interpreter : Expr.IVisitor<object?>
                         , Stmt.IVisitor<object?>
{
    private LoxEnvironment m_environment;
    private readonly Dictionary<Expr, int> m_locals = [];

    /// <summary>
    /// Global environment.
    /// </summary>
    public readonly LoxEnvironment Globals = new();

    /// <summary>
    /// 
    /// </summary>
    public Interpreter()
    {
        m_environment = Globals;

        Globals.Define("clock", new ClockCallable());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statements"></param>
    public void Interpret(IEnumerable<Stmt?> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError e)
        {
            Lox.RuntimeError(e);
        }
    }

    private void Execute(Stmt? statement)
    {
        _ = statement?.Accept(this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="depth"></param>
    public void Resolve(Expr expr, int depth)
    {
        m_locals.Add(expr, depth);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr">Binary expression.</param>
    /// <returns></returns>
    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        var lhs = Evaluate(expr.Left);
        var rhs = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.EqualEqual:
                return IsEqual(lhs, rhs);
            case TokenType.BangEqual:
                return !IsEqual(lhs, rhs);
            case TokenType.Greater:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return (double)lhs > (double)rhs;
            case TokenType.GreaterEqual:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return (double)lhs >= (double)rhs;
            case TokenType.Less:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return (double)lhs < (double)rhs;
            case TokenType.LessEqual:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return (double)lhs <= (double)rhs;
            case TokenType.Minus:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return (double)lhs - (double)rhs;
            case TokenType.Slash:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return (double)lhs / (double)rhs;
            case TokenType.Star:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return (double)lhs * (double)rhs;
            case TokenType.Plus:
                if (lhs is double lhsDouble && rhs is double rhsDouble)
                {
                    return lhsDouble + rhsDouble;
                }
                else if (lhs is string lhsString && rhs is string rhsString)
                {
                    return string.Concat(lhsString, rhsString);
                }

                throw new RuntimeError(expr.Operator,
                    $"Operands (lhs={lhs}, rhs={rhs}) must be two numbers or two strings.");
        }

        // Unreachable
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr">Call expression.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If the callee evaluates to <c>null</c>.</exception>
    public object? VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);
        if (callee is not ILoxCallable)
        {
            throw new RuntimeError(token: expr.ClosingParen, "Can only call functions and classes.");
        }

        var arguments = expr.Arguments.Select(arg => Evaluate(arg));
        var function = (ILoxCallable)callee;

        if (arguments.Count() != function.Arity())
        {
            throw new RuntimeError(
                token: expr.ClosingParen, $"Expected {function.Arity()} arguments but got {arguments.Count()}.");
        }

        return function?.Call(this, arguments);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr">Get expression.</param>
    /// <returns></returns>
    /// <exception cref="RuntimeError">If the expression does not evaluate to a class instance.</exception>
    public object? VisitGetExpr(Expr.Get expr)
    {
        var obj = Evaluate(expr.Object);
        if (obj is LoxInstance instance)
        {
            return instance.Get(expr.Name);
        }

        throw new RuntimeError(token: expr.Name, "Only instances have properties.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr">Grouping expression.</param>
    /// <returns></returns>
    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr">Literal expression.</param>
    /// <returns></returns>
    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Operator.Type == TokenType.Or)
        {
            if (IsTruthy(left))
            {
                return left;
            }
        }
        else
        {
            if (!IsTruthy(left))
            {
                return left;
            }
        }

        return Evaluate(expr.Right);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public object? VisitSetExpr(Expr.Set expr)
    {
        var obj = Evaluate(expr.Object);

        if (obj is not LoxInstance instance)
        {
            throw new RuntimeError(token: expr.Name, "Only instances have fields.");
        }

        var value = Evaluate(expr.Value);
        instance.Set(expr.Name, value);
        return value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public object? VisitSuperExpr(Expr.Super expr)
    {
        if (m_locals.TryGetValue(expr, out var distance))
        {
            var superclass = (LoxClass?)m_environment.GetAt(distance: distance, name: expr.Keyword.Lexeme);
            var loxObject = (LoxInstance?)m_environment.GetAt(distance: distance - 1, "this")
                ?? throw new RuntimeError(token: expr.Method, "Could not find 'this' of corresponding superclass.");
            var method = superclass?.FindMethod(name: expr.Method.Lexeme)
                ?? throw new RuntimeError(token: expr.Method, message: $"Undefined property '{expr.Method.Lexeme}'.");
            return method.Bind(loxObject);
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public object? VisitThisExpr(Expr.This expr) => LookUpVariable(name: expr.Keyword, expr: expr);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr">Unary expression.</param>
    /// <returns></returns>
    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        var rhs = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.Bang:
                return !IsTruthy(rhs);

            case TokenType.Minus:
                CheckNumberOperand(expr.Operator, rhs);
                return -(double)rhs;
        }

        // Unreachable
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public object? VisitVariableExpr(Expr.Variable expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    private object? LookUpVariable(Token name, Expr expr)
    {
        if (m_locals.TryGetValue(expr, out var distance))
        {
            return m_environment.GetAt(distance, name.Lexeme);
        }
        else
        {
            return Globals.Get(name);
        }
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    /// <summary>
    /// <c>false</c> and <c>nil</c> are falsey, everything else is truthy.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsTruthy(object? value)
    {
        if (value == null) return false;
        if (value is bool v) return v;
        return true;
    }

    private static bool IsEqual(object? lhs, object? rhs)
    {
        if (lhs == null && rhs == null) return true;
        if (lhs == null) return false;

        return lhs.Equals(rhs);
    }

    private static void CheckNumberOperand(Token op, [NotNull] object? operand)
    {
        if (operand is double) return;
        throw new RuntimeError(op, $"Operand ({operand}) must be a number.");
    }

    private static void CheckNumberOperands(Token op, [NotNull] object? lhs, [NotNull] object? rhs)
    {
        if (lhs is double && rhs is double) return;
        throw new RuntimeError(op, $"Operands (lhs={lhs}, rhs={rhs}) must be numbers.");
    }

    private static string Stringify(object? value)
    {
        if (value == null) return "nil";

        if (value is double doubleValue)
        {
            string text = doubleValue.ToString();
            if (text.EndsWith(".0"))
            {
                text = text[0..^2];
            }
            return text;
        }

        return value.ToString() ?? "";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns><c>null</c></returns>
    public object? VisitExpressionStatementStmt(Stmt.ExpressionStatement stmt)
    {
        _ = Evaluate(stmt.Expression);
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns><c>null</c></returns>
    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        var function = new LoxFunction(stmt, m_environment, isInitialiser: false);
        m_environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns><c>null</c></returns>
    public object? VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns><c>null</c></returns>
    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expression);
        Console.WriteLine(Stringify(value));
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns></returns>
    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        var value = stmt.Value == null ? null : Evaluate(stmt.Value);
        throw new Return(value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt">'while' statement to execute.</param>
    /// <returns><c>null</c></returns>
    public object? VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns><c>null</c></returns>
    public object? VisitVarStmt(Stmt.Var stmt)
    {
        var value = stmt.Initializer != null ? Evaluate(stmt.Initializer) : null;
        m_environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns>The value being assigned</returns>
    public object? VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);

        if (m_locals.TryGetValue(expr, out var distance))
        {
            m_environment.AssignAt(distance, expr.Name, value);
        }
        else
        {
            Globals.Assign(expr.Name, value);
        }

        return value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns><c>null</c></returns>
    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new LoxEnvironment(m_environment));
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns><c>null</c></returns>
    public object? VisitClassStmt(Stmt.Class stmt)
    {
        object? superclass = null;
        if (stmt.Superclass != null)
        {
            superclass = Evaluate(stmt.Superclass);

            if (superclass is not LoxClass)
            {
                throw new RuntimeError(stmt.Superclass.Name, "Superclass must be a class.");
            }
        }

        m_environment.Define(stmt.Name.Lexeme, value: null);

        if (stmt.Superclass != null)
        {
            m_environment = new LoxEnvironment(enclosing: m_environment);
            m_environment.Define(name: "super", superclass);
        }

        var methods = new Dictionary<string, LoxFunction>();
        foreach (var method in stmt.Methods)
        {
            methods[method.Name.Lexeme] = new LoxFunction(
                declaration: method,
                closure: m_environment,
                isInitialiser: method.Name.Lexeme == LoxClass.InitMethodName);
        }

        var loxClass = new LoxClass(name: stmt.Name.Lexeme, (LoxClass?)superclass, methods: methods);

        if (superclass != null && m_environment.Enclosing != null)
        {
            m_environment = m_environment.Enclosing;
        }

        m_environment.Assign(stmt.Name, loxClass);

        return null;
    }

    internal void ExecuteBlock(List<Stmt?> statements, LoxEnvironment environment)
    {
        var previousEnvironment = m_environment;

        try
        {
            m_environment = environment;
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            m_environment = previousEnvironment;
        }
    }
}
