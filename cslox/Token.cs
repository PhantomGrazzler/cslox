namespace cslox
{
    internal class Token
    {
        private readonly TokenType m_type;
        private readonly string m_lexeme;
        private readonly object m_literal;
        private readonly int m_line;

        internal Token(TokenType token_type, string lexeme, object literal, int line)
        {
            m_type = token_type;
            m_lexeme = lexeme;
            m_literal = literal;
            m_line = line;
        }

        public override string ToString()
        {
            return m_type.ToString() + " " + m_lexeme + " " + m_literal;
        }
    }
}
