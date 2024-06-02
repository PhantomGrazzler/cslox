namespace cslox;

internal interface ILoxCallable
{
    int Arity();
    object? Call(Interpreter interpreter, IEnumerable<object?> arguments);
}
