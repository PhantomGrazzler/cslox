namespace cslox;

/// <summary>
/// 
/// </summary>
public class LoxEnvironment
{
    private readonly LoxEnvironment? m_enclosing;
    private readonly Dictionary<string, object?> m_environment = new();

    internal LoxEnvironment(LoxEnvironment? enclosing = null)
    {
        m_enclosing = enclosing;
    }

    internal void Define(string name, object? value)
    {
        m_environment[name] = value;
    }

    internal object? Get(Token name)
    {
        if (m_environment.ContainsKey(name.Lexeme))
        {
            return m_environment[name.Lexeme];
        }
        else if (m_enclosing != null)
        {
            return m_enclosing.Get(name);
        }
        else
        {
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }

    internal object? GetAt(int distance, Token name)
    {
        return Ancestor(distance).Get(name);
    }

    private LoxEnvironment Ancestor(int distance)
    {
        var environment = this;
        for (var i = 0; i < distance; i++)
        {
            environment = environment?.m_enclosing;
        }

        if (environment == null)
        {
            throw new NullReferenceException(
                $"{nameof(LoxEnvironment)}.{nameof(Ancestor)}: environment at depth {distance} is null.");
        }

        return environment;
    }

    internal void Assign(Token name, object? value)
    {
        if (m_environment.ContainsKey(name.Lexeme))
        {
            m_environment[name.Lexeme] = value;
        }
        else if (m_enclosing != null)
        {
            m_enclosing.Assign(name, value);
        }
        else
        {
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }

    internal void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance).Assign(name, value);
    }
}
