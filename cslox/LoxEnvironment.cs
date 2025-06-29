namespace cslox;

/// <summary>
/// 
/// </summary>
public class LoxEnvironment
{
    internal readonly LoxEnvironment? Enclosing;
    private readonly Dictionary<string, object?> m_environment = [];

    internal LoxEnvironment(LoxEnvironment? enclosing = null)
    {
        Enclosing = enclosing;
    }

    internal void Define(string name, object? value)
    {
        m_environment[name] = value;
    }

    internal object? Get(Token name)
    {
        if (m_environment.TryGetValue(name.Lexeme, out object? value))
        {
            return value;
        }
        else if (Enclosing != null)
        {
            return Enclosing.Get(name);
        }
        else
        {
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }

    internal object? GetAt(int distance, string name)
    {
        return Ancestor(distance).m_environment.TryGetValue(name, out object? value) ? value : null;
    }

    private LoxEnvironment Ancestor(int distance)
    {
        var environment = this;
        for (var i = 0; i < distance; i++)
        {
            environment = environment?.Enclosing;
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
        else if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
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
