
namespace cslox;

internal class LoxClass : ILoxCallable
{
    internal readonly string Name;

    internal LoxClass(string name)
    {
        Name = name;
    }

    public int Arity() => 0;

    public object? Call(Interpreter interpreter, IEnumerable<object?> arguments)
    {
        return new LoxInstance(this);
    }

    public override string ToString() => Name;
}
