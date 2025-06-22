namespace cslox;

internal class LoxFunction : ILoxCallable
{
    private readonly Stmt.Function m_declaration;
    private readonly LoxEnvironment m_closure;
    private readonly bool m_isInitialiser;

    internal LoxFunction(Stmt.Function declaration, LoxEnvironment closure, bool isInitialiser)
    {
        m_declaration = declaration;
        m_closure = closure;
        m_isInitialiser = isInitialiser;
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
            return m_isInitialiser ? m_closure.GetAt(distance: 0, name: "this") : returnValue.Value;
        }

        return m_isInitialiser ? m_closure.GetAt(distance: 0, name: "this") : null;
    }

    internal LoxFunction Bind(LoxInstance instance)
    {
        var environment = new LoxEnvironment(m_closure);
        environment.Define(name: "this", value: instance);
        return new LoxFunction(declaration: m_declaration, closure: environment, isInitialiser: m_isInitialiser);
    }

    public override string ToString() => $"<fn {m_declaration.Name.Lexeme}>";
}
