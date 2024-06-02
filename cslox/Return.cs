namespace cslox;

internal class Return : Exception
{
    internal object? Value { get; } = null;

    internal Return(object? value)
    {
        Value = value;
    }
}
