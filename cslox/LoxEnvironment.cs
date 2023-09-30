namespace cslox;

internal class LoxEnvironment
{
    private readonly Dictionary<string, object?> m_environment = new();

    internal void Define(string name, object? value)
    {
        m_environment[name] = value;
    }

    internal object? Get(Token name)
    {
        if(m_environment.ContainsKey(name.Lexeme))
        {
            return m_environment[name.Lexeme];
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    internal void Assign(Token name, object? value)
    {
        if (m_environment.ContainsKey(name.Lexeme))
        {
            m_environment[name.Lexeme] = value;
        }
        else
        {
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
