
namespace cslox;

internal class LoxClass : ILoxCallable
{
    internal readonly string Name;
    private readonly Dictionary<string, LoxFunction> m_methods;

    internal LoxClass(string name, Dictionary<string, LoxFunction> methods)
    {
        Name = name;
        m_methods = methods;
    }

    public int Arity() => 0;

    public object? Call(Interpreter interpreter, IEnumerable<object?> arguments)
    {
        return new LoxInstance(this);
    }

    internal LoxFunction? FindMethod(string name)
    {
        if (m_methods.TryGetValue(name, out var method))
        {
            return method;
        }

        return null;
    }

    public override string ToString() => Name;
}
