namespace cslox.test;

public class ParserTests
{
    private static Expr? Parse(string source)
    {
        var tokens = new Scanner(source).ScanTokens();
        return new Parser(tokens).Parse();
    }

    [InlineData("")]
    [InlineData(";")]
    [InlineData(" ")]
    [InlineData("var")]
    [InlineData("(1")]
    [InlineData("1+")]
    [InlineData("2-")]
    [InlineData("*3")]
    [InlineData("/4")]
    [InlineData("var a = 1;")]
    [Theory]
    public void InvalidExpressions(string source)
    {
        Assert.Null(Parse(source));
    }

    [InlineData("nil", null)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("0", 0.0)]
    [InlineData("5", 5.0)]
    [InlineData("0.123", 0.123)]
    [InlineData("\"string with spaces\"", "string with spaces")]
    [InlineData("\"1.23\"", "1.23")]
    [Theory]
    public void ValidLiterals(string source, object? expectedValue)
    {
        var expression = Parse(source);
        Assert.NotNull(expression);

        var literal = expression as Expr.Literal;
        Assert.NotNull(literal);
        Assert.Equal(expectedValue, literal.Value);
    }

    [InlineData("nil", "==", "true", null, TokenType.EqualEqual, true)]
    [InlineData("false", "!=", "7", false, TokenType.BangEqual, 7.0)]
    [InlineData("\"false\"", "!=", "85.349", "false", TokenType.BangEqual, 85.349)]
    [InlineData("1", ">", "0", 1.0, TokenType.Greater, 0.0)]
    [InlineData("\"some string\"", ">=", "nil", "some string", TokenType.GreaterEqual, null)]
    [InlineData("true", "<", "false", true, TokenType.Less, false)]
    [InlineData("false", "<=", "true", false, TokenType.LessEqual, true)]
    [InlineData("3", "-", "4", 3.0, TokenType.Minus, 4.0)]
    [InlineData("5", "+", "6", 5.0, TokenType.Plus, 6.0)]
    [InlineData("8", "/", "9", 8.0, TokenType.Slash, 9.0)]
    [InlineData("10", "*", "11", 10.0, TokenType.Star, 11.0)]
    [Theory]
    public void BinaryExpressions(
        string lhsString, string op, string rhsString, object? lhsExpected, TokenType opExpected, object? rhsExpected)
    {
        var expression = Parse($"{lhsString} {op} {rhsString}");
        Assert.NotNull(expression);

        var equality = expression as Expr.Binary;
        Assert.NotNull(equality);

        var lhsValue = equality.Left as Expr.Literal;
        Assert.NotNull(lhsValue);
        Assert.Equal(lhsExpected, lhsValue.Value);

        var rhsValue = equality.Right as Expr.Literal;
        Assert.NotNull(rhsValue);
        Assert.Equal(rhsExpected, rhsValue.Value);

        var opToken = equality.Operator;
        Assert.NotNull(opToken);
        Assert.Equal(op, opToken.Lexeme);
        Assert.Equal(opExpected, opToken.Type);
    }

    [InlineData("!", "true", TokenType.Bang, true)]
    [InlineData("-", "nil", TokenType.Minus, null)]
    [Theory]
    public void UnaryExpressions(string op, string rhsString, TokenType opExpected, object? rhsExpected)
    {
        var expression = Parse($"{op}{rhsString}");
        Assert.NotNull(expression);

        var unary = expression as Expr.Unary;
        Assert.NotNull(unary);

        var opToken = unary.Operator;
        Assert.NotNull(opToken);
        Assert.Equal(op, opToken.Lexeme);
        Assert.Equal(opExpected, opToken.Type);

        var rhsValue = unary.Right as Expr.Literal;
        Assert.NotNull(rhsValue);
        Assert.Equal(rhsExpected, rhsValue.Value);
    }

    [Fact]
    public void GroupingExpression()
    {
        var expression = Parse("(75)");
        Assert.NotNull(expression);

        var grouping = expression as Expr.Grouping;
        Assert.NotNull(grouping);

        var literal = grouping.Expression as Expr.Literal;
        Assert.NotNull(literal);
        Assert.Equal(75.0, literal.Value);
    }

    [Fact]
    public void NestedGroupingExpression()
    {
        var expression = Parse("((true))");
        Assert.NotNull(expression);

        var outerGrouping = expression as Expr.Grouping;
        Assert.NotNull(outerGrouping);

        var innerGrouping = outerGrouping.Expression as Expr.Grouping;
        Assert.NotNull(innerGrouping);

        var literal = innerGrouping.Expression as Expr.Literal;
        Assert.NotNull(literal);
        Assert.Equal(true, literal.Value);
    }
}
