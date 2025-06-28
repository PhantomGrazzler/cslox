
namespace cslox;

internal class LoxClass : ILoxCallable
{
    internal static readonly string InitMethodName = "init";
    internal readonly string Name;
    internal readonly LoxClass? Superclass;
    private readonly Dictionary<string, LoxFunction> m_methods;

    internal LoxClass(string name, LoxClass? superclass, Dictionary<string, LoxFunction> methods)
    {
        Name = name;
        Superclass = superclass;
        m_methods = methods;
    }

    public int Arity()
    {
        var initialiser = FindMethod(InitMethodName);
        return initialiser == null ? 0 : initialiser.Arity();
    }

    public object? Call(Interpreter interpreter, IEnumerable<object?> arguments)
    {
        var instance = new LoxInstance(this);
        var initialiser = FindMethod(InitMethodName);
        _ = initialiser?.Bind(instance).Call(interpreter, arguments);
        return instance;
    }

    internal LoxFunction? FindMethod(string name)
    {
        if (m_methods.TryGetValue(name, out var method))
        {
            return method;
        }

        if (Superclass != null)
        {
            return Superclass.FindMethod(name);
        }

        return null;
    }

    public override string ToString() => Name;
}
