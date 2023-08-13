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

    [InlineData("nil", "==", "true", null, true)]
    [InlineData("false", "!=", "7", false, 7.0)]
    [InlineData("\"false\"", "!=", "85.349", "false", 85.349)]
    [Theory]
    public void EqualityExpressions(
        string lhsString, string op, string rhsString, object? lhsExpected, object? rhsExpected)
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
    }
}
