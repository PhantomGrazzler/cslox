namespace cslox;

internal class LoxClass
{
    internal readonly string Name;

    internal LoxClass(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
