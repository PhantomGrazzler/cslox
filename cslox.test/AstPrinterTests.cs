namespace cslox.test;

public class AstPrinterTests
{
    [Fact]
    public void SimpleExpression()
    {
        var expression = new Expr.Binary(
            new Expr.Unary(
                new Token(TokenType.Minus, "-", new(), 1),
                new Expr.Literal(123)),
            new Token(TokenType.Star, "*", new(), 1),
            new Expr.Grouping(
                new Expr.Literal(45.67))
            );

        Assert.Equal("(* (- 123) (group 45.67))", new AstPrinter().Print(expression));
    }
}
