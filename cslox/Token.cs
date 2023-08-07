namespace cslox;

/// <summary>
/// 
/// </summary>
/// <param name="Type"></param>
/// <param name="Lexeme"></param>
/// <param name="Literal"></param>
/// <param name="Line"></param>
public record Token(TokenType Type, string Lexeme, object Literal, int Line)
{
    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Type} {Lexeme} {Literal} (line {Line})";
    }
}
