namespace cslox.test;

public class AstPrinterTests
{
    private readonly AstPrinter m_astPrinter = new();

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

        Assert.Equal("(* (- 123) (group 45.67))", m_astPrinter.Print(expression));
    }

    [Fact]
    public void VariableDeclaration()
    {
        var expression = new Expr.Assign(
            new Token(TokenType.Identifier, "var_name", new(), 1),
            new Expr.Unary(
                new Token(TokenType.Minus, "-", new(), 1),
                new Expr.Literal(123)));

        Assert.Equal("var_name = (- 123)", m_astPrinter.Print(expression));
    }

    [Fact]
    public void AssignmentExpression()
    {
        var expression = new Expr.Variable(new Token(TokenType.Identifier, "var_name", new(), 1));

        Assert.Equal("var_name", m_astPrinter.Print(expression));
    }

    [Fact]
    public void LogicalExpressions()
    {
        var expression = new Expr.Logical(
            new Expr.Literal(8),
            new Token(TokenType.Or, "or", new(), 1),
            new Expr.Logical(
                new Expr.Literal("hi"),
                new Token(TokenType.And, "and", new(), 1),
                new Expr.Literal(9)));

        Assert.Equal("(or 8 (and hi 9))", m_astPrinter.Print(expression));
    }

    [Fact]
    public void CallExpression()
    {
        var expression = new Expr.Call(
            Callee: new Expr.Literal("function"),
            ClosingParen: new Token(TokenType.RightParen, ")", new(), 1),
            Arguments: new()
            {
                new Expr.Literal(65),
                new Expr.Variable(new Token(TokenType.Identifier, "variable", new(), 1))
            });

        Assert.Equal("call function (arguments: 65 variable)", m_astPrinter.Print(expression));
    }
}
