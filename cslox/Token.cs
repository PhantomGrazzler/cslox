namespace cslox;

/// <summary>
/// TODO
/// </summary>
public class Token
{
    private readonly object m_literal;
    private readonly int m_line;

    /// <summary>
    /// TODO
    /// </summary>
    public readonly TokenType Type;

    /// <summary>
    /// TODO
    /// </summary>
    public readonly string Lexeme;

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="token_type"></param>
    /// <param name="lexeme"></param>
    /// <param name="literal"></param>
    /// <param name="line"></param>
    public Token(TokenType token_type, string lexeme, object literal, int line)
    {
        Type = token_type;
        Lexeme = lexeme;
        m_literal = literal;
        m_line = line;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Type} {Lexeme} {m_literal}";
    }
}
