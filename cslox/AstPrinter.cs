using System.Text;

namespace cslox;

/// <summary>
/// 
/// </summary>
public class AstPrinter : Expr.IVisitor<string>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitLiteralExpr(Expr.Literal expr) => expr.Value.ToString() ?? "nil";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var builder = new StringBuilder();

        builder.Append('(').Append(name);
        foreach (var expr in exprs)
        {
            builder.Append(' ').Append(expr.Accept(this));
        }
        builder.Append(')');

        return builder.ToString();
    }
}
