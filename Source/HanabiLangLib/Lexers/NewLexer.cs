using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace HanabiLangLib.Lexers
{
    public class NewLexer
    {
        private readonly string _input;
        private int _pos;
        private int _line;

        public NewLexer(string input)
        {
            _input = input;
            _pos = 0;
            _line = 1;
        }

        ///// <summary>
        ///// Identifier not allowed charachers
        ///// </summary>
        //private static readonly HashSet<char> IdentifierNotAllowedChars = new HashSet<char>()
        //{   '`', '~', '!', '@', '#', '$', '%',
        //    '^', '&', '*', '(', ')', '-', '+',
        //    '=', '[', ']', '{', '}', '|', '\\',
        //    ';', ':', '\'', '\"', ',', '.', '<',
        //    '>', '/', '?'
        //};

        /// <summary>
        /// Token skip characters
        /// </summary>
        private static readonly HashSet<char> SkipChars = new HashSet<char>()
        {
            ' ', // Normal Space
            '\t', // Horizontal Tab
            '\r', // Carriage Return
            '\0', // Zero
            '\u000B', // Vertical Tab
            '\u000C', // Form Feed
            '\u0085', // Next Line
            '\u00A0', // No-Break Space
            '\u1680', // Ogham Space Mark
            '\u2000', // En Quad
            '\u2001', // Em Quad
            '\u2002', // En Space
            '\u2003', // Em Space
            '\u2004', // Three-Per-Em Space
            '\u2005', // Four-Per-Em Space
            '\u2006', // Six-Per-Em Space
            '\u2007', // Figure Space
            '\u2008', // Punctuation Space
            '\u2009', // Thin Space
            '\u200A', // Hair Space
            '\u2028', // Line Separator
            '\u2029', // Paragraph Separator
            '\u202F', // Narrow No-Break Space
            '\u205F', // Medium Mathematical Space
            '\u3000', // Ideographic Space
            '\uFEFF', // Zero Width No-Break Space (BOM)
        };

        /// <summary>
        /// Reserved keywords
        /// </summary>
        private static readonly HashSet<string> Keywords = new HashSet<string>()
        {
            "if", "else", "for", "while", "define",
            "fn", "let" ,"var", "auto" ,"const",
            "in", "break", "continue", "return",
            "import", "from", "as", "throw", "try", "catch", "finally", "params",
            "switch", "case", "default", "async", "await", "class", "this", "super",
            "null", "true", "false", "private", "public", "protected", "internal",
            "static", "using", "namespace", /*"object",*/ "dynamic", "enum", "is", "not"
        };

        //private static bool CheckIdentifier(char c, bool firstChar = false)
        //{
        //    if (c == '_')
        //        return true;
        //    if (firstChar && char.IsDigit(c))
        //        return false;
        //    if (char.IsPunctuation(c))
        //        return false;
        //    if (SkipChars.Contains(c))
        //        return false;
        //    return true;
        //}

        /// <param name="c">utf8 / utf 16</param>
        /// <param name="utf32Char">null: not a utf32 char, length: 1</param>
        /// <returns></returns>
        private static bool CheckIdentifier(char c, string utf32Char = null, bool firstChar = false)
        {
            UnicodeCategory category = utf32Char == null ? char.GetUnicodeCategory(c) : char.GetUnicodeCategory(utf32Char, 0);

            if (c == '_')
            {
                return true;
            }

            if (firstChar)
            {
                switch (category)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.OtherSymbol:
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                switch (category)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                        case UnicodeCategory.OtherSymbol:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.Format:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public List<Token> Tokenize()
        {
            return Tokenize(terminateToken: null);
        }

        private List<Token> Tokenize(TokenType? terminateToken)
        {
            var tokens = new List<Token>();
            while (true)
            {
                SkipWhitespaceAndComments();
                if (_pos >= _input.Length)
                {
                    // tokens.Add(new Token(TokenType.EOF, "", _pos, _line));
                    break;
                }

                char c = _input[_pos];
                int startPos = _pos;
                int startLine = _line;
                switch (c)
                {
                    case '~': tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), startPos, startLine)); _pos++; break;
                    case '^': tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), startPos, startLine)); _pos++; break;
                    case '(': tokens.Add(new Token(TokenType.OPEN_ROUND_BRACKET, c.ToString(), startPos, startLine)); _pos++; break;
                    case ')': tokens.Add(new Token(TokenType.CLOSE_ROUND_BRACKET, c.ToString(), startPos, startLine)); _pos++; break;
                    case '[': tokens.Add(new Token(TokenType.OPEN_SQURE_BRACKET, c.ToString(), startPos, startLine)); _pos++; break;
                    case ']': tokens.Add(new Token(TokenType.CLOSE_SQURE_BRACKET, c.ToString(), startPos, startLine)); _pos++; break;
                    case '{': tokens.Add(new Token(TokenType.OPEN_CURLY_BRACKET, c.ToString(), startPos, startLine)); _pos++; break;
                    case '}': tokens.Add(new Token(TokenType.CLOSE_CURLY_BRACKET, c.ToString(), startPos, startLine)); _pos++; break;
                    case ';': tokens.Add(new Token(TokenType.SEMI_COLON, c.ToString(), startPos, startLine)); _pos++; break;
                    case ':': tokens.Add(new Token(TokenType.COLON, c.ToString(), startPos, startLine)); _pos++; break;
                    case ',': tokens.Add(new Token(TokenType.COMMA, c.ToString(), startPos, startLine)); _pos++; break;
                    case '.':
                        // Number
                        if (_pos + 1 < _input.Length && char.IsDigit(_input[_pos + 1]))
                        {
                            tokens.Add(ReadNumberToken());
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.DOT, c.ToString(), startPos, startLine)); 
                            _pos++;
                        }
                        break;
                    //+, +=, ++, -, -=, --, ->
                    case '=':
                    case '+':
                    case '-':
                    case '&':
                    case '|':
                    case '<':
                    case '>':
                        // ==, ++, --, &&, ||, <<, >>
                        if (_pos + 1 < _input.Length && _input[_pos + 1] == c)
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, _input.Substring(_pos, 2), startPos, startLine));
                            _pos += 2;
                        }
                        // +=, -=, &=, |=, <=, >=
                        else if (_pos + 1 < _input.Length && _input[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, _input.Substring(_pos, 2), startPos, startLine));
                            _pos += 2;
                        }
                        // =>, ->
                        else if ((c == '=' || c == '-') && _pos + 1 < _input.Length && _input[_pos + 1] == '>')
                        {
                            switch (c)
                            {
                                case '=':
                                    tokens.Add(new Token(TokenType.DOUBLE_ARROW, _input.Substring(_pos, 2), startPos, startLine));
                                    break;
                                case '-':
                                    tokens.Add(new Token(TokenType.SINGLE_ARROW, _input.Substring(_pos, 2), startPos, startLine));
                                    break;
                            }
                            _pos += 2;
                        }
                        // Number
                        else if ((c == '+' || c == '-') && (
                            (_pos + 1 < _input.Length && char.IsDigit(_input[_pos + 1])) || 
                            (_pos + 2 < _input.Length && _input[_pos + 1] == '.' && char.IsDigit(_input[_pos + 2]))))
                        {
                            tokens.Add(ReadNumberToken());
                        }
                        // EQUALS
                        else if (c == '=')
                        {
                            tokens.Add(new Token(TokenType.EQUALS, c.ToString(), startPos, startLine));
                            _pos++;
                        }
                        // +, -, &, |, <, >
                        else
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), startPos, startLine));
                            _pos++;
                        }
                        break;
                    case '!':
                    case '*':
                    case '/':
                    case '%':
                        // !=, *=, /=, %=
                        if (_pos + 1 < _input.Length && _input[_pos + 1] == '=')
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, _input.Substring(_pos, 2), startPos, startLine));
                            _pos += 2;
                        }
                        // !, *, /, %
                        else
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), startPos, startLine));
                            _pos++;
                        }
                        break;
                    case '?':
                        // ??
                        if (_pos + 1 < _input.Length && _input[_pos + 1] == c)
                        {
                            tokens.Add(new Token(TokenType.OPERATOR, _input.Substring(_pos, 2), startPos, startLine));
                            _pos += 2;
                        }
                        // ?.
                        else if (_pos + 1 < _input.Length && _input[_pos + 1] == '.')
                        {
                            tokens.Add(new Token(TokenType.QUESTION_DOT, _input.Substring(_pos, 2), startPos, startLine));
                            _pos += 2;
                        }
                        // ?(
                        else if (_pos + 1 < _input.Length && _input[_pos + 1] == '(')
                        {
                            tokens.Add(new Token(TokenType.QUESTION_OPEN_ROUND_BRACKET, _input.Substring(_pos, 2), startPos, startLine));
                            _pos += 2;
                        }
                        // ?[
                        else if (_pos + 1 < _input.Length && _input[_pos + 1] == '[')
                        {
                            tokens.Add(new Token(TokenType.QUESTION_OPEN_SQURE_BRACKET, _input.Substring(_pos, 2), startPos, startLine));
                            _pos += 2;
                        }
                        // ?
                        else
                        {
                            tokens.Add(new Token(TokenType.QUESTION_MARK, c.ToString(), startPos, startLine));
                            _pos++;
                        }
                        break;
                    case '\"':
                    case '\'':
                        if (_pos + 2 < _input.Length && _input[_pos + 1] == c && _input[_pos + 2] == c)
                        {
                            _pos += 3;
                            tokens.Add(new Token(TokenType.STRING, ReadString(new string(c, 3), readInterpolatedChunk: false, isLiteral: true, isMultiline: true), startPos, startLine));
                        }
                        else
                        {
                            _pos++;
                            tokens.Add(new Token(TokenType.STRING, ReadString(c.ToString(), readInterpolatedChunk: false, isLiteral: false, isMultiline: false), startPos, startLine));
                        }
                        break;
                    case '@':
                        if (_pos + 1 < _input.Length && (_input[_pos + 1] == '\'' || _input[_pos + 1] == '\"'))
                        {
                            string delimiter = _input[_pos + 1].ToString();
                            _pos += 2;
                            tokens.Add(new Token(TokenType.STRING, ReadString(delimiter, readInterpolatedChunk: false, isLiteral: true, isMultiline: true), startPos, startLine));
                        }
                        else if (_pos + 2 < _input.Length && _input[_pos + 1] == '$' && (_input[_pos + 2] == '\'' || _input[_pos + 2] == '\"'))
                        {
                            string delimiter = _input[_pos + 2].ToString();
                            _pos += 3;
                            var chunks = ReadInterpolatedString(delimiter, isLiteral: true, isMultiline: true);
                            tokens.Add(new InterpolatedStringToken(TokenType.INTERPOLATED_STRING, "", startPos, startLine, chunks.Item1, chunks.Item2));
                        }
                        else
                        {
                            goto default;
                        }
                        break;
                    case '$':
                        if (_pos + 3 < _input.Length && (_input[_pos + 1] == '\'' || _input[_pos + 1] == '\"') && (_input[_pos + 2] == _input[_pos + 1]) && (_input[_pos + 3] == _input[_pos + 1]))
                        {
                            string delimiter = new string(_input[_pos + 1], 3);
                            _pos += 4;
                            var chunks = ReadInterpolatedString(delimiter, isLiteral: true, isMultiline: true);
                            tokens.Add(new InterpolatedStringToken(TokenType.INTERPOLATED_STRING, "", startPos, startLine, chunks.Item1, chunks.Item2));
                        }
                        else if (_pos + 1 < _input.Length && (_input[_pos + 1] == '\'' || _input[_pos + 1] == '\"'))
                        {
                            string delimiter = _input[_pos + 1].ToString();
                            _pos += 2;
                            var chunks = ReadInterpolatedString(delimiter, isLiteral: false, isMultiline: false);
                            tokens.Add(new InterpolatedStringToken(TokenType.INTERPOLATED_STRING, "", startPos, startLine, chunks.Item1, chunks.Item2));
                        }
                        else if (_pos + 2 < _input.Length && _input[_pos + 1] == '@' && (_input[_pos + 2] == '\'' || _input[_pos + 2] == '\"'))
                        {
                            string delimiter = _input[_pos + 2].ToString();
                            _pos += 3;
                            var chunks = ReadInterpolatedString(delimiter, isLiteral: true, isMultiline: true);
                            tokens.Add(new InterpolatedStringToken(TokenType.INTERPOLATED_STRING, "", startPos, startLine, chunks.Item1, chunks.Item2));
                        }
                        else
                        {
                            goto default;
                        }
                        break;
                    default:
                        string utf32Char = _pos + 1 < _input.Length && char.IsSurrogatePair(c, _input[_pos + 1]) ? _input.Substring(_pos, 2) : null;
                        if (CheckIdentifier(c, utf32Char, firstChar: true)) 
                        {
                            _pos += utf32Char == null ? 1 : 2;
                            string id = ReadIdentifier(startPos);
                            if (id == "true")
                                tokens.Add(new Token(TokenType.TRUE, id, startPos, startLine));
                            else if (id == "false")
                                tokens.Add(new Token(TokenType.FALSE, id, startPos, startLine));
                            else if (id == "null")
                                tokens.Add(new Token(TokenType.NULL, id, startPos, startLine));
                            else if (id == "is")
                                tokens.Add(new Token(TokenType.OPERATOR, id, startPos, startLine));
                            else if (id == "not")
                                if (tokens.Count > 0 && tokens[tokens.Count - 1].Type == TokenType.OPERATOR && tokens[tokens.Count - 1].Raw == "is")
                                    tokens[tokens.Count - 1] = new Token(TokenType.OPERATOR, $"{tokens[tokens.Count - 1].Raw} {id}", tokens[tokens.Count - 1].Pos, tokens[tokens.Count - 1].Line);
                                else
                                    throw new FormatException($"Invalid keyword '{id}' at line {_line}, position {_pos}");
                            else if (Keywords.Contains(id))
                                tokens.Add(new Token(TokenType.KEYWORD, id, startPos, startLine));
                            else
                                tokens.Add(new Token(TokenType.IDENTIFIER, id, startPos, startLine));
                        }
                        else if (char.IsDigit(c))
                        {
                            tokens.Add(ReadNumberToken());
                        }
                        else
                        {
                            throw new FormatException($"Invalid character '{utf32Char ?? c.ToString()}' at line {_line}, position {_pos}");
                        }
                        break;
                }

                if (terminateToken.HasValue && tokens[tokens.Count - 1].Type == terminateToken)
                    break;
            }
            return tokens;
        }

        private void SkipWhitespaceAndComments()
        {
            while (_pos < _input.Length)
            {
                char c = _input[_pos];
                if (c == '\n')
                {
                    _line++;
                    _pos++;
                    continue;
                }
                if (c == '\r')
                {
                    // Windows line endings
                    if (_pos + 1 < _input.Length && _input[_pos + 1] == '\n')
                        _pos++;
                    _line++;
                    _pos++;
                    continue;
                }
                if (SkipChars.Contains(c))
                {
                    _pos++;
                    continue;
                }
                // single line comment
                if (_input[_pos] == '#' || (_pos + 1 < _input.Length && _input[_pos] == '/' && _input[_pos + 1] == '/'))
                {
                    if (_input[_pos] == '#')
                        _pos ++;
                    else
                        _pos += 2;
                    while (_pos < _input.Length)
                    {
                        if (_input[_pos] == '\n')
                        {
                            _line++;
                            _pos++;
                            break;
                        }
                        if (c == '\r')
                        {
                            // Windows line endings
                            if (_pos + 1 < _input.Length && _input[_pos + 1] == '\n')
                                _pos++;
                            _line++;
                            _pos++;
                            break;
                        }
                        _pos++;
                    }
                    continue;
                }
                // multi line comment
                if (_pos + 1 < _input.Length && _input[_pos] == '/' && _input[_pos + 1] == '*')
                {
                    _pos += 2;
                    while (_pos + 1 < _input.Length)
                    {
                        if (_input[_pos] == '\n')
                        {
                            _line++;
                        }
                        if (c == '\r')
                        {
                            // Windows line endings
                            if (_pos + 1 < _input.Length && _input[_pos + 1] == '\n')
                                _pos++;
                            _line++;
                        }
                        if (_input[_pos] == '*' && _input[_pos + 1] == '/')
                        {
                            _pos += 2;
                            break;
                        }
                        _pos++;
                    }
                    continue;
                }
                break;
            }
        }

        private string ReadString(string delimiter, bool readInterpolatedChunk, bool isLiteral, bool isMultiline)
        {
            int delimiterLength = delimiter.Length;
            var result = new StringBuilder();

            bool isVerbatimLiteral = isLiteral && delimiter.Length == 1;

            // skip the first immediate newline
            if (!isVerbatimLiteral && isMultiline)
            {
                if (_pos < _input.Length && _input[_pos] == '\n')
                {
                    _pos++;
                }
                else if (_pos < _input.Length && _input[_pos] == '\r')
                {
                    if (_pos + 1 < _input.Length && _input[_pos + 1] == '\n')
                        _pos++;
                    _pos++;
                }
            }

            while (_pos < _input.Length)
            {
                char c = _input[_pos];

                if (readInterpolatedChunk)
                {
                    if (c == '{')
                    {
                        if (_pos + 1 < _input.Length && _input[_pos + 1] == '{')
                        {
                            result.Append('{');
                            _pos += 2;
                            continue;
                        }
                        return result.ToString();
                    }
                    else if (c == '}')
                    {
                        if (_pos + 1 < _input.Length && _input[_pos + 1] == '}')
                        {
                            result.Append('}');
                            _pos += 2;
                            continue;
                        }
                        throw new FormatException($"Invalid Interpolated String, '}}' found without '{{' at line {_line}, position {_pos - 1}");
                    }
                }

                // verbatim literal
                if (delimiter.Length == 1 && c == delimiter[0] && _pos + 1 < _input.Length && _input[_pos + 1] == delimiter[0])
                {
                    result.Append(delimiter[0]);
                    _pos += 2;
                    continue;
                }

                if (_pos + delimiterLength <= _input.Length && _input.Substring(_pos, delimiterLength) == delimiter)
                {
                    _pos += delimiterLength;
                    return result.ToString();
                }

                if (!isLiteral && c == '\\' && _pos + 1 < _input.Length)
                {
                    _pos++;
                    char next = _input[_pos++];

                    switch (next)
                    {
                        case '0': result.Append('\0'); break;
                        case 'a': result.Append('\a'); break;
                        case 'b': result.Append('\b'); break;
                        case 't': result.Append('\t'); break;
                        case 'r': result.Append('\r'); break;
                        case 'n': result.Append('\n'); break;
                        case 'f': result.Append('\f'); break;
                        case 'v': result.Append('\v'); break;
                        case '\"': result.Append('\"'); break;
                        case '\'': result.Append('\''); break;
                        case '\\': result.Append('\\'); break;
                        case 'u':
                            result.Append(ParseUnicodeEscape(4));
                            break;
                        case 'U':
                            result.Append(ParseUnicodeEscape(8));
                            break;
                        default:
                            // Any Unicode character may be used except those that must be escaped:
                            // backslash and the control characters other than tab, line feed,
                            // and carriage return (U+0000 to U+0008, U+000B, U+000C, U+000E to U+001F, U+007F).
                            if (!isVerbatimLiteral && isMultiline)
                            {
                                while (_pos < _input.Length)
                                {
                                    char whiteSpace = _input[_pos];
                                    if (whiteSpace == '\n' || whiteSpace == '\r')
                                    {
                                        _line++;
                                        if (whiteSpace == '\r' && _pos + 1 < _input.Length && _input[_pos + 1] == '\n')
                                            _pos++;
                                        _pos++;
                                    }
                                    else if (char.IsWhiteSpace(whiteSpace) ||
                                        (whiteSpace >= 0x00 && whiteSpace <= 0x08) ||
                                        whiteSpace == 0x0B ||
                                        whiteSpace == 0x0C ||
                                        (whiteSpace >= 0x0E && whiteSpace <= 0x1F) ||
                                        whiteSpace == 0x7F
                                    )
                                    {
                                        _pos++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                throw new FormatException($"Invalid escape sequence in string: \\{next} at line {_line}, position {_pos}");
                            }
                            break;
                    }
                }
                else
                {
                    if (c == '\n' || c == '\r')
                    {
                        if (!isMultiline)
                            throw new FormatException($"Newline in non-multiline string at line {_line}, position {_pos}");

                        _line++;
                        if (c == '\r' && _pos + 1 < _input.Length && _input[_pos + 1] == '\n')
                            _pos++;
                        _pos++;
                        result.Append(Environment.NewLine);
                    }
                    else
                    {
                        result.Append(c);
                        _pos++;
                    }
                }
            }

            // Check for unclosed string
            throw new FormatException($"Unterminated string at line {_line}, position {_pos}");
        }

        /// <returns>(texts, interpolatedTokens)</returns>
        private Tuple<List<string>, List<List<Token>>> ReadInterpolatedString(string delimiter, bool isLiteral, bool isMultiline)
        {
            List<string> texts = new List<string>();
            List<List<Token>> interpolatedTokens = new List<List<Token>>();
            while (true)
            {
                var text = ReadString(delimiter, true, isLiteral, isMultiline);
                texts.Add(text);
                if (_pos < _input.Length &&_input[_pos] == '{')
                {
                    _pos++;
                    var tokens = this.Tokenize(TokenType.CLOSE_CURLY_BRACKET);
                    if (tokens.Count > 0 && tokens[tokens.Count - 1].Type != TokenType.CLOSE_CURLY_BRACKET)
                        throw new FormatException($"Invalid Interpolated String, }} is not found at line {_line}, position {_pos - 1}");
                    tokens.RemoveAt(tokens.Count - 1);
                    if (tokens.Count <= 0)
                        throw new FormatException($"Invalid Interpolated String, expression required inside {{}} at line {_line}, position {_pos - 1}");
                    interpolatedTokens.Add(tokens);
                    texts.Add(null);
                }
                else
                {
                    break;
                }
            }
            return Tuple.Create(texts, interpolatedTokens);
        }

        private string ParseUnicodeEscape(int length)
        {
            int startPos = _pos;
            int startLine = _line;
            if (_pos + length > _input.Length)
            {
                throw new FormatException($"Unterminated Unicode escape sequence (expected {length} hex digits) at line {startLine}, position {startPos}.");
            }

            string hex = _input.Substring(_pos, length);
            _pos += length;

            try
            {
                int codePoint = Convert.ToInt32(hex, 16);

                return char.ConvertFromUtf32(codePoint);
            }
            catch (Exception)
            {
                throw new FormatException($"Invalid Unicode scalar value '{hex}' in escape sequence at line {startLine}, position {startPos}.");
            }
        }

        private static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'f') ||
                   (c >= 'A' && c <= 'F');
        }

        private Token ReadNumberToken()
        {
            int startPos = _pos;
            int startLine = _line;

            // sign
            if (_pos < _input.Length && (_input[_pos] == '+' || _input[_pos] == '-'))
            {
                _pos++;
            }

            // base
            if (_pos + 1 < _input.Length && _input[_pos] == '0')
            {
                char numberBaseChar = char.ToLowerInvariant(_input[_pos + 1]);
                if (numberBaseChar == 'x' || numberBaseChar == 'b' || numberBaseChar == 'o')
                {
                    _pos += 2;
                    int basedNumberStartIndex = _pos;

                    while (_pos < _input.Length)
                    {
                        char c = _input[_pos];
                        if (numberBaseChar == 'x' && (IsHexDigit(c) || c == '_')) _pos++;
                        else if (numberBaseChar == 'b' && (c == '0' || c == '1' || c == '_')) _pos++;
                        else if (numberBaseChar == 'o' && ((c >= '0' && c <= '7') || c == '_')) _pos++;
                        else break;
                    }

                    if (_pos - basedNumberStartIndex <= 0)
                        throw new FormatException($"Invalid {numberBaseChar}-based literal at line {startLine}, position {startPos}");

                    string basedLiteral = _input.Substring(startPos, _pos - startPos);
                    if (basedLiteral.EndsWith("_"))
                        throw new FormatException($"Invalid {numberBaseChar}-based literal at line {startLine}, position {startPos}");

                    return new Token(TokenType.INT, basedLiteral, _pos, _line);
                }
            }

            bool hasDigits = false;
            bool isFloat = false;

            // integer
            while (_pos < _input.Length && (char.IsDigit(_input[_pos]) || _input[_pos] == '_'))
            {
                _pos++;
                hasDigits = true;
            }

            // fractional
            if (_pos < _input.Length && _input[_pos] == '.')
            {
                isFloat = true;
                _pos++;
                while (_pos < _input.Length && (char.IsDigit(_input[_pos]) || _input[_pos] == '_'))
                {
                    _pos++;
                    hasDigits = true;
                }
            }

            // exponent
            if (_pos < _input.Length && (_input[_pos] == 'e' || _input[_pos] == 'E'))
            {
                isFloat = true;
                _pos++;
                if (_pos < _input.Length && (_input[_pos] == '+' || _input[_pos] == '-'))
                    _pos++;
                while (_pos < _input.Length && (char.IsDigit(_input[_pos]) || _input[_pos] == '_'))
                    _pos++;
            }

            if (!hasDigits)
                throw new FormatException($"Invalid number starting at position at line {startLine}, position {startPos}");

            string literal = _input.Substring(startPos, _pos - startPos);
            if (literal.EndsWith("_"))
                throw new FormatException($"Invalid number starting at position at line {startLine}, position {startPos}");

            if (isFloat)
            {
                return new Token(TokenType.FLOAT, literal, _pos, _line);
            }
            else
            {
                return new Token(TokenType.INT, literal, _pos, _line);
            }
        }

        private string ReadIdentifier(int startPos)
        {
            int start = startPos;
            while (_pos < _input.Length)
            {
                char c = _input[_pos];
                string utf32Char = _pos + 1 < _input.Length && char.IsSurrogatePair(c, _input[_pos + 1]) ? _input.Substring(_pos, 2) : null;
                if (CheckIdentifier(c, utf32Char, firstChar: false))
                    _pos += utf32Char == null ? 1 : 2;
                else
                    break;
            }
            return _input.Substring(start, _pos - start);
        }
    }
}
