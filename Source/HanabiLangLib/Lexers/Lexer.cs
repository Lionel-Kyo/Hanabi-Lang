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
            "import", "from", "as", "throw", "try", "catch", "finally", "params",
            "switch", "case", "default", "async", "await", "class", "this", "super",
            "null", "true", "false", "private", "public", "protected", "internal",
            "static", "using", "namespace", /*"object",*/ "dynamic", "enum", "is", "not"
        };

        private static void BlockComment(ref int index, ref bool isBlockComment, string line)
        {
            int closeIndex = line.IndexOf("*/", index);
            int length = line.Length;
            if (closeIndex == -1)
            {
                index = line.Length;
            }
            else
            {
                index = closeIndex + 1;
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

        private static Token LiteralStringToken(char startChar, ref int index, 
            IEnumerable<string> lines, ref int lineIndex)
        {
            int startLineIndex = lineIndex;
            StringBuilder result = new StringBuilder();

            bool endOfString = false;

            while (true)
            {
                string line = lines.ElementAt(lineIndex);
                while (index < line.Length)
                {
                    if (line[index] == startChar)
                    {
                        if (index + 1 < line.Length && line[index + 1] == startChar)
                        {
                            index++;
                        }
                        else
                        {
                            endOfString = true;
                            break;
                        }
                    }
                    result.Append(line[index]);
                    index++;
                }
                if (endOfString)
                    break;
                lineIndex++;
                if (lineIndex < lines.Count())
                    result.Append(Environment.NewLine);
                else
                    break;
                index = 0;
            }

            if (!endOfString)
                throw new SystemException($"string defined in line: {lineIndex + 1} is not ended");

            return new Token(TokenType.STRING, result.ToString(), startLineIndex + 1);
        }

        private static InterpolatedStringToken InterpolatedLiteralStringToken(char startChar, ref int index,
            IEnumerable<string> lines, ref int lineIndex)
        {
            int startLineIndex = lineIndex;
            List<string> texts = new List<string>();
            StringBuilder text = new StringBuilder();
            // line number, interpolate start index
            Queue<Tuple<int, int>> openSqaureIndexQueue = new Queue<Tuple<int, int>>();
            List<List<Token>> interpolatedTokens = new List<List<Token>>();
            bool endOfString = false;

            while (true)
            {
                string line = lines.ElementAt(lineIndex);
                while (index < line.Length)
                {
                    char c = line[index];
                    if (c == startChar && openSqaureIndexQueue.Count == 0)
                    {
                        if (index + 1 < line.Length && line[index + 1] == startChar)
                        {
                            text.Append(c);
                            index += 2;
                        }
                        else
                        {
                            endOfString = true;
                            break;
                        }
                    }
                    else if (c == '{')
                    {
                        index++;
                        if (index >= line.Length)
                        {
                            if (lineIndex + 1 < lines.Count())
                            {
                                lineIndex++;
                                index = 0;
                                line = lines.ElementAt(lineIndex);
                            }
                            else
                            {
                                throw new SystemException($"Interpolated string cannot end with, line: {lineIndex + 1}");
                            }
                        }

                        if (line[index] == '{')
                        {
                            text.Append('{');
                            index++;
                        }
                        else
                        {
                            openSqaureIndexQueue.Enqueue(Tuple.Create(lineIndex, index));
                        }
                    }
                    else if (c == '}')
                    {
                        index++;
                        if (index >= line.Length)
                        {
                            if (lineIndex + 1 < lines.Count())
                            {
                                lineIndex++;
                                index = 0;
                                line = lines.ElementAt(lineIndex);
                            }
                            else
                            {
                                throw new SystemException($"Interpolated string cannot end with }}, line: {lineIndex + 1}");
                            }
                        }

                        if (line.Length > 0 && line[index] == '}')
                        {
                            text.Append('}');
                            index++;
                        }
                        else if (openSqaureIndexQueue.Count > 0)
                        {
                            var lastOpenSquare = openSqaureIndexQueue.Dequeue();
                            int openSquareLine = lastOpenSquare.Item1;
                            int openSquareIndex = lastOpenSquare.Item2;
                            StringBuilder interpolated = new StringBuilder();
                            while (openSquareLine < lineIndex)
                            {
                                interpolated.AppendLine(lines.ElementAt(openSquareLine).Substring(openSquareIndex));
                                openSquareLine++;
                                openSquareIndex = 0;
                            }
                            interpolated.AppendLine(line.Substring(openSquareIndex,
                                index - openSquareIndex <= 0 ? 0 : index - openSquareIndex - 1));
                            var token = Tokenize(new List<string>() { interpolated.ToString() });

                            texts.Add(text.ToString());
                            text.Clear();
                            texts.Add(null);
                            interpolatedTokens.Add(token);
                        }
                        else
                        {
                            throw new SystemException($"Cannot put }} without {{ in a interpolated string, line: {lineIndex + 1}");
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
                if (endOfString)
                    break;
                lineIndex++;
                if (lineIndex < lines.Count())
                    text.Append(Environment.NewLine);
                else
                    break;
                index = 0;
            }

            if (!endOfString)
                throw new SystemException($"string defined in line: {startLineIndex + 1} is not ended");

            if (text.Length > 0)
            {
                texts.Add(text.ToString());
                text.Clear();
            }

            return new InterpolatedStringToken(TokenType.INTERPOLATED_STRING, "", startLineIndex + 1, texts, interpolatedTokens);
        }

        private static InterpolatedStringToken InterpolatedEscapeStringToken(char startChar, ref int index,
            string line, int startLineIndex)
        {
            int lineIndex = startLineIndex;
            List<string> texts = new List<string>();
            StringBuilder text = new StringBuilder();
            Queue<int> openSqaureIndexQueue = new Queue<int>();
            List<List<Token>> interpolatedTokens = new List<List<Token>>();
            bool endOfString = false;

            while (index < line.Length)
            {
                char c = line[index];
                if (c == startChar && openSqaureIndexQueue.Count == 0)
                {
                    endOfString = true;
                    break;
                }
                if (c == '{')
                {
                    index++;
                    if (index >= line.Length)
                        throw new SystemException($"Interpolated string cannot end with {{, line: {lineIndex + 1}");

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
                        throw new SystemException($"Interpolated string cannot end with }}, line: {lineIndex + 1}");

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
                        throw new SystemException($"Cannot put }} without {{ in a interpolated string, line: {lineIndex + 1}");
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
                        throw new SystemException($"Cannot end with escape character, line: {lineIndex + 1}");

                    char nextChar = line[index];
                    if (nextChar == startChar)
                        throw new SystemException($"Cannot end with escape character, line: {lineIndex + 1}");

                    switch (nextChar)
                    {
                        case '\'':
                            text.Append('\'');
                            index++;
                            break;
                        case '\"':
                            text.Append('\"');
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
                                throw new SystemException($"Convert to unicode failed, line: {lineIndex + 1}");
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
                                throw new SystemException($"Convert to unicode failed, line: {lineIndex + 1}");
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
            
            if (!endOfString)
                throw new SystemException($"string defined in line: {lineIndex + 1} is not ended");

            if (text.Length > 0)
            {
                texts.Add(text.ToString());
                text.Clear();
            }
            return new InterpolatedStringToken(TokenType.INTERPOLATED_STRING, "", startLineIndex + 1, texts, interpolatedTokens);
        }

        private static Token EscapeStringToken(char startChar, ref int index,
            string line, int startLineIndex)
        {
            StringBuilder result = new StringBuilder();
            int lineIndex = startLineIndex;
            bool endOfString = false;

            while (index < line.Length)
            {
                char c = line[index];
                if (c == startChar)
                {
                    endOfString = true;
                    break;
                }
                else if (c == '\\')
                {
                    index++;
                    if (index >= line.Length)
                        throw new SystemException($"Cannot end with escape character, line: {lineIndex + 1}");

                    char nextChar = line[index];
                    //if (nextChar == startChar)
                    //    throw new SystemException($"Cannot end with escape character, line: {lineIndex + 1}");

                    switch (nextChar)
                    {
                        case '\'':
                            result.Append('\'');
                            index++;
                            break;
                        case '\"':
                            result.Append('\"');
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
                                throw new SystemException($"Convert to unicode failed, line: {lineIndex + 1}");
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
                                throw new SystemException($"Convert to unicode failed, line: {lineIndex + 1}");
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

            if (!endOfString)
                throw new SystemException($"string defined in line: {lineIndex + 1} is not ended");

            return new Token(TokenType.STRING, result.ToString(), startLineIndex + 1);
        }

        public static List<Token> Tokenize(IEnumerable<string> lines)
        {
            List<Token> tokens = new List<Token>();
            bool isBlockComment = false;

            for (int lineIndex = 0; lineIndex < lines.Count(); lineIndex++)
            {
                string line = lines.ElementAt(lineIndex);
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];

                    if (SkipChars.Contains(c))
                        continue;

                    else if (c == '#')
                        break;

                    else if (c == '\r' || c == '\n')
                        continue;

                    else if (isBlockComment)
                    {
                        BlockComment(ref i, ref isBlockComment, line);
                        if (i >= line.Length)
                            break;
                    }

                    else if (c == ';')
                    {
                        tokens.Add(new Token(TokenType.SEMI_COLON, ";", lineIndex + 1));
                    }

                    // check int / float
                    else if (char.IsDigit(c))
                    {
                        StringBuilder numberBuilder = new StringBuilder();
                        byte dotNums = 0;
                        int numberBase = 10;

                        if (c == '0' && i + 1 < line.Length)
                        {
                            char nextChar = line[i + 1];
                            if (nextChar == 'b' || nextChar == 'B')
                            {
                                numberBase = 2;
                            }
                            else if (nextChar == 'o' || nextChar == 'O')
                            {
                                numberBase = 8;
                            }
                            else if (nextChar == 'x' || nextChar == 'X')
                            {
                                numberBase = 16;
                            }
                        }

                        if (numberBase != 10)
                        {
                            i += 2;
                            if (i >= line.Length)
                                throw new SystemException($"Unexpected base{numberBase} number: line {lineIndex + 1}");
                        }

                        while (i < line.Length)
                        {
                            c = line[i];
                            if (IdentifierNotAllowedChars.Contains(c) && c != '.')
                                break;
                            if (c == '.')
                            {
                                dotNums++;
                                if (numberBase != 10)
                                    throw new SystemException($"Unexpected floating point (base{numberBase}): line {lineIndex + 1}");
                                if (dotNums > 1)
                                    throw new SystemException($"Unexpected floating point: line {lineIndex + 1}");
                            }
                            else
                            {
                                if (numberBase == 2)
                                {
                                    if (!(c >= '0' && c <= '1'))
                                        throw new SystemException($"Unexpected base{numberBase} number: line {lineIndex + 1}");
                                }
                                else if (numberBase == 8)
                                {
                                    if (!(c >= '0' && c <= '7'))
                                        throw new SystemException($"Unexpected base{numberBase} number: line {lineIndex + 1}");
                                }
                                else if (numberBase == 10)
                                {
                                    if (!(c >= '0' && c <= '9'))
                                        throw new SystemException($"Unexpected base{numberBase} number: line {lineIndex + 1}");
                                }
                                else if (numberBase == 16)
                                {
                                    if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                                        throw new SystemException($"Unexpected base{numberBase} number: line {lineIndex + 1}");
                                }
                            }

                            numberBuilder.Append(c);
                            i++;
                        }

                        i--;

                        string number = numberBuilder.ToString();
                        if (number.EndsWith("."))
                        {
                            throw new SystemException($"Unexpected number ends with \".\": line {lineIndex + 1}");
                        }

                        if (dotNums == 1)
                        {
                            tokens.Add(new Token(TokenType.FLOAT, number, lineIndex + 1));
                        }
                        else if (dotNums == 0)
                        {
                            if (numberBase == 10)
                                tokens.Add(new Token(TokenType.INT, number, lineIndex + 1));
                            else
                                tokens.Add(new Token(TokenType.INT, Convert.ToInt64(number, numberBase).ToString(), lineIndex + 1));
                        }
                    }

                    // check identifier
                    else if (CheckIdentifier(c, true))
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
                                tokens.Add(new Token(TokenType.NULL, identifier, lineIndex + 1));
                            else if (identifier == "true")
                                tokens.Add(new Token(TokenType.TRUE, identifier, lineIndex + 1));
                            else if (identifier == "false")
                                tokens.Add(new Token(TokenType.FALSE, identifier, lineIndex + 1));
                            else
                                tokens.Add(new Token(TokenType.KEYWORD, identifier, lineIndex + 1));
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.IDENTIFIER, identifier, lineIndex + 1));
                        }

                    }

                    // !=, !
                    else if (c == '!')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "!=", lineIndex + 1));
                            i++;
                        }
                        else tokens.Add(new Token(TokenType.OPERATOR, "!", lineIndex + 1));
                    }

                    // ==, >=, =
                    else if (c == '=')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "==", lineIndex + 1));
                            i++;
                        }
                        else if (i + 1 < line.Length && line[i + 1] == '>')
                        {
                            tokens.Add(new Token(TokenType.DOUBLE_ARROW, "=>", lineIndex + 1));
                            i++;
                        }
                        else tokens.Add(new Token(TokenType.EQUALS, "=", lineIndex + 1));
                        continue;
                    }

                    // <=, <
                    else if (c == '<')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "<=", lineIndex + 1));
                            i++;
                        }
                        else tokens.Add(new Token(TokenType.OPERATOR, "<", lineIndex + 1));
                    }

                    // >=, >
                    else if (c == '>')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, ">=", lineIndex + 1));
                            i++;
                        }
                        else tokens.Add(new Token(TokenType.OPERATOR, ">", lineIndex + 1));
                    }

                    // ||
                    else if (c == '|')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '|')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "||", lineIndex + 1));
                            i++;
                        }
                        else tokens.Add(new Token(TokenType.OPERATOR, "|", lineIndex + 1));
                    }

                    // &&
                    else if (c == '&')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '&')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "&&", lineIndex + 1));
                            i++;
                        }
                    }

                    // *, *=, %, %=
                    else if (c == '*' || c == '%')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, $"{c}=", lineIndex + 1));
                            i++;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), lineIndex + 1));
                        }
                    }

                    // +, +=, ++, -, -=, --, ->
                    else if (c == '+' || c == '-')
                    {
                        // ->
                        if (i + 1 < line.Length && c == '-' && line[i + 1] == '>')
                        {
                            tokens.Add(new Token(TokenType.SINGLE_ARROW, "->", lineIndex + 1));
                            i++;
                        }
                        // ++, --
                        else if (i + 1 < line.Length && line[i + 1] == c)
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, $"{c}{c}", lineIndex + 1));
                            i++;
                        }
                        // +=, -=
                        else if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, $"{c}=", lineIndex + 1));
                            i++;
                        }
                        // +, =
                        else
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), lineIndex + 1));
                        }
                    }

                    // open round bracket
                    else if (c == '(')
                    {
                        tokens.Add(new Token(TokenType.OPEN_ROUND_BRACKET, c.ToString(), lineIndex + 1));
                    }

                    // close round bracket
                    else if (c == ')')
                    {
                        tokens.Add(new Token(TokenType.CLOSE_ROUND_BRACKET, c.ToString(), lineIndex + 1));
                    }

                    // open squre bracket
                    else if (c == '[')
                    {
                        tokens.Add(new Token(TokenType.OPEN_SQURE_BRACKET, c.ToString(), lineIndex + 1));
                    }

                    // close squre bracket
                    else if (c == ']')
                    {
                        tokens.Add(new Token(TokenType.CLOSE_SQURE_BRACKET, c.ToString(), lineIndex + 1));
                    }

                    // open curly bracket
                    else if (c == '{')
                    {
                        tokens.Add(new Token(TokenType.OPEN_CURLY_BRACKET, c.ToString(), lineIndex + 1));
                    }

                    // close curly bracket
                    else if (c == '}')
                    {
                        tokens.Add(new Token(TokenType.CLOSE_CURLY_BRACKET, c.ToString(), lineIndex + 1));
                        continue;
                    }

                    // ?
                    else if (c == '?')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '?')
                        {
                            tokens.Add(new Token(TokenType.DOUBLE_QUESTION_MARK, "??", lineIndex + 1));
                            i++;
                        }
                        else if (i + 1 < line.Length && line[i + 1] == '.')
                        {
                            tokens.Add(new Token(TokenType.QUESTION_DOT, "?.", lineIndex + 1));
                            i++;
                        }
                        else if (i + 1 < line.Length && line[i + 1] == '(')
                        {
                            tokens.Add(new Token(TokenType.QUESTION_OPEN_ROUND_BRACKET, "?(", lineIndex + 1));
                            i++;
                        }
                        else if (i + 1 < line.Length && line[i + 1] == '[')
                        {
                            tokens.Add(new Token(TokenType.QUESTION_OPEN_SQURE_BRACKET, "?[", lineIndex + 1));
                            i++;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.QUESTION_MARK, c.ToString(), lineIndex + 1));
                        }
                    }

                    // .
                    else if (c == '.')
                    {
                        tokens.Add(new Token(TokenType.DOT, c.ToString(), lineIndex + 1));
                    }

                    // ,
                    else if (c == ',')
                    {
                        tokens.Add(new Token(TokenType.COMMA, c.ToString(), lineIndex + 1));
                    }

                    // :
                    else if (c == ':')
                    {
                        tokens.Add(new Token(TokenType.COLON, c.ToString(), lineIndex + 1));
                    }

                    // string: "", ''
                    else if (c == '"' || c == '\'')
                    {
                        i++;
                        tokens.Add(EscapeStringToken(c, ref i, line, lineIndex));
                    }

                    else if (c == '$')
                    {
                        if (i + 1 < line.Length && (line[i + 1] == '@'))
                        {
                            i += 2;
                            if (i < line.Length && (line[i] == '\'' || line[i] == '\"'))
                            {
                                i++;
                                int beforeLineIndex = lineIndex;
                                tokens.Add(InterpolatedLiteralStringToken(line[i - 1], ref i, lines, ref lineIndex));
                                if (lineIndex > beforeLineIndex)
                                    break;
                            }
                            else throw new SystemException($"Unexpected Token $@, line: {lineIndex + 1}");
                        }
                        else if (i + 1 < line.Length && (line[i + 1] == '\'' || line[i + 1] == '\"'))
                        {
                            i += 2;
                            tokens.Add(InterpolatedEscapeStringToken(line[i - 1], ref i, line, lineIndex));
                        }
                        else
                        {
                            throw new SystemException($"Unexpected Token $, line: {lineIndex + 1}");
                        }
                    }

                    else if (c == '@')
                    {
                        if (i + 1 < line.Length && (line[i + 1] == '$'))
                        {
                            i += 2;
                            if (i < line.Length && (line[i] == '\'' || line[i] == '\"'))
                            {
                                i++;
                                int beforeLineIndex = lineIndex;
                                tokens.Add(InterpolatedLiteralStringToken(line[i - 1], ref i, lines, ref lineIndex));
                                if (lineIndex > beforeLineIndex)
                                    break;
                            }
                            else
                            {
                                throw new SystemException($"Unexpected Token @$, line: {lineIndex + 1}");
                            }
                        }
                        else if (i + 1 < line.Length && (line[i + 1] == '\'' || line[i + 1] == '\"'))
                        {
                            i += 2;
                            int beforeLineIndex = lineIndex;
                            tokens.Add(LiteralStringToken(line[i - 1], ref i, lines, ref lineIndex));
                            if (lineIndex > beforeLineIndex)
                                break;
                        }
                        else
                        {
                            throw new SystemException($"Unexpected Token @, line: {lineIndex + 1}");
                        }
                    }

                    // /=, /*, /
                    else if (c == '/')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, "/=", lineIndex + 1));
                            i++;
                        }
                        else if (i + 1 < line.Length && line[i + 1] == '/')
                        {
                            break;
                        }
                        else if (i + 1 < line.Length && line[i + 1] == '*')
                        {
                            isBlockComment = true;
                            i += 2;
                            BlockComment(ref i, ref isBlockComment, line);
                            if (i >= line.Length)
                                break;
                            else
                                continue;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), lineIndex + 1));
                            continue;
                        }
                    }
                    else
                    {
                        throw new SystemException($"Unexpected Token {c}, line: {lineIndex + 1}");
                    }
                }
            }

            return tokens;

        }

        public static string[] ReadScriptToLines(string fullPath)
        {
            return System.IO.File.ReadAllLines(fullPath);
        }
    }
}
