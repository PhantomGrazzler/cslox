namespace cslox
{
    internal class Lox
    {
        private static bool HadError = false;

        internal static void RunFile(string path)
        {
            var file = File.OpenText(path);
            Run(file.ReadToEnd());

            if(HadError)
            {
                Environment.Exit(65);
            }
        }

        internal static void RunPrompt()
        {
            while(true) 
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
            tokens.ForEach(token => Console.WriteLine(token));
        }

        internal static void Error(int line, string message)
        {
            Report(line, where: "", message);
        }

        private static void Report(int line, string where, string message) 
        {
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            HadError = true;
        }
    }
}
