namespace cslox.test;

public class ScannerTests
{
    [Fact]
    public void NoTokens()
    {
        var scanner = new Scanner(string.Empty);
        var tokens = scanner.ScanTokens();

        Assert.Single(tokens);
        Assert.Equal(TokenType.Eof, tokens.First().Type);
    }

    [Theory]
    [InlineData("(", TokenType.LeftParen)]
    [InlineData(")", TokenType.RightParen)]
    [InlineData("{", TokenType.LeftBrace)]
    [InlineData("}", TokenType.RightBrace)]
    [InlineData(",", TokenType.Comma)]
    [InlineData(".", TokenType.Dot)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("+", TokenType.Plus)]
    [InlineData(";", TokenType.Semicolon)]
    [InlineData("/", TokenType.Slash)]
    [InlineData("*", TokenType.Star)]
    public void SingleCharacterToken(string token, TokenType expectedType)
    {
        var scanner = new Scanner(token);
        var tokens = scanner.ScanTokens();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens.First().Type);
    }

    [Theory]
    [InlineData("!", TokenType.Bang)]
    [InlineData("!=", TokenType.BangEqual)]
    [InlineData("=", TokenType.Equal)]
    [InlineData("==", TokenType.EqualEqual)]
    [InlineData(">", TokenType.Greater)]
    [InlineData(">=", TokenType.GreaterEqual)]
    [InlineData("<", TokenType.Less)]
    [InlineData("<=", TokenType.LessEqual)]
    public void OneOrTwoCharacterTokens(string token, TokenType expectedType)
    {
        var scanner = new Scanner(token);
        var tokens = scanner.ScanTokens();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens.First().Type);
    }

    [Theory]
    [InlineData("\"my string\"", TokenType.String)]
    [InlineData("5", TokenType.Number)]
    [InlineData("5.24", TokenType.Number)]
    [InlineData("my_identifier", TokenType.Identifier)]
    public void Literals(string token, TokenType expectedType)
    {
        var scanner = new Scanner(token);
        var tokens = scanner.ScanTokens();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens.First().Type);
    }

    [Theory]
    [InlineData("and", TokenType.And)]
    [InlineData("class", TokenType.Class)]
    [InlineData("else", TokenType.Else)]
    [InlineData("false", TokenType.False)]
    [InlineData("fun", TokenType.Fun)]
    [InlineData("for", TokenType.For)]
    [InlineData("if", TokenType.If)]
    [InlineData("nil", TokenType.Nil)]
    [InlineData("or", TokenType.Or)]
    [InlineData("print", TokenType.Print)]
    [InlineData("return", TokenType.Return)]
    [InlineData("super", TokenType.Super)]
    [InlineData("this", TokenType.This)]
    [InlineData("true", TokenType.True)]
    [InlineData("var", TokenType.Var)]
    [InlineData("while", TokenType.While)]
    public void Keywords(string token, TokenType expectedType)
    {
        var scanner = new Scanner(token);
        var tokens = scanner.ScanTokens();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens.First().Type);
    }
}
