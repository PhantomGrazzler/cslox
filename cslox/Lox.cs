namespace cslox;

internal sealed class Lox
{
    private static readonly Interpreter Interpreter = new();
    private static bool HadError = false;
    private static bool HadRuntimeError = false;

    internal static void RunFile(string path)
    {
        var file = File.OpenText(path);
        Run(file.ReadToEnd());

        if (HadError) Environment.Exit(65);
        if (HadRuntimeError) Environment.Exit(70);
    }

    internal static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            HadError = false;
        }
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var expression = parser.Parse();

        if (HadError || expression == null) return;

        Interpreter.Interpret(expression);

        /*
        if (expression != null)
        {
            Console.WriteLine(new AstPrinter().Print(expression));
        }
        else
        {
            tokens.ForEach(token => Console.WriteLine(token));
        }
        */
    }

    internal static void Error(int line, string message)
    {
        Report(line, where: "", message);
    }

    internal static void Error(Token token, string message)
    {
        if (token.Type == TokenType.Eof)
        {
            Report(token.Line, where: " at end", message);
        }
        else
        {
            Report(token.Line, where: $" at '{token.Lexeme}'", message);
        }
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        HadError = true;
    }

    internal static void RuntimeError(RuntimeError error)
    {
        Console.Error.WriteLine(error.Message);
        Console.Error.WriteLine($"[line {error.Token.Line}]");
        HadRuntimeError = true;
    }
}
