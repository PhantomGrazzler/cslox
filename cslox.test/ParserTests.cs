namespace cslox.test;

public class ParserTests
{

    #region Expressions

    private static Expr? ParseExpression(string source)
    {
        var tokens = new Scanner(source).ScanTokens();
        var statements = new Parser(tokens).Parse();
        if (statements.Count > 0 && statements.First() is Stmt.ExpressionStatement statementExpression)
        {
            return statementExpression.Expression;
        }
        else
        {
            return null;
        }
    }

    [InlineData(";")]
    [InlineData("var")]
    [InlineData("(1")]
    [InlineData("1+")]
    [InlineData("2-")]
    [InlineData("*3")]
    [InlineData("/4")]
    [InlineData("var a = 1;")]
    [Theory]
    public void InvalidExpressions(string source)
    {
        Assert.Null(ParseExpression(source));
    }

    [InlineData("nil", null)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("0", 0.0)]
    [InlineData("5", 5.0)]
    [InlineData("0.123", 0.123)]
    [InlineData("\"string with spaces\"", "string with spaces")]
    [InlineData("\"1.23\"", "1.23")]
    [Theory]
    public void ValidLiterals(string source, object? expectedValue)
    {
        var expression = ParseExpression($"{source};");
        Assert.NotNull(expression);

        var literal = expression as Expr.Literal;
        Assert.NotNull(literal);
        Assert.Equal(expectedValue, literal.Value);
    }

    [InlineData("nil", "==", "true", null, TokenType.EqualEqual, true)]
    [InlineData("false", "!=", "7", false, TokenType.BangEqual, 7.0)]
    [InlineData("\"false\"", "!=", "85.349", "false", TokenType.BangEqual, 85.349)]
    [InlineData("1", ">", "0", 1.0, TokenType.Greater, 0.0)]
    [InlineData("\"some string\"", ">=", "nil", "some string", TokenType.GreaterEqual, null)]
    [InlineData("true", "<", "false", true, TokenType.Less, false)]
    [InlineData("false", "<=", "true", false, TokenType.LessEqual, true)]
    [InlineData("3", "-", "4", 3.0, TokenType.Minus, 4.0)]
    [InlineData("5", "+", "6", 5.0, TokenType.Plus, 6.0)]
    [InlineData("8", "/", "9", 8.0, TokenType.Slash, 9.0)]
    [InlineData("10", "*", "11", 10.0, TokenType.Star, 11.0)]
    [Theory]
    public void BinaryExpressions(
        string lhsString, string op, string rhsString, object? lhsExpected, TokenType opExpected, object? rhsExpected)
    {
        var expression = ParseExpression($"{lhsString} {op} {rhsString};");
        Assert.NotNull(expression);

        var equality = expression as Expr.Binary;
        Assert.NotNull(equality);

        var lhsValue = equality.Left as Expr.Literal;
        Assert.NotNull(lhsValue);
        Assert.Equal(lhsExpected, lhsValue.Value);

        var rhsValue = equality.Right as Expr.Literal;
        Assert.NotNull(rhsValue);
        Assert.Equal(rhsExpected, rhsValue.Value);

        var opToken = equality.Operator;
        Assert.NotNull(opToken);
        Assert.Equal(op, opToken.Lexeme);
        Assert.Equal(opExpected, opToken.Type);
    }

    [InlineData("!", "true", TokenType.Bang, true)]
    [InlineData("-", "nil", TokenType.Minus, null)]
    [Theory]
    public void UnaryExpressions(string op, string rhsString, TokenType opExpected, object? rhsExpected)
    {
        var expression = ParseExpression($"{op}{rhsString};");
        Assert.NotNull(expression);

        var unary = expression as Expr.Unary;
        Assert.NotNull(unary);

        var opToken = unary.Operator;
        Assert.NotNull(opToken);
        Assert.Equal(op, opToken.Lexeme);
        Assert.Equal(opExpected, opToken.Type);

        var rhsValue = unary.Right as Expr.Literal;
        Assert.NotNull(rhsValue);
        Assert.Equal(rhsExpected, rhsValue.Value);
    }

    [Fact]
    public void GroupingExpression()
    {
        var expression = ParseExpression("(75);");
        Assert.NotNull(expression);

        var grouping = expression as Expr.Grouping;
        Assert.NotNull(grouping);

        var literal = grouping.Expression as Expr.Literal;
        Assert.NotNull(literal);
        Assert.Equal(75.0, literal.Value);
    }

    [Fact]
    public void NestedGroupingExpression()
    {
        var expression = ParseExpression("((true));");
        Assert.NotNull(expression);

        var outerGrouping = expression as Expr.Grouping;
        Assert.NotNull(outerGrouping);

        var innerGrouping = outerGrouping.Expression as Expr.Grouping;
        Assert.NotNull(innerGrouping);

        var literal = innerGrouping.Expression as Expr.Literal;
        Assert.NotNull(literal);
        Assert.Equal(true, literal.Value);
    }

