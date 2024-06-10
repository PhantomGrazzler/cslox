namespace cslox.test;

public class ScannerTests
{
    private static List<Token> ScanTokens(string tokens)
    {
        var scanner = new Scanner(tokens);
        return scanner.ScanTokens();
    }

    [Fact]
    public void NoTokens()
    {
        var tokens = ScanTokens(string.Empty);

        var firstToken = Assert.Single(tokens);
        Assert.Equal(TokenType.Eof, firstToken.Type);
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
        var tokens = ScanTokens(token);

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
        var tokens = ScanTokens(token);

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
        var tokens = ScanTokens(token);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens.First().Type);
    }

    [Fact]
    public void UnterminatedString()
    {
        var tokens = ScanTokens("\"unterminated string");

        var firstToken = Assert.Single(tokens);
        Assert.Equal(TokenType.Eof, firstToken.Type);
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
        var tokens = ScanTokens(token);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens.First().Type);
    }

    [Theory]
    [InlineData("And")]
    [InlineData("cLass")]
    [InlineData("elSe")]
    [InlineData("falsE")]
    [InlineData("FUN")]
    [InlineData("FoR")]
    [InlineData("iF")]
    [InlineData("nil_")]
    [InlineData("_or")]
    [InlineData("print1")]
    [InlineData("return2")]
    [InlineData("sup3r")]
    public void AlmostKeywords(string token)
    {
        var tokens = ScanTokens(token);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens.First().Type);
    }

    [Theory]
    [InlineData("@")]
    [InlineData("#")]
    [InlineData("$")]
    [InlineData("%")]
    [InlineData("^")]
    [InlineData("&")]
    [InlineData("`")]
    [InlineData("~")]
    [InlineData(@"\")]
    [InlineData("|")]
    [InlineData("?")]
    [InlineData(":")]
    [InlineData("'")]
    [InlineData("[")]
    [InlineData("]")]
    public void InvalidCharacters(string invalidCharacter)
    {
        var tokens = ScanTokens(invalidCharacter);

        var firstToken = Assert.Single(tokens);
        Assert.Equal(TokenType.Eof, firstToken.Type);
    }

    [Fact]
    public void Comments()
    {
        var input = @"// First line comment
// Second line comment!! //
// fun hello() { print(hello); }";

        var tokens = ScanTokens(input);

        var firstToken = Assert.Single(tokens);
        Assert.Equal(TokenType.Eof, firstToken.Type);
    }

    [Fact]
    public void MultilineString()
    {
        var multilineString = "\"first line\nsecond line\"";
        var tokens = ScanTokens(multilineString);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.String, tokens.First().Type);
    }

    [Fact]
    public void MultilineInput()
    {
        var multilineInput = "var count = 1;\n" + // 5 tokens
            "{ print nil; }\n" + // 5 tokens
            "print -1;"; // 4 tokens + EOF token
        var tokens = ScanTokens(multilineInput);

        Assert.Equal(15, tokens.Count);
    }
}
