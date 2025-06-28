namespace cslox;

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
/// <param name="tokens">Tokens to be parsed when calling <see cref="Parse"/>.</param>
public class Parser(List<Token> tokens)
{
    private enum Kind
    {
        Function,
        Method,
    }

    private const int MaxArguments = 255;
    private readonly List<Token> m_tokens = tokens;
    private int m_currentIndex = 0;

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
            if (Match(TokenType.Class)) return ClassDeclaration();
            if (Match(TokenType.Fun)) return Function(Kind.Function);
            if (Match(TokenType.Var)) return VarDeclaration();

            return Statement();
        }
        catch (ParseError)
        {
            Synchronise();
            return null;
        }
    }

    private Stmt.Class ClassDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected class name.");

        Expr.Variable? superclass = null;
        if (Match(TokenType.Less))
        {
            _ = Consume(TokenType.Identifier, "Expected a superclass name.");
            superclass = new Expr.Variable(Previous());
        }

        _ = Consume(TokenType.LeftBrace, "Expected '{' before class body.");

        List<Stmt.Function> methods = [];
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            methods.Add(Function(Kind.Method));
        }

        _ = Consume(TokenType.RightBrace, "Expected '}' after class body.");

        return new Stmt.Class(name, superclass, methods);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.For)) return ForStatement();
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.Print)) return PrintStatement();
        if (Match(TokenType.Return)) return ReturnStatement();
        if (Match(TokenType.While)) return WhileStatement();
        if (Match(TokenType.LeftBrace)) return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt ForStatement()
    {
        _ = Consume(TokenType.LeftParen, "Expected '(' after 'for'.");

        Stmt? GetInitializer()
        {
            if (Match(TokenType.Semicolon)) return null;
            else if (Match(TokenType.Var)) return VarDeclaration();
            else return ExpressionStatement();
        }
        var initializer = GetInitializer();

        var condition = Check(TokenType.Semicolon) ? new Expr.Literal(true) : Expression();
        _ = Consume(TokenType.Semicolon, "Expected ';' after 'for' loop condition.");

        var increment = Check(TokenType.RightParen) ? null : Expression();
        _ = Consume(TokenType.RightParen, "Expected ')' after 'for' loop increment.");

        var body = Statement();

        if (increment != null)
        {
            body = new Stmt.Block([body, new Stmt.ExpressionStatement(increment)]);
        }

        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block([initializer, body]);
        }

        return body;
    }

    private Stmt.If IfStatement()
    {
        _ = Consume(TokenType.LeftParen, "Expected '(' after 'if'.");
        var condition = Expression();
        _ = Consume(TokenType.RightParen, "Expected ')' after 'if' condition.");

        var thenBranch = Statement();
        var elseBranch = Match(TokenType.Else) ? Statement() : null;

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt.Print PrintStatement()
    {
        var value = Expression();
        _ = Consume(TokenType.Semicolon, "Expected ';' after value.");

        return new Stmt.Print(value);
    }

    private Stmt.Return ReturnStatement()
    {
        var keyword = Previous();
        Expr? value = null;
        if (!Check(TokenType.Semicolon))
        {
            value = Expression();
        }

        _ = Consume(TokenType.Semicolon, "Expected ';' after return value.");
        return new Stmt.Return(keyword, value);
    }

    private Stmt.While WhileStatement()
    {
        _ = Consume(TokenType.LeftParen, "Expected '(' after 'while'.");
        var condition = Expression();
        _ = Consume(TokenType.RightParen, "Expected ')' after 'while' condition.");
        var body = Statement();

        return new Stmt.While(condition, body);
    }

    private Stmt.Var VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected a variable name.");
        var initializer = Match(TokenType.Equal) ? Expression() : null;
        _ = Consume(TokenType.Semicolon, "Expected ';' after variable declaration.");

        return new Stmt.Var(name, initializer);
    }

    private Stmt.ExpressionStatement ExpressionStatement()
    {
        var expr = Expression();
        _ = Consume(TokenType.Semicolon, "Expected ';' after expression.");
        return new Stmt.ExpressionStatement(expr);
    }

    private Stmt.Function Function(Kind kind)
    {
        var name = Consume(TokenType.Identifier, $"Expected {kind} name.");
        _ = Consume(TokenType.LeftParen, $"Expected '(' after {kind} name.");
        var parameters = new List<Token>();

        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= MaxArguments)
                {
                    _ = Error(Peek(), "Cannot have more than 255 parameters.");
                }
                parameters.Add(Consume(TokenType.Identifier, "Expected parameter name."));
            } while (Match(TokenType.Comma));
        }

        _ = Consume(TokenType.RightParen, "Expected ')' after parameters.");
        _ = Consume(TokenType.LeftBrace, $"Expected '{{' before {kind} body.");
        var body = Block();

        return new Stmt.Function(name, parameters, body);
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
                return new Expr.Assign(variable.Name, value);
            }
            else if (expr is Expr.Get get)
            {
                return new Expr.Set(Object: get.Object, Name: get.Name, Value: value);
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

        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.Dot))
            {
                var name = Consume(TokenType.Identifier, "Expected property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr.Call FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();

        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (arguments.Count >= MaxArguments)
                {
                    _ = Error(token: Peek(), message: $"Cannot have more than {MaxArguments} arguments.");
                }

                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }

        var closingParen = Consume(TokenType.RightParen, "Expected ')' after arguments.");

        return new Expr.Call(callee, closingParen, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.True)) return new Expr.Literal(true);
        if (Match(TokenType.False)) return new Expr.Literal(false);
        if (Match(TokenType.Nil)) return new Expr.Literal(null);
        if (Match(TokenType.Number, TokenType.String)) return new Expr.Literal(Previous().Literal);
        if (Match(TokenType.Identifier)) return new Expr.Variable(Previous());
        if (Match(TokenType.This)) return new Expr.This(Previous());

        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            _ = Consume(TokenType.RightParen, "Expected ')' after expression.");
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
                _ = Advance();
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
