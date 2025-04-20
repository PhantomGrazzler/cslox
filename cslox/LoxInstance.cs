namespace cslox;

internal class LoxInstance
{
    private readonly LoxClass m_class;
    private readonly Dictionary<string, object?> m_fields = [];

    internal LoxInstance(LoxClass @class)
    {
        m_class = @class;
    }

    internal object? Get(Token name)
    {
        if (m_fields.TryGetValue(name.Lexeme, out var field))
        {
            return field;
        }

        var method = m_class.FindMethod(name.Lexeme);
        if (method != null) return method.Bind(this);

        throw new RuntimeError(token: name, $"Undefined property '{name.Lexeme}'.");
    }

    internal void Set(Token name, object? value)
    {
        m_fields[name.Lexeme] = value;
    }

    public override string ToString() => $"{m_class.Name} instance";
}
