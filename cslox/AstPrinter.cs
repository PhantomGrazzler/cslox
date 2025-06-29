using System.Text;

namespace cslox;

/// <summary>
/// 
/// </summary>
public sealed class AstPrinter : Expr.IVisitor<string>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string Print(Expr expr) => expr.Accept(this);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitAssignExpr(Expr.Assign expr) => $"{expr.Name.Lexeme} = {expr.Value.Accept(this)}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitBinaryExpr(Expr.Binary expr) => Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitCallExpr(Expr.Call expr) =>
        $"call {expr.Callee.Accept(this)} {Parenthesize(name: "arguments:", [.. expr.Arguments])}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitGetExpr(Expr.Get expr) => $"{expr.Object.Accept(this)}.{expr.Name.Lexeme}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitGroupingExpr(Expr.Grouping expr) => Parenthesize("group", expr.Expression);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitLiteralExpr(Expr.Literal expr) => expr.Value?.ToString() ?? "nil";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitLogicalExpr(Expr.Logical expr) => Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitSetExpr(Expr.Set expr) =>
        Parenthesize(name: "=", new Expr.Get(expr.Object, expr.Name), expr.Value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitSuperExpr(Expr.Super expr) => $"{expr.Keyword.Lexeme}.{expr.Method.Lexeme}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitThisExpr(Expr.This expr) => $"{expr.Keyword.Lexeme}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitUnaryExpr(Expr.Unary expr) => Parenthesize(expr.Operator.Lexeme, expr.Right);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public string VisitVariableExpr(Expr.Variable expr) => $"{expr.Name.Lexeme}";

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var builder = new StringBuilder();

        _ = builder.Append('(').Append(name);
        foreach (var expr in exprs)
        {
            _ = builder.Append(' ').Append(expr.Accept(this));
        }
        _ = builder.Append(')');

        return builder.ToString();
    }
}
