﻿using cslox;

internal class Program
{
    private const string CompletedSection = "9.3";

    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine($"PhantomGrazzler's Lox interpreter (completed up to section {CompletedSection})\n");

        if (args.Length > 1)
        {
            Console.WriteLine("Usage: cslox.exe [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            Console.WriteLine($"Executing file: {args[0]}");
            Lox.RunFile(args[0]);
        }
        else
        {
            Lox.RunPrompt();
        }
    }
}
