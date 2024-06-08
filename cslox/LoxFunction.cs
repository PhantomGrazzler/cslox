namespace cslox;

internal class LoxFunction : ILoxCallable
{
    private readonly Stmt.Function m_declaration;
    private readonly LoxEnvironment m_closure;

    internal LoxFunction(Stmt.Function declaration, LoxEnvironment closure)
    {
        m_declaration = declaration;
        m_closure = closure;
    }

    public int Arity() => m_declaration.Params.Count;

    public object? Call(Interpreter interpreter, IEnumerable<object?> arguments)
    {
        var environment = new LoxEnvironment(m_closure);

        foreach (var (argument, parameter) in arguments.Zip(m_declaration.Params))
        {
            environment.Define(name: parameter.Lexeme, value: argument);
        }

        try
        {
            interpreter.ExecuteBlock(m_declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.Value;
        }

        return null;
    }

    public override string ToString() => $"<fn {m_declaration.Name.Lexeme}>";
}
