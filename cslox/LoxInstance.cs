namespace cslox;

internal class LoxInstance
{
    private readonly LoxClass m_class;

    internal LoxInstance(LoxClass @class)
    {
        m_class = @class;
    }

    public override string ToString() => $"{m_class.Name} instance";
}
