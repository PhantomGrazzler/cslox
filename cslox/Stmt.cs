// File generated by generate_ast on 08-Jan-2024 22:58:11 +11:00
#pragma warning disable 1591

namespace cslox;

public abstract record Stmt
{
    public interface IVisitor<R>
    {
        R VisitBlockStmt(Block stmt);
        R VisitExpressionStatementStmt(ExpressionStatement stmt);
        R VisitIfStmt(If stmt);
        R VisitPrintStmt(Print stmt);
        R VisitVarStmt(Var stmt);
    }

    public abstract R Accept<R>(IVisitor<R> visitor);

    public record Block(List<Stmt?> Statements) : Stmt
    {
        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitBlockStmt(this);
        }
    }

    public record ExpressionStatement(Expr Expression) : Stmt
    {
        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitExpressionStatementStmt(this);
        }
    }

    public record If(Expr Condition, Stmt ThenBranch, Stmt? ElseBranch) : Stmt
    {
        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitIfStmt(this);
        }
    }

    public record Print(Expr Expression) : Stmt
    {
        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitPrintStmt(this);
        }
    }

    public record Var(Token Name, Expr? Initializer) : Stmt
    {
        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitVarStmt(this);
        }
    }

}

#pragma warning restore 1591
