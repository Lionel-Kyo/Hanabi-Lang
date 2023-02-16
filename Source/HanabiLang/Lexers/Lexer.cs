using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HanabiLang.Lexers
{
    public static class Lexer
    {
        /// <summary>
        /// Identifier not allowed charachers
        /// </summary>
        private static readonly char[] IdentifierNotAllowedChars = new char[]
        {   '`', '~', '!', '@', '#', '$', '%',
            '^', '&', '*', '(', ')', '-', '+',
            '=', '[', ']', '{', '}', '|', '\\',
            ';', ':', '\'', '\"', ',', '.', '<',
            '>', '/', '?', ' ', '\t', '\r', '\n', '\0'
        };

        /// <summary>
        /// Token skip characters
        /// </summary>
        private static readonly char[] SkipChars = new char[] { ' ', '\t', '\r', '\0' };

        /// <summary>
        /// Reserved keywords
        /// </summary>
        private static readonly string[] Keywords = new string[]
        {
            "if", "else", "for", "while", "define",
            "fn", "let" ,"var", "auto" ,"const",
            "in", "break", "continue", "return",
            "import", "from", "as", "throw", "try", "catch", "finally",
            "switch", "case", "default", "async", "await", "class", "this", "super",
            "null", "true", "false", "private", "public", "protected", "internal",
            "static", "using", "namespace", "object", "dynamic", "enum", "is", "not"
        };

        private static void BlockComment(ref int index, ref bool isBlockComment, string line)
        {
            int closeIndex = line.IndexOf("*/");
            if (closeIndex == -1)
            {
                index = line.Length;
            }
            else
            {
                index = closeIndex + 2;
                isBlockComment = false;
            }
        }

        private static bool CheckIdentifier(char c, bool firstChar = false)
        {
            if (firstChar && char.IsDigit(c))
                return false;
            if (IdentifierNotAllowedChars.Contains(c))
                return false;
            return true;
        }

        private static void AddLiteralStringToken(char startChar, ref int index, List<Token> tokens, string line, int lineIndex)
        {
            StringBuilder result = new StringBuilder();
            index++;

            while (index < line.Length && line[index] != startChar)
            {
                result.Append(line[index]);
                index++;
            }

            tokens.Add(new Token(TokenType.STRING, result.ToString(), lineIndex));
        }

        private static void AddInterpolatedLiteralStringToken(char startChar, ref int index, List<Token> tokens, string line, int lineIndex)
        {
            List<string> texts = new List<string>();
            StringBuilder text = new StringBuilder();
            index++;
            Queue<int> openSqaureIndexQueue = new Queue<int>();
            List<List<Token>> interpolatedTokens = new List<List<Token>>();
            while (index < line.Length)
            {
                char c = line[index];
                if (c == startChar)
                {
                    break;
                }
                else if (c == '{')
                {
                    index++;
                    if (index >= line.Length)
                        throw new SystemException("Interpolated string cannot end with {");
                    if (line[index] == '{')
                    {
                        text.Append('{');
                        index++;
                    }
                    else
                    {
                        openSqaureIndexQueue.Enqueue(index);
                    }
                }
                else if (c == '}')
                {
                    index++;
                    if (index >= line.Length)
                        throw new SystemException("Interpolated string cannot end with }");
                    if (openSqaureIndexQueue.Count > 0)
                    {
                        int lastOpenSquareIndex = openSqaureIndexQueue.Dequeue();
                        string interpolated = line.Substring(lastOpenSquareIndex, index - lastOpenSquareIndex - 1);
                        var token = Tokenize(new List<string>() { interpolated });

                        texts.Add(text.ToString());
                        text.Clear();
                        texts.Add(null);
                        interpolatedTokens.Add(token);
                    }
                    else if (line[index] == '}')
                    {
                        text.Append('}');
                        index++;
                    }
                    else
                    {
                        throw new SystemException("Cannot put } without { in a interpolated string");
                    }
                }
                else if (openSqaureIndexQueue.Count > 0)
                {
                    index++;
                }
                else
                {
                    text.Append(c);
                    index++;
                }
            }
            if (text.Length > 0)
            {
                texts.Add(text.ToString());
                text.Clear();
            }
            tokens.Add(new InterpolatedStringToken(TokenType.INTERPOLATED_STRING, "", lineIndex, texts, interpolatedTokens));
        }

        private static void AddEscapeInterpolatedStringToken(char startChar, ref int index, List<Token> tokens, string line, int lineIndex)
        {
            List<string> texts = new List<string>();
            StringBuilder text = new StringBuilder();
            index++;
            Queue<int> openSqaureIndexQueue = new Queue<int>();
            List<List<Token>> interpolatedTokens = new List<List<Token>>();

            while (index < line.Length)
            {
                char c = line[index];
                if (c == startChar)
                {
                    break;
                }
                if (c == '{')
                {
                    index++;
                    if (index >= line.Length)
                        throw new SystemException("Interpolated string cannot end with {");
                    if (line[index] == '{')
                    {
                        text.Append('{');
                        index++;
                    }
                    else
                    {
                        openSqaureIndexQueue.Enqueue(index);
                    }
                }
                else if (c == '}')
                {
                    index++;
                    if (index >= line.Length)
                        throw new SystemException("Interpolated string cannot end with }");
                    if (openSqaureIndexQueue.Count > 0)
                    {
                        int lastOpenSquareIndex = openSqaureIndexQueue.Dequeue();
                        string interpolated = line.Substring(lastOpenSquareIndex, index - lastOpenSquareIndex - 1);
                        var token = Tokenize(new List<string>() { interpolated });

                        texts.Add(text.ToString());
                        text.Clear();
                        texts.Add(null);
                        interpolatedTokens.Add(token);
                    }
                    else if (line[index] == '}')
                    {
                        text.Append('}');
                        index++;
                    }
                    else
                    {
                        throw new SystemException("Cannot put } without { in a interpolated string");
                    }
                }
                else if (openSqaureIndexQueue.Count > 0)
                {
                    index++;
                }
                else if (c == '\\')
                {
                    index++;
                    if (index >= line.Length)
                        throw new SystemException("Cannot end with escape character");
                    char nextChar = line[index];
                    if (nextChar == startChar)
                        throw new SystemException("Cannot end with escape character");

                    switch (nextChar)
                    {
                        case '\'':
                            text.Append('\'');
                            index++;
                            break;
                        case '\"':
                            text.Append('\'');
                            index++;
                            break;
                        case '\\':
                            text.Append('\\');
                            index++;
                            break;
                        case '0':
                            text.Append('\0');
                            index++;
                            break;
                        case 'a':
                            text.Append('\a');
                            index++;
                            break;
                        case 'b':
                            text.Append('\b');
                            index++;
                            break;
                        case 'f':
                            text.Append('\f');
                            index++;
                            break;
                        case 'n':
                            text.Append('\n');
                            index++;
                            break;
                        case 'r':
                            text.Append('\r');
                            index++;
                            break;
                        case 't':
                            text.Append('\t');
                            index++;
                            break;
                        case 'v':
                            text.Append('\v');
                            index++;
                            break;
                        case 'u':
                            index++;
                            if (index + 4 <= line.Length && ushort.TryParse(line.Substring(index, 4),
                                System.Globalization.NumberStyles.HexNumber, null, out ushort u16))
                            {
                                text.Append(Convert.ToChar(u16));
                                index += 4;
                            }
                            else
                            {
                                throw new SystemException("Convert to unicode failed");
                            }
                            break;
                        case 'U':
                            index++;
                            if (index + 8 <= line.Length && uint.TryParse(line.Substring(index, 8),
                                System.Globalization.NumberStyles.HexNumber, null, out uint u32))
                            {
                                text.Append((char)u32);
                                index += 8;
                            }
                            else
                            {
                                throw new SystemException("Convert to unicode failed");
                            }
                            break;
                    }
                }
                else
                {
                    text.Append(c);
                    index++;
                }
            }
            if (text.Length > 0)
            {
                texts.Add(text.ToString());
                text.Clear();
            }
            tokens.Add(new InterpolatedStringToken(TokenType.INTERPOLATED_STRING, "", lineIndex, texts, interpolatedTokens));
        }

        private static void AddEscapeStringToken(char startChar, ref int index, List<Token> tokens, string line, int lineIndex)
        {
            StringBuilder result = new StringBuilder();
            index++;
            while (index < line.Length)
            {
                char c = line[index];
                if (c == startChar)
                {
                    break;
                }
                else if (c == '\\')
                {
                    index++;
                    if (index >= line.Length)
                        throw new SystemException("Cannot end with escape character");
                    char nextChar = line[index];
                    //if (nextChar == startChar)
                    //    throw new SystemException("Cannot end with escape character");

                    switch (nextChar)
                    {
                        case '\'':
                            result.Append('\'');
                            index++;
                            break;
                        case '\"':
                            result.Append('\'');
                            index++;
                            break;
                        case '\\':
                            result.Append('\\');
                            index++;
                            break;
                        case '0':
                            result.Append('\0');
                            index++;
                            break;
                        case 'a':
                            result.Append('\a');
                            index++;
                            break;
                        case 'b':
                            result.Append('\b');
                            index++;
                            break;
                        case 'f':
                            result.Append('\f');
                            index++;
                            break;
                        case 'n':
                            result.Append('\n');
                            index++;
                            break;
                        case 'r':
                            result.Append('\r');
                            index++;
                            break;
                        case 't':
                            result.Append('\t');
                            index++;
                            break;
                        case 'v':
                            result.Append('\v');
                            index++;
                            break;
                        case 'u':
                            index++;
                            if (index + 4 <= line.Length && ushort.TryParse(line.Substring(index, 4),
                                System.Globalization.NumberStyles.HexNumber, null, out ushort u16))
                            {
                                result.Append(Convert.ToChar(u16));
                                index += 4;
                            }
                            else
                            {
                                throw new SystemException("Convert to unicode failed");
                            }
                            break;
                        case 'U':
                            index++;
                            if (index + 8 <= line.Length && uint.TryParse(line.Substring(index, 8),
                                System.Globalization.NumberStyles.HexNumber, null, out uint u32))
                            {
                                result.Append((char)u32);
                                index += 8;
                            }
                            else
                            {
                                throw new SystemException("Convert to unicode failed");
                            }
                            break;
                    }
                }
                else
                {
                    result.Append(c);
                    index++;
                }
            }
            tokens.Add(new Token(TokenType.STRING, result.ToString(), lineIndex));
        }

        public static List<Token> Tokenize(IEnumerable<string> lines)
        {
            List<Token> tokens = new List<Token>();
            int line_index = 0;
            bool isBlockComment = false;

            foreach (string line in lines)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];

                    if (SkipChars.Contains(c))
                        continue;

                    if (c == '#')
                        break;

                    if (isBlockComment)
                    {
                        BlockComment(ref i, ref isBlockComment, line);
                        if (i >= line.Length)
                            break;
                    }

                    if (c == ';')
                    {
                        tokens.Add(new Token(TokenType.SEMI_COLON, ";", line_index));
                    }

                    // check int / float
                    if (char.IsDigit(c))
                    {
                        string number = "";

                        int dotNums = 0;

                        while (char.IsDigit(c) || c == '.')
                        {
                            if (c == '.')
                                dotNums++;
                            if (dotNums > 1)
                            {
                                dotNums--;
                                break;
                            }
                            number += c;
                            i++;
                            if (i >= line.Length)
                                break;
                            c = line[i];
                        }

                        i--;

                        if (number.EndsWith("."))
                        {
                            i--;
                            dotNums--;
                            number = number.TrimEnd('.');
                        }

                        if (dotNums == 1)
                            tokens.Add(new Token(TokenType.FLOAT, number, line_index));
                        else if (dotNums == 0)
                            tokens.Add(new Token(TokenType.INT, number, line_index));

                        continue;
                    }

                    // check identifier
                    if (CheckIdentifier(c, true))
                    {
                        string identifier = "";

                        while (i < line.Length && CheckIdentifier(line[i]))
                        {
                            identifier += line[i];
                            i++;
                        }

                        i--;

                        if (Keywords.Contains(identifier))
                        {
                            if (identifier == "null")
                                tokens.Add(new Token(TokenType.NULL, identifier, line_index));
                            else if (identifier == "true")
                                tokens.Add(new Token(TokenType.TRUE, identifier, line_index));
                            else if (identifier == "false")
                                tokens.Add(new Token(TokenType.FALSE, identifier, line_index));
                            else
                                tokens.Add(new Token(TokenType.KEYWORD, identifier, line_index));
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.IDENTIFIER, identifier, line_index));
                        }

                        continue;
                    }

                    // !=, !
                    if (c == '!')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "!=", line_index));
                            i++;
                        }
                        else tokens.Add(new Token(TokenType.OPERATOR, "!", line_index));
                        continue;
                    }

                    // ==, >=, =
                    if (c == '=')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "==", line_index));
                            i++;
                        }
                        else if (i + 1 < line.Length && line[i + 1] == '>')
                        {
                            tokens.Add(new Token(TokenType.DOUBLE_ARROW, "=>", line_index));
                            i++;
                        }
                        else tokens.Add(new Token(TokenType.EQUALS, "=", line_index));
                        continue;
                    }

                    // <=, <
                    if (c == '<')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "<=", line_index));
                            i++;
                        }
                        else tokens.Add(new Token(TokenType.OPERATOR, "<", line_index));
                        continue;
                    }

                    // >=, >
                    if (c == '>')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, ">=", line_index));
                            i++;
                        }
                        else tokens.Add(new Token(TokenType.OPERATOR, ">", line_index));
                        continue;
                    }

                    // ||
                    if (c == '|')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '|')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "||", line_index));
                            i++;
                        }
                        continue;
                    }

                    // &&
                    if (c == '&')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '&')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "&&", line_index));
                            i++;
                        }
                        continue;
                    }

                    // *, *=, %, %=
                    if (c == '*' || c == '%')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, $"{c}=", line_index));
                            i++;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), line_index));
                        }
                        continue;
                    }

                    // +, +=, ++, -, -=, --, ->
                    if (c == '+' || c == '-')
                    {
                        // ->
                        if (i + 1 < line.Length && c == '-' && line[i + 1] == '>')
                        {
                            tokens.Add(new Token(TokenType.SINGLE_ARROW, "->", line_index));
                            i++;
                        }
                        // ++, --
                        else if (i + 1 < line.Length && line[i + 1] == c)
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, $"{c}{c}", line_index));
                            i++;
                        }
                        // +=, -=
                        else if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, $"{c}=", line_index));
                            i++;
                        }
                        // +, =
                        else
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), line_index));
                        }
                        continue;
                    }

                    // open round bracket
                    if (c == '(')
                    {
                        tokens.Add(new Token(TokenType.OPEN_ROUND_BRACKET, c.ToString(), line_index));
                        continue;
                    }

                    // close round bracket
                    if (c == ')')
                    {
                        tokens.Add(new Token(TokenType.CLOSE_ROUND_BRACKET, c.ToString(), line_index));
                        continue;
                    }

                    // open squre bracket
                    if (c == '[')
                    {
                        tokens.Add(new Token(TokenType.OPEN_SQURE_BRACKET, c.ToString(), line_index));
                        continue;
                    }

                    // close squre bracket
                    if (c == ']')
                    {
                        tokens.Add(new Token(TokenType.CLOSE_SQURE_BRACKET, c.ToString(), line_index));
                        continue;
                    }

                    // open curly bracket
                    if (c == '{')
                    {
                        tokens.Add(new Token(TokenType.OPEN_CURLY_BRACKET, c.ToString(), line_index));
                        continue;
                    }

                    // close curly bracket
                    if (c == '}')
                    {
                        tokens.Add(new Token(TokenType.CLOSE_CURLY_BRACKET, c.ToString(), line_index));
                        continue;
                    }

                    // ?
                    if (c == '?')
                    {
                        tokens.Add(new Token(TokenType.QUESTION_MARK, c.ToString(), line_index));
                        continue;
                    }

                    // .
                    if (c == '.')
                    {
                        tokens.Add(new Token(TokenType.DOT, c.ToString(), line_index));
                        continue;
                    }

                    // ,
                    if (c == ',')
                    {
                        tokens.Add(new Token(TokenType.COMMA, c.ToString(), line_index));
                        continue;
                    }

                    // :
                    if (c == ':')
                    {
                        tokens.Add(new Token(TokenType.COLON, c.ToString(), line_index));
                        continue;
                    }

                    // string: "", ''
                    if (c == '"' || c == '\'')
                    {
                        AddEscapeStringToken(c, ref i, tokens, line, line_index);
                        continue;
                    }

                    if (c == '$')
                    {
                        if (i + 1 < line.Length && (line[i + 1] == '@'))
                        {
                            i++;
                            if (i + 1 < line.Length && (line[i + 1] == '\'' || line[i + 1] == '\"'))
                            {
                                i++;
                                AddInterpolatedLiteralStringToken(line[i], ref i, tokens, line, line_index);
                            }
                            else throw new SystemException($"Unexpected Token $@");
                            continue;
                        }
                        else if (i + 1 < line.Length && (line[i + 1] == '\'' || line[i + 1] == '\"'))
                        {
                            i++;
                            AddEscapeInterpolatedStringToken(line[i], ref i, tokens, line, line_index);
                            continue;
                        }
                        else throw new SystemException($"Unexpected Token $");
                    }

                    if (c == '@')
                    {
                        if (i + 1 < line.Length && (line[i + 1] == '\'' || line[i + 1] == '\"'))
                        {
                            i++;
                            AddLiteralStringToken(line[i], ref i, tokens, line, line_index);
                            continue;
                        }
                    }

                    // /=, /*, /
                    if (c == '/')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "/=", line_index));
                            i++;
                        }
                        else if (i + 1 < line.Length && line[i + 1] == '/')
                        {
                            break;
                        }
                        else if (i + 1 < line.Length && line[i + 1] == '*')
                        {
                            isBlockComment = true;
                            BlockComment(ref i, ref isBlockComment, line);
                            if (i >= line.Length)
                                break;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), line_index));
                            continue;
                        }
                    }
                }

                line_index++;
            }

            return tokens;

        }
    }
}
