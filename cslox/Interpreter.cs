﻿using System.Diagnostics.CodeAnalysis;

namespace cslox;

/// <summary>
/// 
/// </summary>
public sealed class RuntimeError : Exception
{
    /// <summary>
    /// 
    /// </summary>
    public Token Token { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <param name="message"></param>
    public RuntimeError(Token token, string message) : base(message)
    {
        Token = token;
    }
}

/// <summary>
/// 
/// </summary>
public class Interpreter : Expr.IVisitor<object?>
                         , Stmt.IVisitor<object?>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="statements"></param>
    public void Interpret(IEnumerable<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        catch(RuntimeError e)
        {
            Lox.RuntimeError(e);
        }
    }

    private void Execute(Stmt statement)
    {
        _ = statement.Accept(this);
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

        switch(expr.Operator.Type)
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
                if(lhs is double lhsDouble && rhs is double rhsDouble)
                {
                    return lhsDouble + rhsDouble;
                }
                else if(lhs is string lhsString && rhs is string rhsString)
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
    /// <param name="expr">Unary expression.</param>
    /// <returns></returns>
    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        var rhs = Evaluate(expr.Right);

        switch(expr.Operator.Type)
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
        if(lhs is double && rhs is double) return;
        throw new RuntimeError(op, $"Operands (lhs={lhs}, rhs={rhs}) must be numbers.");
    }

    private static string Stringify(object? value)
    {
        if (value == null) return "nil";

        if(value is double doubleValue)
        {
            string text = doubleValue.ToString();
            if(text.EndsWith(".0"))
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
    /// <returns></returns>
    public object? VisitExpressionStatementStmt(Stmt.ExpressionStatement stmt)
    {
        _ = Evaluate(stmt.Expression);
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns></returns>
    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expression);
        Console.WriteLine(Stringify(value));
        return null;
    }
}