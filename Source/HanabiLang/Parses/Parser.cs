using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Lexers;
using HanabiLang.Parses.Nodes;

namespace HanabiLang.Parses
{
    class Parser
    {
        private List<Token> tokens { get; set; }
        private int currentTokenIndex { get; set; }
        private List<AstNode> nodes { get; }

        public Parser(IEnumerable<Token> tokens)
        {
            this.nodes = new List<AstNode>();
            this.tokens = tokens.ToList();
        }

        public AbstractSyntaxTree Parse()
        {
            var ast = new AbstractSyntaxTree();

            while (this.currentTokenIndex < this.tokens.Count)
            {
                var child = this.ParseChild();

                if (child != null)
                    this.nodes.Add(child);
            }

            ast.Nodes = this.nodes;

            return ast;
        }

        // Expressions
        private AstNode Factor(bool skipIndexers, bool skipArrowFn)
        {
            var currentToken = this.tokens[this.currentTokenIndex];
            switch (currentToken.Type)
            {
                case TokenType.IDENTIFIER:
                    if (!skipArrowFn && this.currentTokenIndex + 1 < this.tokens.Count &&
                        this.tokens[this.currentTokenIndex + 1].Type == TokenType.DOUBLE_ARROW)
                    {
                        return FunctionDefinition(false, AccessibilityLevel.Public, true, true);
                    }
                    break;
                case TokenType.INT:
                    this.currentTokenIndex++;
                    return new IntNode(long.Parse(currentToken.Raw));

                case TokenType.FLOAT:
                    this.currentTokenIndex++;
                    return new FloatNode(double.Parse(currentToken.Raw));

                case TokenType.STRING:
                    this.currentTokenIndex++;
                    return new StringNode(currentToken.Raw);
                case TokenType.INTERPOLATED_STRING:
                    Queue<AstNode> interpolatedNodes = new Queue<AstNode>();
                    InterpolatedStringToken interpolatedToken = (InterpolatedStringToken)currentToken;
                    foreach (List<Token> tokens in interpolatedToken.InterpolatedTokens)
                    {
                        Parser parser = new Parser(tokens);
                        var interpolatedNode = parser.Expression();
                        interpolatedNodes.Enqueue(interpolatedNode);
                    }
                    this.currentTokenIndex++;
                    return new InterpolatedString(interpolatedToken.Texts, interpolatedNodes);
                case TokenType.TRUE:
                    this.currentTokenIndex++;
                    return new BooleanNode(true);
                case TokenType.FALSE:
                    this.currentTokenIndex++;
                    return new BooleanNode(false);
                case TokenType.NULL:
                    this.currentTokenIndex++;
                    return new NullNode();
                case TokenType.OPEN_ROUND_BRACKET:
                    {
                        // Check if it is a arrow function
                        int lastCurrentIndex = this.currentTokenIndex;
                        try
                        {
                            return FunctionDefinition(false, AccessibilityLevel.Public, true, false);
                        }
                        catch (SystemException)
                        {

                            // Check if round bracket priority
                            // adjust currentTokenIndex shifted when checking arrow function
                            currentTokenIndex = lastCurrentIndex;
                            this.Expect(TokenType.OPEN_ROUND_BRACKET);
                            var expression = this.Expression();
                            this.Expect(TokenType.CLOSE_ROUND_BRACKET);

                            return expression;
                        }
                    }
                case TokenType.OPERATOR:
                    {
                        this.currentTokenIndex++;

                        // Unary operator
                        if (currentToken.Raw == "-" || currentToken.Raw == "+" || currentToken.Raw == "!")
                        {
                            var expression = this.Factor(skipIndexers, skipArrowFn);
                            return new UnaryNode(expression, currentToken.Raw);
                        }
                        else
                        {
                            throw new SystemException(
                                    "Unexpected token: " + currentToken.Raw + ", " + currentToken.Type +
                                    ", line: " + currentToken.Line);
                        }
                    }

                case TokenType.OPEN_SQURE_BRACKET:
                    {
                        List<AstNode> elements = new List<AstNode>();

                        this.currentTokenIndex++;

                        while (this.currentTokenIndex <= this.tokens.Count &&
                               this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_SQURE_BRACKET)
                        {
                            elements.Add(this.Expression());

                            if (this.tokens[this.currentTokenIndex].Type == TokenType.COMMA)
                                this.currentTokenIndex++;
                        }

                        this.Expect(TokenType.CLOSE_SQURE_BRACKET);

                        var thing = new ListNode(elements);

                        return CheckIndexersAccess(thing);
                    }
                case TokenType.OPEN_CURLY_BRACKET:
                    {
                        var keyValues = new List<Tuple<AstNode, AstNode>>();

                        this.currentTokenIndex++;

                        while (this.currentTokenIndex <= this.tokens.Count &&
                               this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
                        {
                            AstNode key = this.Expression();
                            this.Expect(TokenType.COLON);
                            AstNode value = this.Expression();
                            keyValues.Add(Tuple.Create(key, value));

                            if (this.tokens[this.currentTokenIndex].Type == TokenType.COMMA)
                                this.currentTokenIndex++;
                        }

                        this.Expect(TokenType.CLOSE_CURLY_BRACKET);

                        return new DictNode(keyValues);
                    }

                default:
                    break;
            }
            return this.Identifier(skipIndexers);
        }

        private AstNode Term(bool skipIndexers, bool skipArrowFn)
        {
            var left = this.Factor(skipIndexers, skipArrowFn);

            while (this.currentTokenIndex < this.tokens.Count &&
                   (this.tokens[this.currentTokenIndex].Raw == "*" || this.tokens[this.currentTokenIndex].Raw == "/"
                   || this.tokens[this.currentTokenIndex].Raw == "."))
            {
                var currentToken = this.tokens[this.currentTokenIndex];

                this.Expect(TokenType.OPERATOR, TokenType.DOT);

                left = new ExpressionNode(left, this.Factor(skipIndexers, skipArrowFn), currentToken.Raw);
            }

            // For function calling
            if (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_ROUND_BRACKET)
            {
                left = FunctionCall(left);
            }

            return left;
        }

        private AstNode Expression(bool skipIndexers = false, bool skipEquals = false, bool skipArrowFn = false)
        {
            var left = this.Term(skipIndexers, skipArrowFn);

            if (this.currentTokenIndex >= this.tokens.Count)
            {
                return left;
            }

            Token currentToken = this.tokens[this.currentTokenIndex];
            string currentRaw = currentToken.Raw;

            while (currentRaw == "+" || currentRaw == "-" ||
                   currentRaw == "==" || currentRaw == "!=" ||
                   currentRaw == ">" || currentRaw == "<" ||
                   currentRaw == ">=" || currentRaw == "<=" ||
                   currentRaw == "%")
            {

                this.Expect(TokenType.OPERATOR, TokenType.DOT);

                left = new ExpressionNode(left, this.Term(skipIndexers, skipArrowFn), currentToken.Raw);
                left.Line = currentToken.Line;

                currentToken = this.tokens[this.currentTokenIndex];
                currentRaw = currentToken.Raw;
            }

            // && and ||
            if (this.currentTokenIndex < this.tokens.Count &&
                (currentRaw == "||" || currentRaw == "&&"))
            {
                this.Expect(TokenType.OPERATOR);

                left = new ExpressionNode(left, this.Expression(), currentRaw);
            }

            if (this.currentTokenIndex < this.tokens.Count &&
                currentToken.Type == TokenType.QUESTION_MARK)
            {
                var condition = left;
                this.Expect(TokenType.QUESTION_MARK);
                var consequent = this.Expression();
                this.Expect(TokenType.COLON);
                var alternative = this.Expression();
                left = new TernaryNode(condition, consequent, alternative);
            }

            if (this.currentTokenIndex < this.tokens.Count &&
            (currentRaw == "++" || currentRaw == "--"))
            {
                this.Expect(TokenType.OPERATOR);
                return new VariableAssignmentNode(left, new ExpressionNode(left, new IntNode(1), currentRaw[0].ToString()));
            }

            if (this.currentTokenIndex < this.tokens.Count &&
                (currentRaw == "+=" || currentRaw == "-=" || currentRaw == "*=" || currentRaw == "/=" || currentRaw == "%="))
            {
                this.Expect(TokenType.OPERATOR);

                return new VariableAssignmentNode(left, new ExpressionNode(left, this.Expression(), currentRaw.TrimEnd('=')));
            }

            if (!skipEquals && currentToken.Type == TokenType.EQUALS)
            {
                this.Expect(TokenType.EQUALS);
                return new VariableAssignmentNode(left, this.Expression());
            }

            if (currentToken.Type == TokenType.SEMI_COLON)
            {
                this.Expect(TokenType.SEMI_COLON);
                return left;
            }

            return left;
        }


        private AstNode VariableDefinition(bool constant, bool isStatic, AccessibilityLevel level)
        {
            this.currentTokenIndex++;

            var variableName = this.tokens[this.currentTokenIndex].Raw;
            this.Expect(TokenType.IDENTIFIER);

            // Check if datatype defined
            AstNode dataType = null;
            if (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type == TokenType.COLON)
            {
                this.Expect(TokenType.COLON);
                dataType = this.Expression(true, true, true);
            }

            // check if it is get set variable
            AccessibilityLevel? lastLevel = null;
            FnDefineNode getFn = null;
            FnDefineNode setFn = null;
            if (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_CURLY_BRACKET)
            {
                this.Expect(TokenType.OPEN_CURLY_BRACKET);
                while (this.currentTokenIndex < this.tokens.Count &&
                    this.tokens[this.currentTokenIndex].Type == TokenType.IDENTIFIER ||
                    this.tokens[this.currentTokenIndex].Type == TokenType.KEYWORD)
                {
                    var keyword = this.tokens[this.currentTokenIndex].Raw;
                    this.Expect(TokenType.IDENTIFIER, TokenType.KEYWORD);

                    List<AstNode> body = new List<AstNode>();
                    if (keyword.Equals("public") || keyword.Equals("protected") ||
                            keyword.Equals("private"))
                    {
                        if (lastLevel.HasValue)
                            throw new SystemException("Cannot redefine accessibility levels");
                        lastLevel = ToAccessibilityLevel(keyword);
                    }
                    else if (keyword.Equals("get") || keyword.Equals("set"))
                    {
                        bool isGet = keyword.Equals("get");

                        if (isGet && getFn != null)
                            throw new SystemException($"Cannot redefine get function");
                        if (!isGet && setFn != null)
                            throw new SystemException($"Cannot redefine set function");

                        if (this.currentTokenIndex < this.tokens.Count &&
                            this.tokens[this.currentTokenIndex].Type == TokenType.DOUBLE_ARROW)
                        {
                            this.Expect(TokenType.DOUBLE_ARROW);
                            var fnBody = this.ParseChild();

                            if (isGet)
                                body.Add(new ReturnNode(fnBody));
                            else
                                body.Add(fnBody);
                        }
                        else if (this.currentTokenIndex < this.tokens.Count &&
                                    this.tokens[this.currentTokenIndex].Type == TokenType.CLOSE_CURLY_BRACKET)
                        {
                            this.Expect(TokenType.OPEN_CURLY_BRACKET);

                            while (this.currentTokenIndex < this.tokens.Count &&
                                    this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
                            {
                                AstNode child = this.ParseChild();
                                if (child != null)
                                    body.Add(child);
                            }

                            this.Expect(TokenType.CLOSE_CURLY_BRACKET);
                        }
                        else
                        {
                            this.Expect(TokenType.SEMI_COLON);
                        }

                        if (lastLevel.HasValue && (int)lastLevel.Value <= (int)level)
                        {
                            throw new SystemException($"Cannot define getter/setter with the same or a larger accessibility level");
                        }

                        if (isGet)
                        {
                            getFn = new FnDefineNode($"get_{variableName}", new List<FnDefineParameter>() { },
                                dataType, body, isStatic, lastLevel ?? level);
                        }
                        else
                        {
                            setFn = new FnDefineNode($"set_{variableName}",
                                new List<FnDefineParameter>()
                                {
                                    new FnDefineParameter("value", dataType)
                                }, new NullNode(), body, isStatic, lastLevel ?? level);
                        }
                        lastLevel = null;
                    }
                    else
                    {
                        throw new SystemException($"Unexpected keyword {keyword}");
                    }
                }
                this.Expect(TokenType.CLOSE_CURLY_BRACKET);
            }
            else if (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type == TokenType.DOUBLE_ARROW)
            {
                this.Expect(TokenType.DOUBLE_ARROW);
                var fnBody = new ReturnNode(this.ParseChild());

                getFn = new FnDefineNode($"get_{variableName}", 
                    new List<FnDefineParameter>() { }, dataType, new List<AstNode>() { fnBody }, isStatic, level);
            }

                if (getFn != null && setFn != null)
            {
                if (getFn.Body.Count > 0 && setFn.Body.Count == 0)
                    throw new SystemException("Setter is auto-implemented variable but Getter is not");
                else if (setFn.Body.Count > 0 && getFn.Body.Count == 0)
                    throw new SystemException("Getter is auto-implemented variable but Setter is not");
            }

            if (this.currentTokenIndex < this.tokens.Count &&
                                    this.tokens[this.currentTokenIndex].Type == TokenType.EQUALS)
            {
                this.Expect(TokenType.EQUALS);

                // Disable assign default value to auto-implemented getter
                /*if (getFn != null && setFn == null)
                    throw new SystemException("Cannot assign value to a read only variable");*/
                if (setFn != null && setFn.Body.Count > 0)
                    throw new SystemException("Only auto-implemented variable can have initializers");

                var node = new VariableDefinitionNode(variableName, this.Expression(), dataType, getFn, setFn, constant, isStatic, level);
                if (this.currentTokenIndex >= this.tokens.Count)
                    node.Line = this.tokens[this.tokens.Count - 1].Line;
                else
                    node.Line = this.tokens[this.currentTokenIndex].Line;
                return node;
            }
            else
            {
                if (getFn == null && setFn == null)
                    throw new SystemException("Normal variable must define a initial value");

                var node = new VariableDefinitionNode(variableName, null, dataType, getFn, setFn, constant, isStatic, level);
                if (this.currentTokenIndex >= this.tokens.Count)
                    node.Line = this.tokens[this.tokens.Count - 1].Line;
                else
                    node.Line = this.tokens[this.currentTokenIndex].Line;
                return node;
            }
        }

        private AstNode IfStatement()
        {
            this.currentTokenIndex++;

            var condition = this.Expression();
            List<AstNode> thenStatements = new List<AstNode>();
            List<AstNode> elseStatements = new List<AstNode>();

            this.Expect(TokenType.OPEN_CURLY_BRACKET);

            while (this.currentTokenIndex < this.tokens.Count &&
                   this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
            {
                AstNode child = this.ParseChild();
                if (child != null)
                    thenStatements.Add(child);
            }

            this.Expect(TokenType.CLOSE_CURLY_BRACKET);

            if (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Raw == "else")
            {
                this.currentTokenIndex++;

                if (this.currentTokenIndex < this.tokens.Count && 
                    this.tokens[this.currentTokenIndex].Raw == "if")
                {
                    elseStatements.Add(this.IfStatement());
                }
                else
                {
                    this.Expect(TokenType.OPEN_CURLY_BRACKET);

                    while (this.currentTokenIndex < this.tokens.Count &&
                           this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
                    {
                        AstNode child = this.ParseChild();
                        if (child != null)
                            elseStatements.Add(child);
                    }

                    this.Expect(TokenType.CLOSE_CURLY_BRACKET);
                }
            }

            return new IfNode(condition, thenStatements,
                                                     elseStatements);

        }
        private AstNode WhileStatement()
        {
            this.currentTokenIndex++;

            var condition = this.Expression();
            List<AstNode> thenStatements = new List<AstNode>();

            this.Expect(TokenType.OPEN_CURLY_BRACKET);

            while (this.currentTokenIndex < this.tokens.Count &&
                   this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
            {
                AstNode child = this.ParseChild();
                if (child != null)
                    thenStatements.Add(child);
            }

            this.Expect(TokenType.CLOSE_CURLY_BRACKET);

            return new WhileNode(condition, thenStatements);
        }
        private AstNode ForStatement()
        {
            this.currentTokenIndex++;
            bool haveBracket = false;
            if (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_ROUND_BRACKET)
            {
                haveBracket = true;
                this.currentTokenIndex++;
            }


            var initializer = this.tokens[this.currentTokenIndex].Raw;

            this.Expect(TokenType.IDENTIFIER);

            if (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Raw != "in")
            {
                throw new SystemException("Keyword 'in' is essential in for loop");
            }

            this.currentTokenIndex++;

            var iterator = this.Expression();

            if (haveBracket)
                Expect(TokenType.CLOSE_ROUND_BRACKET);

            List<AstNode> thenStatements = new List<AstNode>();

            this.Expect(TokenType.OPEN_CURLY_BRACKET);

            while (this.currentTokenIndex < this.tokens.Count &&
                   this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
            {
                AstNode child = this.ParseChild();
                if (child != null)
                    thenStatements.Add(child);
            }

            this.Expect(TokenType.CLOSE_CURLY_BRACKET);

            return new ForNode(initializer, iterator, thenStatements);
        }
        private AstNode ReturnStatement()
        {
            this.currentTokenIndex++;

            var currentToken = this.tokens[this.currentTokenIndex];

            switch (currentToken.Type)
            {
                case TokenType.IDENTIFIER:
                case TokenType.STRING:
                case TokenType.INTERPOLATED_STRING:
                case TokenType.OPEN_ROUND_BRACKET:
                case TokenType.OPERATOR:
                case TokenType.INT:
                case TokenType.FLOAT:
                case TokenType.TRUE:
                case TokenType.FALSE:
                case TokenType.NULL:
                case TokenType.OPEN_SQURE_BRACKET:
                    {
                        var expression = this.Expression();
                        expression.Line = currentToken.Line;
                        return new ReturnNode(expression);
                    }
                case TokenType.KEYWORD:
                    if (currentToken.Raw.Equals("this") || currentToken.Raw.Equals("super"))
                    {
                        var expression = this.Expression();
                        expression.Line = currentToken.Line;
                        return new ReturnNode(expression);
                    }
                    break;
            }
            return new ReturnNode();
        }
        private AstNode ThrowStatement()
        {
            this.currentTokenIndex++;

            var currentToken = this.tokens[this.currentTokenIndex];

            var expression = this.Expression();
            expression.Line = currentToken.Line;
            return new ThrowNode(expression);
        }

        private AstNode ImportStatement()
        {
            this.currentTokenIndex++;
            List<string> imports = null;
            if (this.tokens[this.currentTokenIndex].Type == TokenType.IDENTIFIER)
            {
                imports = new List<string>();
                while (this.currentTokenIndex < this.tokens.Count &&
                    this.tokens[this.currentTokenIndex].Type == TokenType.IDENTIFIER)
                {
                    imports.Add(this.tokens[this.currentTokenIndex].Raw);
                    this.Expect(TokenType.IDENTIFIER);
                    if (this.currentTokenIndex < this.tokens.Count &&
                        this.tokens[this.currentTokenIndex].Type == TokenType.COMMA)
                        this.Expect(TokenType.COMMA);
                }
                if (this.tokens[this.currentTokenIndex].Raw != "from")
                    throw new SystemException("The correct format is import ... from \"\";");
                this.Expect(TokenType.KEYWORD);
            }

            var importPath = this.tokens[this.currentTokenIndex].Raw;
            this.Expect(TokenType.STRING);

            string asName = "";
            if (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Raw == "as")
            {
                this.Expect(TokenType.KEYWORD);
                if (imports != null)
                    throw new SystemException("Format: import ... from \"\"; cannot use as keyword");
                asName = this.tokens[this.currentTokenIndex].Raw;
                this.Expect(TokenType.IDENTIFIER);
            }

            return new ImportNode(importPath, imports, asName);
        }

        private AstNode FunctionDefinition(bool isStatic, AccessibilityLevel level, bool isArrowFn = false, bool isOneParam = false)
        {
            if (!isArrowFn)
            {
                this.currentTokenIndex++;
            }

            string functionName = "";
            if (!isArrowFn)
            {
                functionName = this.tokens[this.currentTokenIndex].Raw;
                this.currentTokenIndex++;
            }

            var parameters = new List<FnDefineParameter>();
            List<AstNode> body = new List<AstNode>();
            bool isLastDefaultValue = false;
            if (!isOneParam)
            {
                this.Expect(TokenType.OPEN_ROUND_BRACKET);

                while (this.currentTokenIndex < this.tokens.Count &&
                        this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_ROUND_BRACKET)
                {
                    var currentToken = this.tokens[this.currentTokenIndex];

                    if (currentToken.Type != TokenType.IDENTIFIER)
                    {
                        throw new SystemException("Expected identifier line: " + (currentToken.Line + 1));
                    }

                    string paramName = currentToken.Raw;
                    AstNode paramType = null;
                    AstNode paramDefaultValue = null;
                    this.currentTokenIndex++;
                    if (this.currentTokenIndex < this.tokens.Count && 
                        this.tokens[this.currentTokenIndex].Type == TokenType.COLON)
                    {
                        this.Expect(TokenType.COLON);
                        paramType = Expression(true, true, true);
                    }

                    if (this.currentTokenIndex < this.tokens.Count && 
                        this.tokens[this.currentTokenIndex].Type == TokenType.EQUALS)
                    {
                        this.Expect(TokenType.EQUALS);
                        paramDefaultValue = Expression();
                        isLastDefaultValue = true;
                    }
                    else
                    {
                        if (isLastDefaultValue)
                        {
                            throw new SystemException("Function default value cannot in the middle");
                        }
                    }

                    parameters.Add(new FnDefineParameter(paramName, paramType, paramDefaultValue));

                    if (this.currentTokenIndex != this.tokens.Count)
                    {
                        if (this.currentTokenIndex < this.tokens.Count && 
                            this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_ROUND_BRACKET)
                        {
                            this.Expect(TokenType.COMMA);
                        }
                    }
                }

                this.Expect(TokenType.CLOSE_ROUND_BRACKET);
            }
            else
            {
                if (this.currentTokenIndex < this.tokens.Count && 
                    this.tokens[this.currentTokenIndex].Type != TokenType.IDENTIFIER)
                {
                    throw new SystemException("Expected identifier line: " + (this.tokens[this.currentTokenIndex].Line + 1));
                }

                string paramName = this.tokens[this.currentTokenIndex].Raw;
                AstNode paramType = null;
                AstNode paramValue = null;
                this.currentTokenIndex++;
                if (this.currentTokenIndex < this.tokens.Count && 
                    this.tokens[this.currentTokenIndex].Type == TokenType.COLON)
                {
                    this.Expect(TokenType.COLON);
                    paramType = Expression();
                }

                if (this.currentTokenIndex < this.tokens.Count && 
                    this.tokens[this.currentTokenIndex].Type == TokenType.EQUALS)
                {
                    this.Expect(TokenType.EQUALS);
                    paramValue = Expression();
                }
                parameters.Add(new FnDefineParameter(paramName, paramType, paramValue));

                //this.currentTokenIndex++;
            }

            AstNode returnType = null;
            if (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Type == TokenType.SINGLE_ARROW)
            {
                this.Expect(TokenType.SINGLE_ARROW);
                returnType = this.Expression(true, true, true);
            }


            bool haveArrow = false;
            bool haveBracket = true;
            if (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Type == TokenType.DOUBLE_ARROW)
            {
                this.currentTokenIndex++;
                haveArrow = true;
            }

            if (isArrowFn && (!haveArrow && returnType == null))
            {
                throw new SystemException("Lambda function must define return type or use double arrow");
            }

            if (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Type != TokenType.OPEN_CURLY_BRACKET)
                haveBracket = false;

            if (haveBracket)
            {
                this.Expect(TokenType.OPEN_CURLY_BRACKET);

                while (this.currentTokenIndex < this.tokens.Count &&
                        this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
                {
                    AstNode child = this.ParseChild();
                    if (child != null)
                        body.Add(child);
                }

                this.Expect(TokenType.CLOSE_CURLY_BRACKET);
            }
            else
            {
                if (!haveArrow)
                    throw new SystemException("Function cannot define without arrow and curly bracket " +
                                            (this.tokens[this.currentTokenIndex].Line + 1));
                AstNode child = this.ParseChild();
                if (child != null)
                    body.Add(new ReturnNode(child));
            }
            return new FnDefineNode(functionName, parameters, returnType, body, isStatic, level);

        }
        private AstNode SwitchStatement()
        {
            this.currentTokenIndex++;

            var expression = this.Expression();

            this.Expect(TokenType.OPEN_CURLY_BRACKET);

            List<SwitchCaseNode> cases = new List<SwitchCaseNode>();
            SwitchCaseNode defaultCase = null;

            while (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
            {
                var token = this.tokens[this.currentTokenIndex];
                bool isDefaultCase = false;
                List<AstNode> caseExpressions = new List<AstNode>();

                // Every cases with same body
                while (this.currentTokenIndex < this.tokens.Count &&
                        this.tokens[this.currentTokenIndex].Type != TokenType.OPEN_CURLY_BRACKET)
                {
                    if (token.Type != TokenType.KEYWORD || (token.Raw != "case" && token.Raw != "default"))
                        throw new SystemException($"switch only accept case or default as keyword");

                    this.currentTokenIndex++;

                    if (token.Raw == "case")
                    {
                        AstNode caseExpression = this.Expression();
                        caseExpression.Line = token.Line;
                        caseExpressions.Add(caseExpression);
                    }
                    else
                    {
                        if (defaultCase != null)
                            throw new SystemException("switch default case cannot define more than 1 time");
                        isDefaultCase = true;
                    }

                    this.Expect(TokenType.COLON);
                }

                var statements = new List<AstNode>();
                this.Expect(TokenType.OPEN_CURLY_BRACKET);

                while (this.currentTokenIndex < this.tokens.Count && 
                    this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
                {
                    AstNode child = this.ParseChild();
                    if (child != null)
                        statements.Add(child);
                }

                this.currentTokenIndex++;

                var switchCase = new SwitchCaseNode(caseExpressions, statements);
                if (isDefaultCase)
                    defaultCase = switchCase;
                else
                    cases.Add(switchCase);
            }

            this.currentTokenIndex++;

            return new SwitchNode(expression, cases, defaultCase);

        }
        private AstNode ClassDefinition(bool isStatic, AccessibilityLevel level)
        {
            this.currentTokenIndex++;

            var name = this.tokens[this.currentTokenIndex].Raw;

            this.Expect(TokenType.IDENTIFIER);

            // Prepare for data class
            /*this.Expect(TokenType.OPEN_ROUND_BRACKET);

            var parameters = new List<string>();

            while (this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_ROUND_BRACKET)
            {
                var token = this.tokens[this.currentTokenIndex];

                if (token.Type != TokenType.IDENTIFIER)
                    throw new SystemException("Expected identifier on line: " + token.Line);

                parameters.Add(token.Raw);

                this.currentTokenIndex++;

                if (this.tokens[this.currentTokenIndex].Type == TokenType.COMMA)
                    this.currentTokenIndex++;
            }

            this.currentTokenIndex++;
            */
            this.Expect(TokenType.OPEN_CURLY_BRACKET);

            var members = new List<AstNode>();

            while (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_CURLY_BRACKET)
            {
                AstNode child = this.ParseChild();
                if (child != null)
                    members.Add(child);
            }

            this.currentTokenIndex++;

            return new ClassDefineNode(name, members, isStatic, level);
        }

        private AstNode FunctionCall(AstNode node)
        {
            this.currentTokenIndex++;

            var arguments = new Dictionary<string, AstNode>();
            int argsCount = 0;
            bool isLastWithName = false;
            while (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_ROUND_BRACKET)
            {
                if (this.currentTokenIndex + 1 < this.tokens.Count &&
                    this.tokens[this.currentTokenIndex].Type == TokenType.IDENTIFIER &&
                    this.tokens[this.currentTokenIndex + 1].Type == TokenType.EQUALS)
                {
                    string argName = this.tokens[this.currentTokenIndex].Raw;
                    this.Expect(TokenType.IDENTIFIER);
                    this.Expect(TokenType.EQUALS);
                    arguments[argName] = this.Expression(false, true, false);
                    isLastWithName = true;
                    argsCount++;
                }
                else
                {
                    // supporting multiple arguments
                    /*if (isLastWithName)
                        throw new SystemException("cannot pass argument without name after passing named argument");*/

                    arguments[argsCount.ToString()] = this.Expression();
                    argsCount++;
                }

                if (this.currentTokenIndex < this.tokens.Count &&
                    this.tokens[this.currentTokenIndex].Type == TokenType.COMMA)
                {
                    this.currentTokenIndex++;
                }
            }

            if (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_ROUND_BRACKET)
            {
                throw new SystemException("Expected ')'");
            }

            this.currentTokenIndex++;

            AstNode lastFnRefCall = new FnReferenceCallNode(node, arguments);
            while (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_ROUND_BRACKET)
            {
                lastFnRefCall = FunctionCall(lastFnRefCall);
            }

            /*AstNode lastFnRefCall = new FnCallNode(currentToken.Raw, arguments);
            while (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_ROUND_BRACKET)
            {
                lastFnRefCall = FnRefCall(lastFnRefCall);
            }*/

            return lastFnRefCall;
        }

        /*private AstNode FnRefCall(AstNode node)
        {
            this.currentTokenIndex++;

            var arguments = new Dictionary<string, AstNode>();
            int argsCount = 0;
            while (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_ROUND_BRACKET)
            {
                arguments[argsCount.ToString()] = this.Expression();

                if (this.tokens[this.currentTokenIndex].Type == TokenType.COMMA)
                {
                    this.currentTokenIndex++;
                }
                argsCount++;
            }

            if (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type != TokenType.CLOSE_ROUND_BRACKET)
            {
                throw new SystemException("Expected ')'");
            }

            this.currentTokenIndex++;

            return new FnReferenceCallNode(node, arguments);
        }*/


        private AstNode Identifier(bool skipIndexers)
        {
            var currentToken = this.tokens[this.currentTokenIndex];

            this.currentTokenIndex++;

            // Function call
            if (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_ROUND_BRACKET)
            {
                //return FunctionCall(currentToken);
                return FunctionCall(new VariableReferenceNode(currentToken.Raw));
            }
            else if (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Type == TokenType.EQUALS)
            {
                /*this.currentTokenIndex++;

                var value = this.Expression();

                return new VariableAssignmentNode(currentToken.Raw, value);*/
            }
            else if (this.currentTokenIndex < this.tokens.Count && 
                this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_SQURE_BRACKET)
            {
                if (!skipIndexers)
                {
                    this.currentTokenIndex--;

                    var obj = this.Expression(true);
                    var indexersAccess = this.CheckIndexersAccess(obj);

                    /*AstNode lastFnRefCall = null;
                    while (this.currentTokenIndex < this.tokens.Count &&
                        this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_ROUND_BRACKET)
                    {
                        if (lastFnRefCall == null)
                            lastFnRefCall = FunctionCall(indexersAccess);
                        else
                            lastFnRefCall = FunctionCall(lastFnRefCall);
                    }
                    if (lastFnRefCall != null)
                        return lastFnRefCall;*/

                    if (this.currentTokenIndex < this.tokens.Count &&
                        this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_ROUND_BRACKET)
                        return FunctionCall(indexersAccess);

                    return indexersAccess;
                }
            }

            var res = new VariableReferenceNode(currentToken.Raw);
            res.Line = currentToken.Line;
            return res;
        }

        private AstNode CheckIndexersAccess(AstNode child)
        {
            if (this.currentTokenIndex >= this.tokens.Count)
                return child;

            var token = this.tokens[this.currentTokenIndex];

            if (token.Type != TokenType.OPEN_SQURE_BRACKET)
                return child;

            this.currentTokenIndex++;

            var expression = this.Expression();
            expression.Line = token.Line;

            this.Expect(TokenType.CLOSE_SQURE_BRACKET);

            if (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type == TokenType.OPEN_SQURE_BRACKET)
            {
                return this.CheckIndexersAccess(new IndexersNode(child, expression));
            }

            return new IndexersNode(child, expression);

        }


        private void Expect(params TokenType[] types)
        {
            Token currentToken = this.tokens[this.currentTokenIndex];

            this.currentTokenIndex++;

            if (!types.Contains(currentToken.Type))
            {
                StringBuilder typesText = new StringBuilder();
                typesText.Append('[');
                foreach (var type in types)
                {
                    typesText.Append($"{type}, ");
                }
                if (types.Length > 0)
                {
                    typesText.Remove(typesText.Length - 2, 2);
                }
                typesText.Append(']');
                throw new SystemException(
                        "Unexpected token: " + currentToken.Raw + ". Expected: " + typesText + ", line: " +
                        (currentToken.Line + 1));
            }
        }

        private AccessibilityLevel ToAccessibilityLevel(string level)
        {
            switch (level)
            {
                case "private":
                    return AccessibilityLevel.Private;
                case "protected":
                    return AccessibilityLevel.Protected;
                case "internal":
                    return AccessibilityLevel.Internal;
                case "public":
                default:
                    return AccessibilityLevel.Public;
            }
        }

        private AstNode CheckAccessibilityLevel(string level)
        {
            this.Expect(TokenType.KEYWORD);
            bool isStatic = level.Equals("static");
            AccessibilityLevel accessibilityLevel = ToAccessibilityLevel(level);

            while (this.currentTokenIndex < this.tokens.Count &&
                this.tokens[this.currentTokenIndex].Type == TokenType.KEYWORD)
            {
                string keyword = this.tokens[this.currentTokenIndex].Raw;
                if (keyword.Equals("static"))
                {
                    this.Expect(TokenType.KEYWORD);
                    if (isStatic)
                        throw new SystemException("Already defined static");
                    isStatic = true;
                }
                else if (keyword.Equals("var") || keyword.Equals("auto") ||
                    keyword.Equals("let") || keyword.Equals("const"))
                {
                    return this.VariableDefinition(keyword.Equals("const"), isStatic, accessibilityLevel);
                }
                else if (keyword.Equals("fn"))
                {
                    return this.FunctionDefinition(isStatic, accessibilityLevel, false, false);
                }
                else if (keyword.Equals("class"))
                {
                    return this.ClassDefinition(isStatic, accessibilityLevel);
                }
            }
            throw new SystemException($"Unexpected {level} defined");
        }

        private AstNode ParseChild()
        {
            var token = this.tokens[this.currentTokenIndex];

            switch (token.Type)
            {
                case TokenType.IDENTIFIER:
                case TokenType.INTERPOLATED_STRING:
                case TokenType.STRING:
                case TokenType.OPEN_ROUND_BRACKET:
                case TokenType.OPERATOR:
                case TokenType.INT:
                case TokenType.FLOAT:
                case TokenType.TRUE:
                case TokenType.FALSE:
                case TokenType.NULL:
                case TokenType.OPEN_SQURE_BRACKET:
                    {
                        var expression = this.Expression();
                        expression.Line = token.Line;
                        return expression;
                    }
                case TokenType.KEYWORD:
                    {
                        AstNode result;

                        if (token.Raw.Equals("var") || token.Raw.Equals("auto") ||
                            token.Raw.Equals("let") || token.Raw.Equals("const"))
                            result = this.VariableDefinition(token.Raw.Equals("const"), false, AccessibilityLevel.Public);
                        else if (token.Raw.Equals("public") || token.Raw.Equals("protected") ||
                            token.Raw.Equals("private"))
                            result = CheckAccessibilityLevel(token.Raw);
                        else if (token.Raw.Equals("if")) result = this.IfStatement();
                        else if (token.Raw.Equals("switch")) result = this.SwitchStatement();
                        else if (token.Raw.Equals("fn")) result = this.FunctionDefinition(false, AccessibilityLevel.Public, false, false);
                        else if (token.Raw.Equals("class")) result = this.ClassDefinition(false, AccessibilityLevel.Public);
                        else if (token.Raw.Equals("for")) result = this.ForStatement();
                        else if (token.Raw.Equals("while")) result = this.WhileStatement();
                        else if (token.Raw.Equals("return")) result = this.ReturnStatement();
                        else if (token.Raw.Equals("import")) result = this.ImportStatement();
                        else if (token.Raw.Equals("throw")) result = this.ThrowStatement();
                        else if (token.Raw.Equals("break"))
                        {
                            this.currentTokenIndex++;
                            result = new BreakNode();
                        }
                        else if (token.Raw.Equals("continue"))
                        {
                            this.currentTokenIndex++;
                            result = new ContinueNode();
                        }
                        else if (token.Raw.Equals("this"))
                        {
                            var expression = this.Expression();
                            expression.Line = token.Line;
                            return expression;
                        }
                        else throw new SystemException("Keyword is not implemented: " + token.Raw);

                        result.Line = token.Line;
                        return result;
                    }
                case TokenType.SEMI_COLON:
                        this.currentTokenIndex++;
                        return null;
                default:
                    throw new SystemException(
                            "Unexpected token: " + token.Type + " line: " +
                            token.Line);
            }
        }
    }
}