    [InlineData("v;", "v")]
    [InlineData("beverage;", "beverage")]
    [Theory]
    public void VariableExpression(string source, string expectedName)
    {
        var statements = Parse(source);
        var statement = Assert.Single(statements);
        Assert.NotNull(statement);

        if (statement is Stmt.ExpressionStatement exprStatement)
        {
            if (exprStatement.Expression is Expr.Variable variable)
            {
                Assert.Equal(expectedName, variable.Name.Lexeme);
            }
            else
            {
                Assert.Fail($"{source} does not contain a variable expression.");
            }
        }
        else
        {
            Assert.Fail($"{source} is not an expression statement.");
        }
    }

    [InlineData("var a=1;", "a", 1.0)]
    [InlineData("var beverage = \"espresso\";", "beverage", "espresso")]
    [InlineData("var null = nil;", "null", null)]
    [Theory]
    public void LiteralAssignmentExpression(string source, string expectedName, object? expectedValue)
    {
        var statements = Parse(source);
        var statement = Assert.Single(statements);
        Assert.NotNull(statement);

        if (statement is Stmt.Var varStatement)
        {
            Assert.Equal(expectedName, varStatement.Name.Lexeme);

            if (varStatement.Initializer is Expr.Literal value)
            {
                Assert.Equal(expectedValue, value.Value);
            }
            else
            {
                Assert.Fail($"Assignment value is not a literal.");
            }
        }
        else
        {
            Assert.Fail($"{source} is not an expression statement.");
        }
    }

    #endregion Expressions

    #region Statements

    private static List<Stmt?> Parse(string source)
    {
        var tokens = new Scanner(source).ScanTokens();
        return new Parser(tokens).Parse();
    }

    [InlineData("print \"one\";")]
    [InlineData("print true;")]
    [InlineData("print 2 + 1;")]
    [InlineData("print (2 + 1 * 4 - 9 / 3);")]
    [Theory]
    public void ValidPrintStatements(string source)
    {
        var statements = Parse(source);
        var statement = Assert.Single(statements);
        Assert.True(statement is Stmt.Print);
    }

    [InlineData("Print \"one\";")]
    [InlineData("prinT true;")]
    [InlineData("print var;")]
    [InlineData("print (2 + 1 * 4 - 9 / 3;")]
    [InlineData("print;")]
    [InlineData("print false")]
    [InlineData("print (print true);")]
    [Theory]
    public void InvalidPrintStatements(string source)
    {
        var statements = Parse(source);
        _ = Assert.Single(statements);
        Assert.Contains(null, statements);
    }

    [InlineData("\"one\";")]
    [InlineData("true;")]
    [InlineData("2 + 1;")]
    [InlineData("(2 + 1 * 4 - 9 / 3);")]
    [InlineData("!true;")]
    [InlineData("-8;")]
    [Theory]
    public void ValidExpressionStatements(string source)
    {
        var statements = Parse(source);
        var statement = Assert.Single(statements);
        Assert.True(statement is Stmt.ExpressionStatement);
    }

    // [InlineData("\"one;")] // - this calls Lox.Error, but does not throw an exception.
    [InlineData("true")]
    [InlineData("var;")]
    [InlineData("(2 + 1 * 4 - 9 / 3;")]
    [InlineData(";")]
    [Theory]
    public void InvalidExpressionStatements(string source)
    {
        var statements = Parse(source);
        _ = Assert.Single(statements);
        Assert.Contains(null, statements);
    }

    [InlineData("var a=1;")]
    [InlineData("var a;")]
    [InlineData("var beverage = \"espresso\";")]
    [InlineData("var a=b;")]
    [Theory]
    public void ValidVariableDeclarations(string source)
    {
        var statements = Parse(source);
        var statement = Assert.Single(statements);
        Assert.True(statement is Stmt.Var);
    }

    [InlineData("var;")]
    [InlineData("var g=;")]
    [InlineData("var g=var;")]
    [InlineData("var p=print;")]
    [Theory]
    public void InvalidVariableDeclarations(string source)
    {
        var statements = Parse(source);
        _ = Assert.Single(statements);
        Assert.Contains(null, statements);
    }

    [InlineData("{}", 0)]
    [InlineData("{ var a=1; }", 1)]
    [InlineData("{{}}", 1)]
    [InlineData("{print a; print b; var c=7; a+b+c;}", 4)]
    [Theory]
    public void BlockStatements(string source, int numberExpectedStatements)
    {
        var statements = Parse(source);
        var statement = Assert.Single(statements);

        var blockStatement = statement as Stmt.Block;
        Assert.NotNull(blockStatement);
        Assert.Equal(numberExpectedStatements, blockStatement.Statements.Count);
    }

    [InlineData("{")]
    [InlineData("}")]
    [InlineData("{{}")]
    [Theory]
    public void InvalidBlockStatements(string source)
    {
        var statements = Parse(source);
        _ = Assert.Single(statements);
        Assert.Contains(null, statements);
    }

    #endregion Statements

}
