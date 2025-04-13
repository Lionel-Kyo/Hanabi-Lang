﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.Exceptions;
using HanabiLang.Lexers;
using HanabiLang.Parses.Nodes;
using HanabiLangLib.Parses.Nodes;

namespace HanabiLang.Parses
{
    public class Parser
    {
        private const bool DISABLE_ARROW_IF = true;
        private const bool DISABLE_ARROW_SWITCH = true;
        private const bool DISABLE_ARROW_LOOP = true;

        private List<Token> tokens { get; set; }
        private int currentTokenIndex { get; set; }
        private List<AstNode> nodes { get; }

        private bool HasToken => this.currentTokenIndex < this.tokens.Count;
        private Token CurrentToken => this.tokens[this.currentTokenIndex];
        private Token LastToken => this.tokens[this.currentTokenIndex - 1];

        private Stack<ParseScope> parseScopes;

        public Parser(IEnumerable<Token> tokens)
        {
            this.nodes = new List<AstNode>();
            this.tokens = tokens.ToList();
            this.parseScopes = new Stack<ParseScope>();
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

        public static string ExceptionToString(Exception ex)
        {
            StringBuilder result = new StringBuilder();
            for (Exception exception = ex; exception != null; exception = exception.InnerException)
            {
                if (exception is HanibiException)
                    result.AppendLine($"Unhandled Exception ({((HanibiException)exception).ExceptionObject.ClassType.Name}): {exception.Message}");
                else
                    result.AppendLine($"Unhandled Exception ({exception.GetType().Name}): {exception.Message}");
            }
            return result.ToString();
        }

        // Expressions
        private AstNode Factor(bool skipIndexer, bool skipArrowFn)
        {
            var currentToken = this.tokens[this.currentTokenIndex];
            switch (currentToken.Type)
            {
                case TokenType.IDENTIFIER:
                    if (!skipArrowFn && this.currentTokenIndex + 1 < this.tokens.Count &&
                        this.tokens[this.currentTokenIndex + 1].Type == TokenType.DOUBLE_ARROW)
                    {
                        return FnDefinition(false, AccessibilityLevel.Public, true, true);
                    }
                    break;
                case TokenType.INT:
                    this.Expect(TokenType.INT);
                    return new IntNode(long.Parse(currentToken.Raw));
                case TokenType.FLOAT:
                    this.Expect(TokenType.FLOAT);
                    return new FloatNode(double.Parse(currentToken.Raw));
                case TokenType.STRING:
                    this.Expect(TokenType.STRING);
                    return new StringNode(currentToken.Raw);
                case TokenType.INTERPOLATED_STRING:
                    Queue<AstNode> interpolatedNodes = new Queue<AstNode>();
                    InterpolatedStringToken interpolatedToken = (InterpolatedStringToken)currentToken;
                    foreach (List<Token> tokens in interpolatedToken.InterpolatedTokens)
                    {
                        Parser parser = new Parser(tokens);
                        var interpolatedNode = parser.Expression();
                        if (parser.HasToken)
                            throw new ParseException($"Cannot end with {parser.tokens[parser.currentTokenIndex].Raw} in a interpolated string",
                                parser.tokens[parser.currentTokenIndex]);
                        interpolatedNodes.Enqueue(interpolatedNode);

                    }
                    this.Expect(TokenType.INTERPOLATED_STRING);
                    return new InterpolatedString(interpolatedToken.Texts, interpolatedNodes);
                case TokenType.TRUE:
                    this.Expect(TokenType.TRUE);
                    return new BooleanNode(true);
                case TokenType.FALSE:
                    this.Expect(TokenType.FALSE);
                    return new BooleanNode(false);
                case TokenType.NULL:
                    this.Expect(TokenType.NULL);
                    return new NullNode();
                case TokenType.OPEN_ROUND_BRACKET:
                    {
                        // Check if it is a lambda function
                        int lastCurrentIndex = this.currentTokenIndex;
                        try
                        {
                            return FnDefinition(false, AccessibilityLevel.Public, true, false);
                        }
                        catch (NotLambdaParseException)
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
                        this.Expect(TokenType.OPERATOR);

                        // Unary operator
                        if (currentToken.Raw == "-" || currentToken.Raw == "+")
                        {
                            var expression = this.TermDot(skipIndexer, skipArrowFn);
                            if (expression is IntNode)
                            {
                                if (currentToken.Raw == "-")
                                    return new IntNode(-((IntNode)expression).Value);
                                else if (currentToken.Raw == "+")
                                    return new IntNode(+((IntNode)expression).Value);
                            }
                            else if (expression is FloatNode)
                            {
                                if (currentToken.Raw == "-")
                                    return new FloatNode(-((FloatNode)expression).Value);
                                else if (currentToken.Raw == "+")
                                    return new FloatNode(+((FloatNode)expression).Value);
                            }
                            return new UnaryNode(expression, currentToken.Raw);
                        }

                        else if (currentToken.Raw == "!")
                        {
                            var expression = this.TermDot(skipIndexer, skipArrowFn);
                            return new UnaryNode(expression, currentToken.Raw);
                        }
                        else if (currentToken.Raw == "*")
                        {
                            var expression = this.TermDot(skipIndexer, skipArrowFn);
                            return new UnaryNode(expression, currentToken.Raw);
                        }
                        else
                        {
                            throw new ParseException(
                                    "Unexpected token: " + currentToken.Raw + ", " + currentToken.Type,
                                    currentToken);
                        }
                    }

                case TokenType.OPEN_SQURE_BRACKET:
                    {
                        List<AstNode> elements = new List<AstNode>();

                        this.Expect(TokenType.OPEN_SQURE_BRACKET);

                        while (this.HasToken && this.CurrentToken.Type != TokenType.CLOSE_SQURE_BRACKET)
                        {
                            elements.Add(this.Expression());

                            if (HasToken && CurrentToken.Type == TokenType.CLOSE_SQURE_BRACKET)
                                break;
                            else if (HasToken && CurrentToken.Type == TokenType.COMMA)
                                Expect(TokenType.COMMA);
                            else
                                throw new ParseFormatNotCompleteException("List expected ',' or ']'", this.tokens[this.currentTokenIndex - 1]);
                        }

                        this.Expect(TokenType.CLOSE_SQURE_BRACKET);

                        var thing = new ListNode(elements);

                        return CheckIndexerAccess(thing);
                    }
                case TokenType.OPEN_CURLY_BRACKET:
                    {
                        var keyValues = new List<Tuple<AstNode, AstNode>>();

                        this.Expect(TokenType.OPEN_CURLY_BRACKET);

                        while (this.HasToken && CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
                        {
                            AstNode key = this.Expression();
                            this.Expect(TokenType.COLON);
                            AstNode value = this.Expression();
                            keyValues.Add(Tuple.Create(key, value));

                            if (this.HasToken && CurrentToken.Type == TokenType.CLOSE_CURLY_BRACKET)
                                break;
                            else if (this.HasToken && CurrentToken.Type == TokenType.COMMA)
                                Expect(TokenType.COMMA);
                            else
                                throw new ParseFormatNotCompleteException("Dict expected ',' or '}'", this.tokens[this.currentTokenIndex - 1]);
                        }

                        this.Expect(TokenType.CLOSE_CURLY_BRACKET);

                        return new DictNode(keyValues);
                    }
                case TokenType.KEYWORD:
                    if (currentToken.Raw == "this" || currentToken.Raw == "super")
                        return this.Identifier(skipIndexer);
                    else if (currentToken.Raw == "catch")
                        return this.CatchExpression();
                    throw new ParseException($"Unexpected keyword: {currentToken.Raw}", currentToken);
                default:
                    break;
            }
            return this.Identifier(skipIndexer);
        }

        private AstNode TermDot(bool skipIndexer, bool skipArrowFn)
        {
            var left = this.Factor(skipIndexer, skipArrowFn);

            while (HasToken &&
                (CurrentToken.Type == TokenType.DOT || CurrentToken.Type == TokenType.QUESTION_DOT ||
                CurrentToken.Type == TokenType.OPEN_ROUND_BRACKET || CurrentToken.Type == TokenType.QUESTION_OPEN_ROUND_BRACKET ||
                (!skipIndexer && (CurrentToken.Type == TokenType.OPEN_SQURE_BRACKET || CurrentToken.Type == TokenType.QUESTION_OPEN_SQURE_BRACKET))))
            {
                if (CurrentToken.Type == TokenType.DOT)
                {
                    this.Expect(TokenType.DOT);
                    left = new ExpressionNode(left, this.Factor(skipIndexer, skipArrowFn), ".");
                }
                else if (CurrentToken.Type == TokenType.QUESTION_DOT)
                {
                    this.Expect(TokenType.QUESTION_DOT);
                    left = new ExpressionNode(left, this.Factor(skipIndexer, skipArrowFn), "?.");
                }
                // Function call
                else if (CurrentToken.Type == TokenType.OPEN_ROUND_BRACKET || CurrentToken.Type == TokenType.QUESTION_OPEN_ROUND_BRACKET)
                {
                    left = FunctionCall(left);
                }
                // Indexer
                else if (!skipIndexer && (CurrentToken.Type == TokenType.OPEN_SQURE_BRACKET || CurrentToken.Type == TokenType.QUESTION_OPEN_SQURE_BRACKET))
                {
                    left = this.CheckIndexerAccess(left);
                }
            }

            return left;
        }

        private AstNode TermArithmetic1(bool skipIndexer, bool skipArrowFn)
        {
            var left = this.TermDot(skipIndexer, skipArrowFn);

            while (HasToken &&
                (CurrentToken.Raw == "*" || CurrentToken.Raw == "/" || CurrentToken.Raw == "%"))
            {
                var currentToken = CurrentToken;
                this.Expect(TokenType.OPERATOR);
                left = new ExpressionNode(left, this.TermDot(skipIndexer, skipArrowFn), currentToken.Raw);
            }

            return left;
        }

        private AstNode TermArithmetic2(bool skipIndexer, bool skipArrowFn)
        {
            var left = this.TermArithmetic1(skipIndexer, skipArrowFn);

            while (HasToken &&
                (CurrentToken.Raw == "+" || CurrentToken.Raw == "-"))
            {
                var currentToken = CurrentToken;
                this.Expect(TokenType.OPERATOR);
                left = new ExpressionNode(left, this.TermArithmetic1(skipIndexer, skipArrowFn), currentToken.Raw);
            }

            return left;
        }

        private AstNode TermComparative(bool skipIndexer, bool skipArrowFn)
        {
            var left = this.TermArithmetic2(skipIndexer, skipArrowFn);

            while (HasToken &&
                (CurrentToken.Raw == ">=" || CurrentToken.Raw == "<=" || CurrentToken.Raw == ">" || CurrentToken.Raw == "<"))
            {
                var currentToken = CurrentToken;
                this.Expect(TokenType.OPERATOR);
                left = new ExpressionNode(left, this.TermArithmetic2(skipIndexer, skipArrowFn), currentToken.Raw);
            }

            return left;
        }

        private AstNode TermEqual(bool skipIndexer, bool skipArrowFn)
        {
            var left = this.TermComparative(skipIndexer, skipArrowFn);

            while (HasToken &&
                (CurrentToken.Raw == "==" || CurrentToken.Raw == "!="))
            {
                if (CurrentToken.Raw == "==" || CurrentToken.Raw == "!=")
                {
                    var currentToken = CurrentToken;
                    this.Expect(TokenType.OPERATOR);
                    left = new ExpressionNode(left, this.TermComparative(skipIndexer, skipArrowFn), currentToken.Raw);
                }
            }

            return left;
        }

        private AstNode TermAnd(bool skipIndexer, bool skipArrowFn)
        {
            var left = this.TermEqual(skipIndexer, skipArrowFn);

            while (HasToken &&
                (CurrentToken.Raw == "&&"))
            {
                var currentToken = CurrentToken;
                this.Expect(TokenType.OPERATOR);
                left = new ExpressionNode(left, this.TermEqual(skipIndexer, skipArrowFn), currentToken.Raw);
            }

            return left;
        }

        private AstNode TermOr(bool skipIndexer, bool skipArrowFn)
        {
            var left = this.TermAnd(skipIndexer, skipArrowFn);

            while (HasToken &&
                (CurrentToken.Raw == "||"))
            {
                var currentToken = CurrentToken;
                this.Expect(TokenType.OPERATOR);
                left = new ExpressionNode(left, this.TermAnd(skipIndexer, skipArrowFn), currentToken.Raw);
            }

            return left;
        }

        private AstNode Expression(bool skipIndexer = false, bool skipEquals = false, bool skipArrowFn = false)
        {
            if (!HasToken)
                throw new ParseFormatNotCompleteException("Format is not complete", this.tokens[this.currentTokenIndex - 1]);

            var left = this.TermOr(skipIndexer, skipArrowFn);

            if (!HasToken)
                return left;

            Token currentToken = this.tokens[this.currentTokenIndex];
            string currentRaw = currentToken.Raw;

            if (this.HasToken &&
                currentToken.Type == TokenType.QUESTION_MARK)
            {
                var condition = left;
                this.Expect(TokenType.QUESTION_MARK);
                var consequent = this.Expression();
                this.Expect(TokenType.COLON);
                var alternative = this.Expression();
                left = new TernaryNode(condition, consequent, alternative);
            }

            if (this.HasToken && currentToken.Type == TokenType.DOUBLE_QUESTION_MARK)
            {
                this.Expect(TokenType.DOUBLE_QUESTION_MARK);
                var consequent = this.Expression();
                left = new NullCoalescingNode(left, consequent);
            }

            if (this.HasToken && (currentRaw == "++" || currentRaw == "--"))
            {
                this.Expect(TokenType.OPERATOR);
                return new VariableAssignmentNode(left, new ExpressionNode(left, new IntNode(1), currentRaw[0].ToString()));
            }
            if (this.HasToken &&
                (currentRaw == "+=" || currentRaw == "-=" || currentRaw == "*=" || currentRaw == "/=" || currentRaw == "%="))
            {
                this.Expect(TokenType.OPERATOR);

                return new VariableAssignmentNode(left, new ExpressionNode(left, this.Expression(), currentRaw.TrimEnd('=')));
            }

            if (!skipEquals && currentToken.Type == TokenType.EQUALS)
            {
                this.Expect(TokenType.EQUALS);
                if (left is ListNode)
                    return new VariableAssignmentNode(((ListNode)left).Elements, this.Expression());
                else
                    return new VariableAssignmentNode(left, this.Expression());
            }

            if (currentToken.Type == TokenType.SEMI_COLON)
            {
                this.Expect(TokenType.SEMI_COLON);
                return left;
            }

            return left;
        }

        private AstNode ParseTypes()
        {
            List<AstNode> types = new List<AstNode> { TermDot(true, true) };
            if (HasToken && (this.CurrentToken.Type == TokenType.QUESTION_MARK))
            {
                this.Expect(TokenType.QUESTION_MARK);
                types.Add(new NullNode());
            }

            while (HasToken && (this.CurrentToken.Raw == "|"))
            {
                this.Expect(TokenType.OPERATOR);
                types.Add(TermDot(true, true));
                if (HasToken && (this.CurrentToken.Type == TokenType.QUESTION_MARK))
                {
                    this.Expect(TokenType.QUESTION_MARK);
                    types.Add(new NullNode());
                }
            }
            return new TypeNode(types);
        }


        private AstNode VariableDefinition(bool constant, bool isStatic, AccessibilityLevel level)
        {
            bool isClassMember = this.parseScopes.Count > 0 && this.parseScopes.Peek() == ParseScope.Class;

            if (constant)
                isStatic = true;

            if (!isClassMember)
                isStatic = true;

            Token keywordToken = this.Expect(TokenType.KEYWORD);

            List<string> variableNames = new List<string>();

            bool hasSquareBracket = false;
            if (this.HasToken && this.CurrentToken.Type == TokenType.OPEN_SQURE_BRACKET)
            {
                this.Expect(TokenType.OPEN_SQURE_BRACKET);
                hasSquareBracket = true;
            }
            while (this.HasToken && this.CurrentToken.Type == TokenType.IDENTIFIER)
            {
                variableNames.Add(this.CurrentToken.Raw);
                this.Expect(TokenType.IDENTIFIER);
                if (this.HasToken && this.CurrentToken.Type != TokenType.COMMA)
                    break;
                this.Expect(TokenType.COMMA);
            }
            if (hasSquareBracket)
            {
                this.Expect(TokenType.CLOSE_SQURE_BRACKET);
            }

            // Check if datatype defined
            AstNode dataType = null;
            if (this.HasToken && this.CurrentToken.Type == TokenType.COLON)
            {
                this.Expect(TokenType.COLON);
                dataType = this.ParseTypes();
                if (this.HasToken && this.CurrentToken.Type == TokenType.COMMA)
                {
                    throw new ParseException("Variable can only define one type each time", this.CurrentToken);
                }
            }

            // check if it is get set variable
            AccessibilityLevel? lastLevel = null;
            FnDefineNode getFn = null;
            FnDefineNode setFn = null;
            if (this.HasToken && this.CurrentToken.Type == TokenType.EQUALS)
            {
                this.Expect(TokenType.EQUALS);

                List<AstNode> toDefined = new List<AstNode>() { this.Expression() };

                while (this.HasToken && this.CurrentToken.Type == TokenType.COMMA)
                {
                    this.Expect(TokenType.COMMA);
                    toDefined.Add(this.Expression());
                }
                if (toDefined.Count == 1)
                {
                    var valueNode1 = new VariableDefinitionNode(variableNames, toDefined[0], dataType, null, null, constant, isStatic, level);
                    valueNode1.Line = keywordToken.Line;
                    return valueNode1;
                }
                if (variableNames.Count != toDefined.Count)
                    throw new ParseException($"Cannot define {toDefined.Count} values to {variableNames.Count} variable{(variableNames.Count > 1 ? "s" : "")}", LastToken);
                
                var valueNode2 = new VariableDefinitionNode(variableNames, new ListNode(toDefined), dataType, null, null, constant, isStatic, level);
                valueNode2.Line = keywordToken.Line;
                return valueNode2;
            }
            else if (this.HasToken && this.CurrentToken.Type == TokenType.OPEN_CURLY_BRACKET)
            {
                this.Expect(TokenType.OPEN_CURLY_BRACKET);
                while (this.HasToken &&
                    this.CurrentToken.Type == TokenType.IDENTIFIER ||
                    this.CurrentToken.Type == TokenType.KEYWORD)
                {
                    var keyword = this.CurrentToken.Raw;
                    this.Expect(TokenType.IDENTIFIER, TokenType.KEYWORD);

                    // Body.Count = 0 : Auto Implemented
                    List<AstNode> body = new List<AstNode>();
                    if (keyword.Equals("public") || keyword.Equals("protected") ||
                            keyword.Equals("private"))
                    {
                        if (lastLevel.HasValue)
                            throw new ParseException("Cannot redefine accessibility levels", this.tokens[this.currentTokenIndex - 1]);
                        lastLevel = ToAccessibilityLevel(keyword);
                    }
                    else if (keyword.Equals("get") || keyword.Equals("set"))
                    {
                        bool isGet = keyword.Equals("get");
                        if (isGet && getFn != null)
                            throw new ParseException($"Cannot redefine get function", this.tokens[this.currentTokenIndex - 1]);
                        if (!isGet && setFn != null)
                            throw new ParseException($"Cannot redefine set function", this.tokens[this.currentTokenIndex - 1]);

                        if (HasToken && CurrentToken.Type == TokenType.DOUBLE_ARROW)
                        {
                            this.parseScopes.Push(ParseScope.Fn);
                            this.Expect(TokenType.DOUBLE_ARROW);
                            var fnBody = this.ParseChild();

                            if (isGet)
                                body.Add(new ReturnNode(fnBody));
                            else
                                body.Add(fnBody);
                            this.parseScopes.Pop();
                        }
                        else if (HasToken && CurrentToken.Type == TokenType.OPEN_CURLY_BRACKET)
                        {
                            this.parseScopes.Push(ParseScope.Fn);
                            this.Expect(TokenType.OPEN_CURLY_BRACKET);

                            while (HasToken && CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
                            {
                                AstNode child = this.ParseChild();
                                if (child != null)
                                    body.Add(child);
                            }

                            this.Expect(TokenType.CLOSE_CURLY_BRACKET);
                            this.parseScopes.Pop();
                        }
                        else
                        {
                            this.Expect(TokenType.SEMI_COLON);
                        }

                        if (lastLevel.HasValue && (int)lastLevel.Value <= (int)level)
                        {
                            throw new ParseException($"Cannot define getter/setter with the same or a larger accessibility level",
                                this.tokens[this.currentTokenIndex - 1]);
                        }

                        if (isGet)
                        {
                            var parameters = new List<FnDefineParameter>();
                            if (isClassMember && !isStatic)
                            {
                                parameters.Add(new FnDefineParameter("this", null));
                            }
                            getFn = new FnDefineStatementNode($"get_{string.Join("_", variableNames)}", parameters, dataType, body, isStatic, lastLevel ?? level);
                        }
                        else
                        {
                            var parameters = new List<FnDefineParameter>() { new FnDefineParameter("value", dataType) };
                            if (isClassMember && !isStatic)
                            {
                                parameters.Insert(0, new FnDefineParameter("this", null));
                            }
                            setFn = new FnDefineStatementNode($"set_{string.Join("_", variableNames)}", parameters, new NullNode(), body, isStatic, lastLevel ?? level);
                        }
                        lastLevel = null;
                    }
                    else
                    {
                        throw new ParseException($"Unexpected keyword {keyword}", this.tokens[this.currentTokenIndex - 1]);
                    }
                }
                this.Expect(TokenType.CLOSE_CURLY_BRACKET);
            }
            else if (this.HasToken && this.CurrentToken.Type == TokenType.DOUBLE_ARROW)
            {
                this.Expect(TokenType.DOUBLE_ARROW);
                var fnBody = new ReturnNode(this.ParseChild());

                var parameters = new List<FnDefineParameter>();
                if (isClassMember && !isStatic)
                {
                    parameters.Add(new FnDefineParameter("this", null));
                }
                getFn = new FnDefineStatementNode($"get_{string.Join("_", variableNames)}", parameters, dataType, new List<AstNode>() { fnBody }, isStatic, level);
            }
            else
            {
                throw new ParseException("Unexpected Variable format.", this.HasToken ? this.CurrentToken : this.LastToken);
            }

            if (getFn != null && setFn != null)
            {
                if (getFn.Body.Count > 0 && setFn.Body.Count == 0)
                    throw new ParseException("Setter is auto-implemented variable but Getter is not", this.tokens[this.currentTokenIndex - 1]);
                else if (setFn.Body.Count > 0 && getFn.Body.Count == 0)
                    throw new ParseException("Getter is auto-implemented variable but Setter is not", this.tokens[this.currentTokenIndex - 1]);

                if (constant)
                    throw new ParseException("Constant cannot have Setter", this.tokens[this.currentTokenIndex - 1]);
            }

            var node = new VariableDefinitionNode(variableNames, null, dataType, getFn, setFn, constant, isStatic, level);
            if (this.currentTokenIndex >= this.tokens.Count)
                node.Line = this.tokens[this.tokens.Count - 1].Line;
            else
                node.Line = this.CurrentToken.Line;
            return node;
        }

        private AstNode IfStatement()
        {
            Token keywordToken = this.Expect(TokenType.KEYWORD);

            var condition = this.Expression();
            List<AstNode> thenBody = new List<AstNode>();
            List<AstNode> elseBody = new List<AstNode>();

            if (!DISABLE_ARROW_IF && HasToken && CurrentToken.Type == TokenType.DOUBLE_ARROW)
            {
                this.parseScopes.Push(ParseScope.Condition);
                this.Expect(TokenType.DOUBLE_ARROW);
                AstNode child = this.ParseChild();
                if (child != null)
                    thenBody.Add(child);
                this.parseScopes.Pop();
            }
            else
            {
                this.parseScopes.Push(ParseScope.Condition);
                this.Expect(TokenType.OPEN_CURLY_BRACKET);

                while (HasToken && CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
                {
                    AstNode child = this.ParseChild();
                    if (child != null)
                        thenBody.Add(child);
                }

                this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
                this.parseScopes.Pop();
            }

            if (HasToken && this.CurrentToken.Raw == "else")
            {
                this.currentTokenIndex++;

                if (HasToken && this.CurrentToken.Raw == "if")
                {
                    elseBody.Add(this.IfStatement());
                }
                else
                {
                    if (!DISABLE_ARROW_IF && HasToken && CurrentToken.Type == TokenType.DOUBLE_ARROW)
                    {
                        this.parseScopes.Push(ParseScope.Condition);
                        this.Expect(TokenType.DOUBLE_ARROW);
                        AstNode child = this.ParseChild();
                        if (child != null)
                            elseBody.Add(child);
                        this.parseScopes.Pop();
                    }
                    else
                    {
                        this.parseScopes.Push(ParseScope.Condition);
                        this.Expect(TokenType.OPEN_CURLY_BRACKET);

                        while (HasToken && CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
                        {
                            AstNode child = this.ParseChild();
                            if (child != null)
                                elseBody.Add(child);
                        }

                        this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
                        this.parseScopes.Pop();
                    }
                }
            }

            return new IfNode(condition, thenBody, elseBody);
        }
        private void OutTryCatchBody(List<AstNode> body)
        {
            this.parseScopes.Push(ParseScope.TryCatchFinally);
            this.Expect(TokenType.OPEN_CURLY_BRACKET);

            while (HasToken && CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
            {
                AstNode child = this.ParseChild();
                if (child != null)
                    body.Add(child);
            }

            this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
            this.parseScopes.Pop();
        }

        private AstNode TryCatchStatement()
        {
            this.Expect(TokenType.KEYWORD);
            List<AstNode> tryBody = new List<AstNode>();
            List<CatchNode> catchNodes = null;
            List<AstNode> finallyBody = null;

            OutTryCatchBody(tryBody);

            while (HasToken && CurrentToken.Raw.Equals("catch"))
            {
                this.Expect(TokenType.KEYWORD);
                if (HasToken && CurrentToken.Type == TokenType.OPEN_ROUND_BRACKET)
                    this.Expect(TokenType.OPEN_ROUND_BRACKET);

                string paramName = "";
                AstNode paramType = null;

                bool hasParam = HasToken && CurrentToken.Type == TokenType.IDENTIFIER;
                if (hasParam)
                {
                    paramName = CurrentToken.Raw;
                    this.Expect(TokenType.IDENTIFIER);

                    if (HasToken && CurrentToken.Type == TokenType.COLON)
                    {
                        this.Expect(TokenType.COLON);
                        paramType = this.ParseTypes();
                    }
                }

                if (HasToken && CurrentToken.Type == TokenType.CLOSE_ROUND_BRACKET)
                    this.Expect(TokenType.CLOSE_ROUND_BRACKET);

                var catchBody = new List<AstNode>();
                OutTryCatchBody(catchBody);

                if (catchNodes == null)
                    catchNodes = new List<CatchNode>();

                catchNodes.Add(new CatchNode(paramName, paramType, catchBody));
            }

            if (HasToken && CurrentToken.Raw.Equals("finally"))
            {
                this.Expect(TokenType.KEYWORD);
                finallyBody = new List<AstNode>();
                OutTryCatchBody(finallyBody);
            }

            if (catchNodes == null && finallyBody == null)
                throw new ParseException("try keyword must work with catch or finally", this.tokens[this.currentTokenIndex - 1]);


            if (catchNodes != null && catchNodes.Where(node => node.DataType == null).Count() > 1)
                throw new ParseException("cannot define 2 type not defined catch", this.tokens[this.currentTokenIndex - 1]);

            return new TryCatchNode(tryBody, catchNodes, finallyBody);
        }
        private AstNode WhileStatement()
        {
            Token keywordToken = this.Expect(TokenType.KEYWORD);

            var condition = this.Expression();
            List<AstNode> thenStatements = new List<AstNode>();

            if (!DISABLE_ARROW_LOOP && HasToken && CurrentToken.Type == TokenType.DOUBLE_ARROW)
            {
                this.parseScopes.Push(ParseScope.Loop);
                this.Expect(TokenType.DOUBLE_ARROW);
                AstNode child = this.ParseChild();
                if (child != null)
                    thenStatements.Add(child);
                this.parseScopes.Pop();
            }
            else
            {
                this.parseScopes.Push(ParseScope.Loop);
                this.Expect(TokenType.OPEN_CURLY_BRACKET);

                while (this.HasToken &&
                       this.CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
                {
                    AstNode child = this.ParseChild();
                    if (child != null)
                        thenStatements.Add(child);
                }
                this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
                this.parseScopes.Pop();
            }

            return new WhileNode(condition, thenStatements);
        }
        private AstNode ForStatement()
        {
            Token keywordToken = this.Expect(TokenType.KEYWORD);

            bool hasBracket = false;
            if (HasToken && CurrentToken.Type == TokenType.OPEN_ROUND_BRACKET)
            {
                hasBracket = true;
                this.Expect(TokenType.OPEN_ROUND_BRACKET);
            }

            List<string> initializer = new List<string>();
            while (this.HasToken && this.CurrentToken.Type == TokenType.IDENTIFIER)
            {
                initializer.Add(this.CurrentToken.Raw);
                this.Expect(TokenType.IDENTIFIER);
                if (this.HasToken && this.CurrentToken.Type != TokenType.COMMA)
                    break;
                this.Expect(TokenType.COMMA);
            }

            if (HasToken && this.CurrentToken.Raw != "in")
            {
                throw new ParseException("Keyword 'in' is essential in for loop", this.tokens[this.currentTokenIndex - 1]);
            }

            this.Expect(TokenType.KEYWORD);

            var iterable = this.Expression();

            if (hasBracket)
                Expect(true, TokenType.CLOSE_ROUND_BRACKET);

            List<AstNode> thenStatements = new List<AstNode>();

            if (!DISABLE_ARROW_LOOP && HasToken && CurrentToken.Type == TokenType.DOUBLE_ARROW)
            {
                this.parseScopes.Push(ParseScope.Loop);
                this.Expect(TokenType.DOUBLE_ARROW);
                AstNode child = this.ParseChild();
                if (child != null)
                    thenStatements.Add(child);
                this.parseScopes.Pop();
            }
            else
            {
                this.parseScopes.Push(ParseScope.Loop);
                this.Expect(TokenType.OPEN_CURLY_BRACKET);

                while (HasToken && CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
                {
                    AstNode child = this.ParseChild();
                    if (child != null)
                        thenStatements.Add(child);
                }

                this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
                this.parseScopes.Pop();
            }

            return new ForNode(initializer, iterable, thenStatements);
        }
        private AstNode ReturnStatement()
        {
            Token keywordToken = this.Expect(TokenType.KEYWORD);

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
            Token keywordToken = this.Expect(TokenType.KEYWORD);

            var currentToken = this.tokens[this.currentTokenIndex];

            var expression = this.Expression();
            expression.Line = currentToken.Line;
            return new ThrowNode(expression);
        }

        private AstNode CatchExpression()
        {
            AstNode defaultValue = null;
            Token keywordToken = this.Expect(TokenType.KEYWORD);
            this.Expect(TokenType.OPEN_ROUND_BRACKET);
            var currentToken = this.tokens[this.currentTokenIndex];
            var expression = this.Expression();
            if (this.HasToken && this.CurrentToken.Type == TokenType.COMMA)
            {
                this.Expect(TokenType.COMMA);
                defaultValue = this.Expression();
            }
            this.Expect(TokenType.CLOSE_ROUND_BRACKET);
            expression.Line = currentToken.Line;
            return new CatchExpressionNode(expression, defaultValue);
        }

        private AstNode ImportStatement()
        {
            Token keywordToken = this.Expect(TokenType.KEYWORD);

            List<Tuple<string, string>> imports = null;
            bool isImportAll = false;
            if (this.CurrentToken.Type == TokenType.IDENTIFIER || this.CurrentToken.Raw == "*")
            {
                imports = new List<Tuple<string, string>>();
                while (this.HasToken && (this.CurrentToken.Type == TokenType.IDENTIFIER || this.CurrentToken.Type == TokenType.OPERATOR))
                {
                    if (this.CurrentToken.Type == TokenType.OPERATOR)
                    {
                        if (this.CurrentToken.Raw != "*")
                            throw new ParseException($"Unexpected operator: {this.CurrentToken.Raw}", this.CurrentToken);
                        isImportAll = true;
                        if (imports.Count > 0)
                            throw new ParseException("import * cannot use with other identifier", this.CurrentToken);
                        this.Expect(TokenType.OPERATOR);
                        if (this.HasToken && this.CurrentToken.Raw == "as")
                            throw new ParseException("import * cannot use as keyword", this.CurrentToken);
                    }
                    else
                    {
                        string importItem = this.CurrentToken.Raw;
                        if (imports.FindIndex(x => x.Item1 == this.CurrentToken.Raw) >= 0)
                            throw new ParseException($"import {this.CurrentToken.Raw} is exists", this.CurrentToken);
                        this.Expect(TokenType.IDENTIFIER);
                        string itemAsName = null;
                        if (this.HasToken && this.CurrentToken.Raw == "as")
                        {
                            this.Expect(TokenType.KEYWORD);
                            this.Expect(TokenType.IDENTIFIER);
                            itemAsName = this.LastToken.Raw;
                            if (imports.FindIndex(x => x.Item2 == this.CurrentToken.Raw) >= 0)
                                throw new ParseException($"import as {this.CurrentToken.Raw} is exists", this.CurrentToken);
                        }
                        imports.Add(Tuple.Create(importItem, itemAsName));
                    }

                    if (this.HasToken && this.CurrentToken.Type == TokenType.COMMA)
                    {
                        this.Expect(TokenType.COMMA);
                        if (isImportAll)
                            throw new ParseException("import * cannot use with other identifier", this.LastToken);
                    }
                    else
                    {
                        break;
                    }
                }
                if (this.CurrentToken.Raw != "from")
                    throw new ParseException("Correct format: import a, b, c from \"\";", this.CurrentToken);
                this.Expect(TokenType.KEYWORD);
            }

            var importPath = this.CurrentToken.Raw;
            this.Expect(TokenType.STRING);

            string asName = null;
            if (this.HasToken && this.CurrentToken.Raw == "as")
            {
                this.Expect(TokenType.KEYWORD);
                if (imports != null)
                    throw new ParseException("Cannot use as keyword after format: import ... from \"\";", this.LastToken);
                this.Expect(TokenType.IDENTIFIER);
                asName = this.LastToken.Raw;
            }

            return new ImportNode(importPath, imports, asName);
        }

        private AstNode FnDefinition(bool isStatic, AccessibilityLevel level, bool isLambdaFn = false, bool isOneParam = false)
        {
            bool isClassMember = this.parseScopes.Count > 0 && this.parseScopes.Peek() == ParseScope.Class;

            if (!isClassMember)
                isStatic = true;

            if (isLambdaFn)
                isStatic = true;

            if (!isLambdaFn)
            {
                this.Expect(TokenType.KEYWORD);
            }

            string functionName = "";
            if (!isLambdaFn)
            {
                Token functionNameToken = this.Expect(TokenType.IDENTIFIER);
                functionName = functionNameToken.Raw;
            }

            var parameters = new List<FnDefineParameter>();
            List<AstNode> body = new List<AstNode>();
            bool isLastDefaultValue = false;
            Token multiArgsToken = null;
            if (!isOneParam)
            {
                this.Expect(TokenType.OPEN_ROUND_BRACKET);
                if (CurrentToken.Type != TokenType.IDENTIFIER && CurrentToken.Type != TokenType.KEYWORD && CurrentToken.Type != TokenType.CLOSE_ROUND_BRACKET)
                    throw new NotLambdaParseException("Expected identifier, keyword, close round bracket", CurrentToken);

                while (this.HasToken && this.CurrentToken.Type != TokenType.CLOSE_ROUND_BRACKET)
                {
                    if (multiArgsToken != null)
                        throw new ParseException($"Cannot have parameter after multiple-arguments", multiArgsToken);

                    bool IsMultiArgs = false;
                    var currentToken = this.CurrentToken;

                    string paramName = null;

                    if (currentToken.Type == TokenType.KEYWORD)
                    {
                        if (currentToken.Raw == "this")
                        {
                            if (isLambdaFn)
                                throw new ParseException($"Unexpect keyword: {currentToken.Raw}, in lambda fn", currentToken);
                            if (!isClassMember)
                                throw new ParseException($"Unexpect keyword: {currentToken.Raw}, outside of class", currentToken);
                            if (parameters.Count > 0)
                                throw new ParseException($"Unexpect keyword: {currentToken.Raw}, must be the first parameter", currentToken);
                            if (isStatic)
                                throw new ParseException($"Unexpect keyword: {currentToken.Raw}, in static fn", currentToken);

                            paramName = "this";
                        }
                        else if (currentToken.Raw == "params")
                        {
                            multiArgsToken = currentToken;
                            this.Expect(TokenType.KEYWORD);
                            IsMultiArgs = true;
                        }
                        else
                        {
                            throw new ParseException($"Unexpect keyword: {currentToken.Raw}", currentToken);
                        }
                    }

                    if (paramName == null)
                    {
                        currentToken = this.CurrentToken;

                        if (currentToken.Type != TokenType.IDENTIFIER)
                            throw new ParseException("Expected identifier", currentToken);

                        paramName = currentToken.Raw;
                    }

                    AstNode paramType = null;
                    AstNode paramDefaultValue = null;
                    this.currentTokenIndex++;
                    if (this.HasToken && this.CurrentToken.Type == TokenType.COLON)
                    {
                        this.Expect(TokenType.COLON);
                        paramType = this.ParseTypes();
                    }

                    if (this.HasToken && this.CurrentToken.Type == TokenType.EQUALS)
                    {
                        this.Expect(TokenType.EQUALS);
                        paramDefaultValue = Expression();
                        isLastDefaultValue = true;
                    }
                    else
                    {
                        if (isLastDefaultValue)
                        {
                            throw new ParseException("Function default value cannot in the middle", this.tokens[this.currentTokenIndex - 1]);
                        }
                    }

                    parameters.Add(new FnDefineParameter(paramName, paramType, paramDefaultValue, IsMultiArgs));

                    if (this.currentTokenIndex != this.tokens.Count)
                    {
                        if (this.HasToken && this.CurrentToken.Type != TokenType.CLOSE_ROUND_BRACKET)
                        {
                            if (this.CurrentToken.Type != TokenType.COMMA)
                                throw new NotLambdaParseException("", this.CurrentToken);
                            this.Expect(TokenType.COMMA);
                        }
                    }
                }

                this.Expect(true, TokenType.CLOSE_ROUND_BRACKET);
            }
            else
            {
                if (this.HasToken && this.CurrentToken.Type != TokenType.IDENTIFIER)
                    throw new ParseException("Expected identifier" , this.CurrentToken);

                string paramName = this.CurrentToken.Raw;
                this.Expect(TokenType.IDENTIFIER);

                AstNode paramType = null;
                AstNode paramValue = null;

                if (this.HasToken && this.CurrentToken.Type == TokenType.COLON)
                {
                    this.Expect(TokenType.COLON);
                    paramType = Expression();
                }

                if (this.HasToken && 
                    this.CurrentToken.Type == TokenType.EQUALS)
                {
                    this.Expect(TokenType.EQUALS);
                    paramValue = Expression();
                }
                parameters.Add(new FnDefineParameter(paramName, paramType, paramValue));
            }

            AstNode returnType = null;
            if (this.HasToken && 
                this.CurrentToken.Type == TokenType.SINGLE_ARROW)
            {
                this.Expect(TokenType.SINGLE_ARROW);
                returnType = this.ParseTypes();
            }


            bool hasArrow = false;
            bool hasBracket = true;
            if (this.HasToken && this.CurrentToken.Type == TokenType.DOUBLE_ARROW)
            {
                this.currentTokenIndex++;
                hasArrow = true;
            }

            if (isLambdaFn && (!hasArrow && returnType == null))
            {
                throw new NotLambdaParseException("Lambda function must define return type or use double arrow", this.LastToken);
            }

            if (this.HasToken && 
                this.CurrentToken.Type != TokenType.OPEN_CURLY_BRACKET)
                hasBracket = false;

            if (hasBracket)
            {
                this.parseScopes.Push(ParseScope.Fn);
                this.Expect(TokenType.OPEN_CURLY_BRACKET);

                while (this.HasToken &&
                        this.CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
                {
                    AstNode child = this.ParseChild();
                    if (child != null)
                        body.Add(child);
                }

                this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
                this.parseScopes.Pop();
            }
            else
            {
                this.parseScopes.Push(ParseScope.Fn);
                if (!hasArrow)
                    throw new ParseException("Function cannot define without arrow and curly bracket ", this.LastToken);
                AstNode child = this.ParseChild();
                if (child != null)
                    body.Add(new ReturnNode(child));
                this.parseScopes.Pop();
            }

            if (isClassMember && !isStatic && (parameters.Count <= 0 || parameters[0].Name != "this"))
                throw new ParseException("Non-static class fn without this parameter", this.LastToken);

            if (isLambdaFn)
                return new FnDefineExpressionNode(functionName, parameters, returnType, body, isStatic, level);
            else
                return new FnDefineStatementNode(functionName, parameters, returnType, body, isStatic, level);
        }

        private AstNode SwitchStatement()
        {
            Token keywordToken = this.Expect(TokenType.KEYWORD);

            var expression = this.Expression();
            this.parseScopes.Push(ParseScope.Condition);
            this.Expect(TokenType.OPEN_CURLY_BRACKET);

            List<SwitchCaseNode> cases = new List<SwitchCaseNode>();
            SwitchCaseNode defaultCase = null;

            while (this.HasToken && 
                this.CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
            {
                var token = this.tokens[this.currentTokenIndex];
                bool isDefaultCase = false;
                List<AstNode> caseExpressions = new List<AstNode>();

                // Every cases with same body
                while (HasToken && CurrentToken.Type == TokenType.KEYWORD)
                {
                    if (token.Raw != "case" && token.Raw != "default")
                        throw new ParseException($"switch only accept case or default as keyword", token);

                    this.Expect(TokenType.KEYWORD);

                    if (token.Raw == "case")
                    {
                        AstNode caseExpression = this.Expression();
                        caseExpression.Line = token.Line;
                        caseExpressions.Add(caseExpression);
                    }
                    else
                    {
                        if (defaultCase != null)
                            throw new ParseException("switch default case cannot define more than 1 time", token);
                        isDefaultCase = true;
                    }
                }

                var statements = new List<AstNode>();
                if (!DISABLE_ARROW_SWITCH && HasToken && CurrentToken.Type == TokenType.DOUBLE_ARROW)
                {
                    this.parseScopes.Push(ParseScope.Condition);
                    this.Expect(TokenType.DOUBLE_ARROW);
                    AstNode child = this.ParseChild();
                    if (child != null)
                        statements.Add(child);
                    statements.Add(new BreakNode());
                    this.parseScopes.Pop();
                }
                else
                {
                    this.parseScopes.Push(ParseScope.Condition);
                    this.Expect(TokenType.OPEN_CURLY_BRACKET);

                    while (HasToken && CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
                    {
                        AstNode child = this.ParseChild();
                        if (child != null)
                            statements.Add(child);
                    }
                    this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
                    this.parseScopes.Pop();
                }


                var switchCase = new SwitchCaseNode(caseExpressions, statements);
                if (isDefaultCase)
                    defaultCase = switchCase;
                else
                    cases.Add(switchCase);
            }

            this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
            this.parseScopes.Pop();

            return new SwitchNode(expression, cases, defaultCase);

        }
        private AstNode ClassDefinition(bool isStatic, AccessibilityLevel level)
        {
            Token keywordToken = this.Expect(TokenType.KEYWORD);

            var name = this.CurrentToken.Raw;

            this.Expect(TokenType.IDENTIFIER);
            List<AstNode> superClasses = new List<AstNode>();

            if (HasToken && CurrentToken.Type == TokenType.COLON)
            {
                bool end = false;
                this.Expect(TokenType.COLON);
                while (!end)
                {
                    superClasses.Add(this.Expression(true, true, true));
                    if (HasToken && CurrentToken.Type == TokenType.COMMA)
                        this.Expect(TokenType.COMMA);
                    else
                        end = true;
                }
            }
            this.parseScopes.Push(ParseScope.Class);
            this.Expect(TokenType.OPEN_CURLY_BRACKET);

            var members = new List<AstNode>();

            while (HasToken && CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
            {
                AstNode child = this.ParseChild();
                if (child != null)
                    members.Add(child);
            }

            this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
            this.parseScopes.Pop();
            //this.currentTokenIndex++;

            if (!members.All(x => x is FnDefineNode || x is VariableDefinitionNode || x is ClassDefineNode))
            {
                throw new ParseException("Class can only contains definition but not implement", keywordToken);
            }

            if (members.Where(x => x is VariableDefinitionNode && ((VariableDefinitionNode)x).Names.Count > 1).Count() > 0) 
            {
                throw new ParseException("Class cannot define multiple variables in same statment", keywordToken);
            }

            if (members.Where(x => x is VariableDefinitionNode && ((VariableDefinitionNode)x).Names.Contains(name)).Count() > 0)
            {
                throw new ParseException($"Class cannot define variable with class name ({name})", keywordToken);
            }

            foreach (var fn in members.Where(x => x is FnDefineStatementNode).Select(x => (FnDefineStatementNode)x).Where(fn => fn.Name.Equals(name)))
            {
                fn.ChangeToConstructorName(name);
            }

            return new ClassDefineNode(name, members, superClasses, isStatic, level);
        }

        private AstNode EnumDefinition(AccessibilityLevel level)
        {
            Token keywordToken = this.Expect(TokenType.KEYWORD);

            var name = this.CurrentToken.Raw;

            this.Expect(TokenType.IDENTIFIER);
            this.parseScopes.Push(ParseScope.Enum);
            this.Expect(TokenType.OPEN_CURLY_BRACKET);

            var members = new Dictionary<string, AstNode>();
            int count = 0;
            while (HasToken && CurrentToken.Type != TokenType.CLOSE_CURLY_BRACKET)
            {
                var key = this.CurrentToken.Raw;
                this.Expect(TokenType.IDENTIFIER);
                if (HasToken && CurrentToken.Type == TokenType.EQUALS)
                {
                    this.Expect(TokenType.EQUALS);
                    AstNode value = this.Expression();
                    members[key] = value;
                }
                else
                {
                    members[key] = new IntNode(count);
                    count++;
                }

                if (HasToken && CurrentToken.Type == TokenType.CLOSE_CURLY_BRACKET)
                    break;
                else if (HasToken && CurrentToken.Type == TokenType.COMMA)
                    Expect(TokenType.COMMA);
                else
                    throw new ParseFormatNotCompleteException("enum expected ',' or '}'", this.tokens[this.currentTokenIndex - 1]);
            }

            this.Expect(true, TokenType.CLOSE_CURLY_BRACKET);
            this.parseScopes.Pop();
            return new EnumDefineNode(name, members, level);
        }
        
        private AstNode FunctionCall(AstNode node)
        {
            Token bracketToken = this.Expect(TokenType.OPEN_ROUND_BRACKET, TokenType.QUESTION_OPEN_ROUND_BRACKET);

            var arguments = new List<AstNode>();
            var keyArguments = new Dictionary<string, AstNode>();
            bool isLastWithName = false;
            while (HasToken && CurrentToken.Type != TokenType.CLOSE_ROUND_BRACKET)
            {
                if (this.currentTokenIndex + 1 < this.tokens.Count &&
                    this.CurrentToken.Type == TokenType.IDENTIFIER &&
                    this.tokens[this.currentTokenIndex + 1].Type == TokenType.COLON)
                {
                    string argName = this.CurrentToken.Raw;
                    this.Expect(TokenType.IDENTIFIER);
                    this.Expect(TokenType.COLON);
                    keyArguments[argName] = this.Expression(false, true, false);
                    isLastWithName = true;
                }
                else
                {
                    // supporting multiple arguments
                    if (isLastWithName)
                        throw new ParseException("cannot pass argument without name after passing named argument", this.tokens[this.currentTokenIndex - 1]);

                    arguments.Add(this.Expression());
                }

                if (HasToken && CurrentToken.Type == TokenType.CLOSE_ROUND_BRACKET)
                    break;
                else if (HasToken && CurrentToken.Type == TokenType.COMMA)
                    Expect(TokenType.COMMA);
                else
                    throw new ParseException("Function call expected ',' or ')'", 
                        this.tokens[this.currentTokenIndex - 1]);
            }

            if (HasToken && CurrentToken.Type != TokenType.CLOSE_ROUND_BRACKET)
            {
                throw new ParseException("Function call expected ')'",
                    this.tokens[this.currentTokenIndex - 1]);
            }

            Expect(true, TokenType.CLOSE_ROUND_BRACKET);

            AstNode result = new FnReferenceCallNode(node, arguments, keyArguments, bracketToken.Type == TokenType.QUESTION_OPEN_ROUND_BRACKET);
            while (HasToken && 
                (CurrentToken.Type == TokenType.OPEN_SQURE_BRACKET || 
                CurrentToken.Type == TokenType.QUESTION_OPEN_SQURE_BRACKET || 
                CurrentToken.Type == TokenType.OPEN_ROUND_BRACKET ||
                CurrentToken.Type == TokenType.QUESTION_OPEN_ROUND_BRACKET))
            {
                if (CurrentToken.Type == TokenType.OPEN_SQURE_BRACKET || CurrentToken.Type == TokenType.QUESTION_OPEN_SQURE_BRACKET)
                {
                    result = CheckIndexerAccess(result);
                }
                else if (CurrentToken.Type == TokenType.OPEN_ROUND_BRACKET || CurrentToken.Type == TokenType.QUESTION_OPEN_ROUND_BRACKET)
                {
                    result = FunctionCall(result);
                }
            }

            return result;
        }
        private AstNode Identifier(bool skipIndexer)
        {
            var currentToken = Expect(TokenType.IDENTIFIER, TokenType.KEYWORD);
            AstNode result = new VariableReferenceNode(currentToken.Raw);
            result.Line = currentToken.Line;
            return result;
        }

        private AstNode CheckIndexerAccess(AstNode child)
        {
            if (!HasToken)
                return child;

            var startToken = this.CurrentToken;

            if (!(startToken.Type == TokenType.OPEN_SQURE_BRACKET || startToken.Type == TokenType.QUESTION_OPEN_SQURE_BRACKET))
                return child;

            this.Expect(TokenType.OPEN_SQURE_BRACKET, TokenType.QUESTION_OPEN_SQURE_BRACKET);

            List<List<AstNode>> slicer = new List<List<AstNode>>();
            List<AstNode> indexer = new List<AstNode>();

            while (true)
            {
                bool isIndexer = false;
                AstNode startNode = null;
                AstNode endNode = null;
                AstNode stepNode = null;
                if (this.HasToken && this.CurrentToken.Type == TokenType.COLON)
                {
                    startNode = new NullNode();
                    startNode.Line = startToken.Line;
                }
                else
                {
                    startNode = this.Expression();
                    startNode.Line = startToken.Line;
                }

                if (this.HasToken && this.CurrentToken.Type != TokenType.COLON)
                {
                    if (slicer.Count > 0)
                        throw new ParseException("Cannot mix slicer with indexer", startToken);
                    indexer.Add(startNode);
                    isIndexer = true;
                }

                if (!isIndexer)
                {
                    if (indexer.Count > 0)
                        throw new ParseException("Cannot mix indexer with slicer", startToken);

                    this.Expect(TokenType.COLON);
                    if (this.HasToken && this.CurrentToken.Type == TokenType.COLON)
                    {
                        endNode = new NullNode();
                        endNode.Line = startToken.Line;
                    }
                    else if (this.HasToken && (this.CurrentToken.Type == TokenType.COMMA || this.CurrentToken.Type == TokenType.CLOSE_SQURE_BRACKET))
                    {
                        endNode = new NullNode();
                        endNode.Line = startToken.Line;
                    }
                    else
                    {
                        endNode = this.Expression();
                        endNode.Line = startToken.Line;
                    }

                    if (this.HasToken && (this.CurrentToken.Type == TokenType.COMMA || this.CurrentToken.Type == TokenType.CLOSE_SQURE_BRACKET))
                    {
                        stepNode = new NullNode();
                        stepNode.Line = startToken.Line;
                    }
                    else
                    {
                        this.Expect(TokenType.COLON);
                        if (this.HasToken && (this.CurrentToken.Type == TokenType.COMMA || this.CurrentToken.Type == TokenType.CLOSE_SQURE_BRACKET))
                        {
                            stepNode = new NullNode();
                            stepNode.Line = startToken.Line;
                        }
                        else
                        {
                            stepNode = this.Expression();
                            stepNode.Line = startToken.Line;
                        }
                    }
                    slicer.Add(new List<AstNode>() { startNode, endNode, stepNode });
                }
                Token lastToken = this.Expect(TokenType.COMMA, TokenType.CLOSE_SQURE_BRACKET);
                if (lastToken.Type == TokenType.CLOSE_SQURE_BRACKET)
                    break;
            }

            AstNode result;
            if (indexer.Count > 0)
                result = new IndexerNode(child, indexer, startToken.Type == TokenType.QUESTION_OPEN_SQURE_BRACKET);
            else
                result = new SlicerNode(child, slicer, startToken.Type == TokenType.QUESTION_OPEN_SQURE_BRACKET);

            while (HasToken &&
                (CurrentToken.Type == TokenType.OPEN_SQURE_BRACKET ||
                CurrentToken.Type == TokenType.QUESTION_OPEN_SQURE_BRACKET ||
                CurrentToken.Type == TokenType.OPEN_ROUND_BRACKET ||
                CurrentToken.Type == TokenType.QUESTION_OPEN_ROUND_BRACKET))
            {
                if (CurrentToken.Type == TokenType.OPEN_SQURE_BRACKET || CurrentToken.Type == TokenType.QUESTION_OPEN_SQURE_BRACKET)
                {
                    result = CheckIndexerAccess(result);
                }
                else if (CurrentToken.Type == TokenType.OPEN_ROUND_BRACKET || CurrentToken.Type == TokenType.QUESTION_OPEN_ROUND_BRACKET)
                {
                    result = FunctionCall(result);
                }
            }
            return result;
        }


        private Token Expect(bool isFormatNotComplete, params TokenType[] types)
        {
            if (!HasToken)
            {
                if (isFormatNotComplete)
                    throw new ParseFormatNotCompleteException("Format is not complete", this.tokens[this.currentTokenIndex - 1]);
                else
                    throw new ParseException("Format is not complete", this.tokens[this.currentTokenIndex - 1]);
            }

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
                if (isFormatNotComplete)
                    throw new ParseFormatNotCompleteException(
                            "Unexpected token: " + currentToken.Raw + ". Expected: " + typesText,
                            currentToken);
                else
                    throw new ParseException(
                           "Unexpected token: " + currentToken.Raw + ". Expected: " + typesText,
                           currentToken);
            }
            return currentToken;
        }

        private Token Expect(params TokenType[] types)
        {
            return Expect(false, types);
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

            while (HasToken && CurrentToken.Type == TokenType.KEYWORD)
            {
                string keyword = this.CurrentToken.Raw;
                if (keyword.Equals("static"))
                {
                    this.Expect(TokenType.KEYWORD);
                    if (isStatic)
                        throw new ParseException("Already defined static", this.LastToken);
                    isStatic = true;
                }
                else if (keyword.Equals("var") || keyword.Equals("let") || keyword.Equals("const"))
                {
                    return this.VariableDefinition(keyword.Equals("const"), isStatic, accessibilityLevel);
                }
                else if (keyword.Equals("fn"))
                {
                    return this.FnDefinition(isStatic, accessibilityLevel, false, false);
                }
                else if (keyword.Equals("class"))
                {
                    return this.ClassDefinition(isStatic, accessibilityLevel);
                }
                else if (keyword.Equals("enum"))
                {
                    return this.EnumDefinition(accessibilityLevel);
                }
            }
            throw new ParseException($"Unexpected {level} defined",
                this.tokens[this.currentTokenIndex]);
        }

        private AstNode ParseChild()
        {
            var token = this.CurrentToken;

            switch (token.Type)
            {
                case TokenType.IDENTIFIER:
                    {
                        // for directly assign without []
                        // a, b = b, a
                        List<AstNode> references = new List<AstNode>() { this.Expression(skipEquals: true) };
                        int lastTokenIndex = this.currentTokenIndex;
                        while (this.HasToken && this.CurrentToken.Type == TokenType.COMMA)
                        {
                            this.Expect(TokenType.COMMA);
                            references.Add(this.Expression(skipEquals: true));
                        }
                        List<AstNode> toAssigned = new List<AstNode>();
                        if (this.HasToken && this.CurrentToken.Type == TokenType.EQUALS)
                        {
                            this.Expect(TokenType.EQUALS);
                            toAssigned.Add(this.Expression());
                            while (this.HasToken && this.CurrentToken.Type == TokenType.COMMA)
                            {
                                this.Expect(TokenType.COMMA);
                                toAssigned.Add(this.Expression());
                            }
                            if (toAssigned.Count == 1)
                                return new VariableAssignmentNode(references, toAssigned[0]);
                            if (references.Count != toAssigned.Count)
                                throw new ParseException($"Cannot assign {toAssigned.Count} values to {references.Count} variable{(references.Count > 1 ? "s" : "")}", LastToken);
                            return new VariableAssignmentNode(references, new ListNode(toAssigned));
                        }
                        else
                        {
                            this.currentTokenIndex = lastTokenIndex;
                            return references[0];
                        }
                    }
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
                case TokenType.OPEN_CURLY_BRACKET:
                    {
                        var expression = this.Expression();
                        expression.Line = token.Line;
                        return expression;
                    }
                case TokenType.KEYWORD:
                    {
                        AstNode result;

                        if (token.Raw.Equals("var") || token.Raw.Equals("let") || token.Raw.Equals("const"))
                            result = this.VariableDefinition(token.Raw.Equals("const"), false, AccessibilityLevel.Public);
                        else if (token.Raw.Equals("public") || token.Raw.Equals("protected") ||
                            token.Raw.Equals("private") || token.Raw.Equals("static"))
                            result = CheckAccessibilityLevel(token.Raw);
                        else if (token.Raw.Equals("if")) result = this.IfStatement();
                        else if (token.Raw.Equals("try")) result = this.TryCatchStatement();
                        else if (token.Raw.Equals("switch")) result = this.SwitchStatement();
                        else if (token.Raw.Equals("fn")) result = this.FnDefinition(false, AccessibilityLevel.Public, false, false);
                        else if (token.Raw.Equals("class")) result = this.ClassDefinition(false, AccessibilityLevel.Public);
                        else if (token.Raw.Equals("enum")) result = this.EnumDefinition(AccessibilityLevel.Public);
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
                        else if (token.Raw.Equals("this") || token.Raw.Equals("super") || token.Raw.Equals("catch"))
                        {
                            var expression = this.Expression();
                            expression.Line = token.Line;
                            return expression;
                        }
                        else
                        {
                            throw new ParseException("Keyword is not implemented: " + token.Raw, token);
                        }

                        while (HasToken && CurrentToken.Type == TokenType.SEMI_COLON)
                        {
                            Expect(TokenType.SEMI_COLON);
                        }
                        result.Line = token.Line;
                        return result;
                    }
                case TokenType.SEMI_COLON:
                        this.currentTokenIndex++;
                        return null;
                default:
                    throw new ParseException("Unexpected token: " + token.Type, token);
            }
        }
    }
}
