using cslox;

internal class Program
{
    private const string CompletedSection = "6.4";

    private static void Main(string[] args)
    {
        Console.WriteLine($"PhantomGrazzler's Lox interpreter (completed up to section {CompletedSection})\n");

        if (args.Length > 1)
        {
            Console.WriteLine("Usage: cslox.exe [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            Lox.RunFile(args[0]);
        }
        else
        {
            Lox.RunPrompt();
        }
    }
}
