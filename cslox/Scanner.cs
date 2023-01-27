namespace cslox;

internal class Scanner
{
    private readonly string m_source;
    private readonly List<Token> m_tokens = new();
    private int m_start = 0;
    private int m_current = 0;
    private int m_line = 1;
    private readonly Dictionary<string, TokenType> m_keywords = new()
    {
        {"and",     TokenType.And},
        {"class",   TokenType.Class},
        {"else",    TokenType.Else},
        {"false",   TokenType.False},
        {"for",     TokenType.For},
        {"fun",     TokenType.Fun},
        {"if",      TokenType.If},
        {"nil",     TokenType.Nil},
        {"or",      TokenType.Or},
        {"print",   TokenType.Print},
        {"return",  TokenType.Return},
        {"super",   TokenType.Super},
        {"this",    TokenType.This},
        {"true",    TokenType.True},
        {"var",     TokenType.Var},
        {"while",   TokenType.While},
    };

    internal Scanner(string source)
    {
        m_source = source;
    }

    internal List<Token> ScanTokens()
    {
        while(!IsAtEnd())
        {
            // We are at the beginning of the next lexeme.
            m_start = m_current;
            ScanToken();
        }

        m_tokens.Add(new Token(TokenType.Eof, lexeme: "", literal: new object(), m_line));
        return m_tokens;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch(c)
        {
            case '(': AddToken(TokenType.LeftParen); break;
            case ')': AddToken(TokenType.RightParen); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case ',': AddToken(TokenType.Comma); break;
            case '.': AddToken(TokenType.Dot); break;
            case '-': AddToken(TokenType.Minus); break;
            case '+': AddToken(TokenType.Plus); break;
            case ';': AddToken(TokenType.Semicolon); break;
            case '*': AddToken(TokenType.Star); break;

            case '!': AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang); break;
            case '=': AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal); break;
            case '<': AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less); break;
            case '>': AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;

            case '/':
                if(Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(TokenType.Slash);
                }
                break;

            case '"': String(); break;

            // Ignore whitespace
            case ' ':
            case '\r':
            case '\t':
                break;

            case '\n':
                ++m_line;
                break;

            default: 
                if(IsDigit(c))
                {
                    Number();
                }
                else if(IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Lox.Error(m_line, $"Unexpected character: {c}"); 
                }
                break;
        };
    }

    private void Identifier()
    {
        while(IsAlphaNumeric(Peek())) Advance();

        var text = m_source[m_start..m_current];
        var type = m_keywords.GetValueOrDefault(text, TokenType.Identifier);
        AddToken(type);
    }

    private void Number()
    {
        while (IsDigit(Peek())) Advance();

        // Look for a fractional part.
        if(Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance(); // Consume the "."

            while (IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.Number, double.Parse(m_source[m_start..m_current]));
    }

    private char PeekNext()
    {
        if (m_current + 1 >= m_source.Length) return '\0';
        else return m_source[m_current + 1];
    }

    private static bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private static bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private static bool IsAlphaNumeric(char c)
    {
        return IsDigit(c) || IsAlpha(c);
    }

    private void String()
    {
        while(Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') ++m_line;
            Advance();
        }

        if(IsAtEnd())
        {
            Lox.Error(m_line, "Unterminated string.");
            return;
        }

        Advance(); // The closing ".

        // Trim the surrounding quotes.
        AddToken(TokenType.String, m_source[(m_start + 1)..(m_current - 1)]);
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        else return m_source[m_current];
    }

    private char Advance()
    {
        return m_source[m_current++];
    }

    private bool Match(char expected)
    {
        if(IsAtEnd()) return false;
        if(m_source[m_current] != expected) return false;

        ++m_current;
        return true;
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, literal: new object());
    }

    private void AddToken(TokenType type, object literal)
    {
        var text = m_source[m_start..m_current];
        m_tokens.Add(new Token(type, text, literal, m_line));
    }

    private bool IsAtEnd()
    {
        return m_current >= m_source.Length;
    }
}
