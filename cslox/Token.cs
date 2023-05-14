namespace cslox;

/// <summary>
/// TODO
/// </summary>
public class Token
{
    private readonly string m_lexeme;
    private readonly object m_literal;
    private readonly int m_line;

    /// <summary>
    /// TODO
    /// </summary>
    public readonly TokenType Type;

    internal Token(TokenType token_type, string lexeme, object literal, int line)
    {
        Type = token_type;
        m_lexeme = lexeme;
        m_literal = literal;
        m_line = line;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Type} {m_lexeme} {m_literal}";
    }
}
