namespace cslox;

internal class Resolver : Expr.IVisitor<object?>
                        , Stmt.IVisitor<object?>
{
    private enum Status
    {
        Uninitialised,
        Initialised,
    }

    private readonly Interpreter m_interpreter;
    /// <summary>
    /// Dictionary maps variable name (lexeme) to its initialisation status.
    /// </summary>
    private readonly Stack<Dictionary<string, Status>> m_scopes = new();

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

    private void BeginScope() => m_scopes.Push(new());
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
        expr.Arguments.ForEach(arg => Resolve(arg));
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
        ResolveFunction(stmt);
        return null;
    }

    private void ResolveFunction(Stmt.Function function)
    {
        BeginScope();
        function.Params.ForEach(param =>
        {
            Declare(param);
            Define(param);
        });
        Resolve(function.Body);
        EndScope();
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

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expression);
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (stmt.Value != null)
        {
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
            scope.ContainsKey(expr.Name.Lexeme) &&
            scope[expr.Name.Lexeme] == Status.Uninitialised)
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
            scope.Add(name.Lexeme, Status.Uninitialised);
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
