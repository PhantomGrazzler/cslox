﻿namespace cslox;

/// <summary>
/// Exception thrown by the parser if it fails to parse the provided input.
/// </summary>
public class ParseError : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public ParseError(string message) : base(message) { }
}

/// <summary>
/// 
/// </summary>
public class Parser
{
    private readonly List<Token> m_tokens = new();
    private int m_currentIndex = 0;

    /// <summary>
    /// Construct a <see cref="Parser"/> with the provided tokens.
    /// </summary>
    /// <param name="tokens">Tokens to be parsed when calling <see cref="Parse"/>.</param>
    public Parser(List<Token> tokens)
    {
        m_tokens = tokens;
    }

    /// <summary>
    /// Parse the tokens provided when the parser was constructed.
    /// </summary>
    /// <returns>A collection of statements which can be <c>null</c> if there was a parsing error.</returns>
    public List<Stmt?> Parse()
    {
        var statements = new List<Stmt?>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(TokenType.Var))
            {
                return VarDeclaration();
            }

            return Statement();
        }
        catch (ParseError)
        {
            Synchronise();
            return null;
        }
    }

    private Stmt Statement()
    {
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.Print)) return PrintStatement();
        if (Match(TokenType.LeftBrace)) return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt IfStatement()
    {
        _ = Consume(TokenType.LeftParen, "Expected '(' after 'if'.");
        var condition = Expression();
        _ = Consume(TokenType.RightParen, "Expected ')' after if condition.");

        var thenBranch = Statement();
        var elseBranch = Match(TokenType.Else) ? Statement() : null;

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        _ = Consume(TokenType.Semicolon, "Expected ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected a variable name.");
        var initializer = Match(TokenType.Equal) ? Expression() : null;
        _ = Consume(TokenType.Semicolon, "Expected ';' after variable declaration.");

        return new Stmt.Var(name, initializer);
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        _ = Consume(TokenType.Semicolon, "Expected ';' after expression.");
        return new Stmt.ExpressionStatement(expr);
    }

    private List<Stmt?> Block()
    {
        var statements = new List<Stmt?>();

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        _ = Consume(TokenType.RightBrace, "Expected '}' after block.");
        return statements;
    }

    private Expr Expression() => Assignment();

    private Expr Assignment()
    {
        var expr = Or();

        if (Match(TokenType.Equal))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable variable)
            {
                var name = variable.Name;
                return new Expr.Assign(name, value);
            }

            _ = Error(token: equals, $"Invalid assignment target: {equals.Lexeme}.");
        }

        return expr;
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(TokenType.Or))
        {
            var op = Previous();
            var right = And();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(TokenType.And))
        {
            var op = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        var expr = Comparison();
        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        var expr = Term();
        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var op = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();
        while (Match(TokenType.Minus, TokenType.Plus))
        {
            var op = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();
        while (Match(TokenType.Slash, TokenType.Star))
        {
            var op = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            var op = Previous();
            var right = Unary();
            return new Expr.Unary(op, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(TokenType.True)) return new Expr.Literal(true);
        if (Match(TokenType.False)) return new Expr.Literal(false);
        if (Match(TokenType.Nil)) return new Expr.Literal(null);
        if (Match(TokenType.Number, TokenType.String)) return new Expr.Literal(Previous().Literal);
        if (Match(TokenType.Identifier)) return new Expr.Variable(Previous());

        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expected ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expected expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    private static ParseError Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseError(message);
    }

    private void Synchronise()
    {
        _ = Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon) return;

            switch (Peek().Type)
            {
                case TokenType.Class:
                case TokenType.For:
                case TokenType.Fun:
                case TokenType.If:
                case TokenType.Print:
                case TokenType.Return:
                case TokenType.Var:
                case TokenType.While:
                    return;
            }

            _ = Advance();
        }
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
        {
            return false;
        }

        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            ++m_currentIndex;
        }

        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.Eof;
    }

    private Token Peek()
    {
        return m_tokens[m_currentIndex];
    }

    private Token Previous()
    {
        return m_tokens[m_currentIndex - 1];
    }
}
