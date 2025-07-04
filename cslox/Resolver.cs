﻿namespace cslox;

internal class Resolver : Expr.IVisitor<object?>
                        , Stmt.IVisitor<object?>
{
    private enum Status
    {
        Uninitialised,
        Initialised,
    }

    private enum FunctionType
    {
        None,
        Function,
        Initialiser,
        Method,
    }

    private enum ClassType
    {
        None,
        Class,
        Subclass,
    }

    private readonly Interpreter m_interpreter;
    /// <summary>
    /// Dictionary maps variable name (lexeme) to its initialisation status.
    /// </summary>
    private readonly Stack<Dictionary<string, Status>> m_scopes = new();
    private FunctionType m_currentFunction = FunctionType.None;
    private ClassType m_currentClass = ClassType.None;

    internal Resolver(Interpreter interpreter)
    {
        m_interpreter = interpreter;
    }

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();

        return null;
    }

    public object? VisitClassStmt(Stmt.Class stmt)
    {
        var enclosingClass = m_currentClass;
        m_currentClass = ClassType.Class;

        Declare(stmt.Name);
        Define(stmt.Name);

        if (stmt.Superclass != null)
        {
            if (stmt.Name.Lexeme == stmt.Superclass.Name.Lexeme)
            {
                Lox.Error(stmt.Superclass.Name, "A class cannot inherit from itself.");
            }

            m_currentClass = ClassType.Subclass;
            Resolve(stmt.Superclass);
            BeginScope();
            m_scopes.Peek().Add("super", Status.Initialised);
        }

        BeginScope();
        m_scopes.Peek().Add("this", Status.Initialised);

        foreach (var method in stmt.Methods)
        {
            var declaration = method.Name.Lexeme == LoxClass.InitMethodName
                ? FunctionType.Initialiser
                : FunctionType.Method;
            ResolveFunction(method, declaration);
        }

        EndScope();
        if (stmt.Superclass != null)
        {
            EndScope();
        }
        m_currentClass = enclosingClass;

        return null;
    }

    private void BeginScope() => m_scopes.Push([]);
    private void EndScope() => m_scopes.Pop();

    internal void Resolve(List<Stmt?> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Stmt? statement) => _ = statement?.Accept(this);
    private void Resolve(Expr? expression) => _ = expression?.Accept(this);

    public object? VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);
        expr.Arguments.ForEach(Resolve);
        return null;
    }

    public object? VisitGetExpr(Expr.Get expr)
    {
        Resolve(expr.Object);
        return null;
    }

    public object? VisitExpressionStatementStmt(Stmt.ExpressionStatement stmt)
    {
        Resolve(stmt.Expression);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        ResolveFunction(stmt, FunctionType.Function);
        return null;
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        var enclosingFunction = m_currentFunction;
        m_currentFunction = type;

        BeginScope();
        function.Params.ForEach(param =>
        {
            Declare(param);
            Define(param);
        });
        Resolve(function.Body);
        EndScope();

        m_currentFunction = enclosingFunction;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch != null)
        {
            Resolve(stmt.ElseBranch);
        }
        return null;
    }

    public object? VisitLiteralExpr(Expr.Literal expr) => null;

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitSetExpr(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Object);
        return null;
    }

    public object? VisitSuperExpr(Expr.Super expr)
    {
        switch (m_currentClass)
        {
            case ClassType.None:
                Lox.Error(token: expr.Keyword, "Cannot use 'super' outside of a class.");
                break;

            case ClassType.Class:
                Lox.Error(token: expr.Keyword, "Cannot use 'super' in a class with no superclass.");
                break;

            case ClassType.Subclass:
                ResolveLocal(expr: expr, name: expr.Keyword);
                break;
        }

        return null;
    }

    public object? VisitThisExpr(Expr.This expr)
    {
        if (m_currentClass == ClassType.None)
        {
            Lox.Error(expr.Keyword, $"Cannot use '{expr.Keyword.Lexeme}' outside of a class.");
            return null;
        }

        ResolveLocal(expr: expr, name: expr.Keyword);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expression);
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (m_currentFunction == FunctionType.None)
        {
            Lox.Error(stmt.Keyword, "Cannot return from top-level code.");
        }

        if (stmt.Value != null)
        {
            if (m_currentFunction == FunctionType.Initialiser)
            {
                Lox.Error(stmt.Keyword, "Cannot return a value from an initialiser.");
            }

            Resolve(stmt.Value);
        }

        return null;
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        if (m_scopes.TryPeek(out var scope) &&
            scope.TryGetValue(expr.Name.Lexeme, out Status value) &&
            value == Status.Uninitialised)
        {
            Lox.Error(token: expr.Name, message: "Cannot read local variable in its own initialiser.");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        var nestingLevel = m_scopes.Count - 1;
        foreach (var scope in m_scopes)
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                m_interpreter.Resolve(expr, m_scopes.Count - 1 - nestingLevel);
                return;
            }

            nestingLevel--;
        }
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
        {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);

        return null;
    }

    private void Declare(Token name)
    {
        if (m_scopes.TryPeek(out var scope))
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                Lox.Error(name, "Already a variable with this name in this scope.");
            }

            scope[name.Lexeme] = Status.Uninitialised;
        }
    }

    private void Define(Token name)
    {
        if (m_scopes.TryPeek(out var scope))
        {
            scope[name.Lexeme] = Status.Initialised;
        }
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }
}
