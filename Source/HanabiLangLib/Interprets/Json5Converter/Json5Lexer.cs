using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HanabiLangLib.Interprets.Json5Converter
{
    public class Json5Lexer
    {
        private readonly string _input;
        private int _pos;
        private int _line;
        public Json5Lexer(string input)
        {
            _input = input;
            _pos = 0;
            _line = 1;
        }

        public List<Json5Token> Tokenize()
        {
            var tokens = new List<Json5Token>();
            while (true)
            {
                SkipWhitespaceAndComments();
                if (_pos >= _input.Length)
                {
                    tokens.Add(new Json5Token(Json5TokenType.EOF, "", _pos, _line));
                    break;
                }

                char c = _input[_pos];
                switch (c)
                {
                    case '{': tokens.Add(new Json5Token(Json5TokenType.LeftBrace, c.ToString(), _pos, _line)); _pos++; break;
                    case '}': tokens.Add(new Json5Token(Json5TokenType.RightBrace, c.ToString(), _pos, _line)); _pos++; break;
                    case '[': tokens.Add(new Json5Token(Json5TokenType.LeftBracket, c.ToString(), _pos, _line)); _pos++; break;
                    case ']': tokens.Add(new Json5Token(Json5TokenType.RightBracket, c.ToString(), _pos, _line)); _pos++; break;
                    case ':': tokens.Add(new Json5Token(Json5TokenType.Colon, c.ToString(), _pos, _line)); _pos++; break;
                    case ',': tokens.Add(new Json5Token(Json5TokenType.Comma, c.ToString(), _pos, _line)); _pos++; break;
                    case '"': tokens.Add(new Json5Token(Json5TokenType.String, ReadString('\"'), _pos, _line)); break;
                    case '\'':
                        tokens.Add(new Json5Token(Json5TokenType.String, ReadString('\''), _pos, _line));
                        break;
                    default:
                        if (char.IsLetter(c) || c == '_' || c == '$')
                        {
                            string id = ReadIdentifier();
                            if (id == "true" || id == "false")
                                tokens.Add(new Json5Token(Json5TokenType.Boolean, id, _pos, _line));
                            else if (id == "null")
                                tokens.Add(new Json5Token(Json5TokenType.Null, id, _pos, _line));
                            else if (id == "NaN" || id == "Infinity")
                                tokens.Add(new Json5Token(Json5TokenType.Float64, id, _pos, _line));
                            else
                                tokens.Add(new Json5Token(Json5TokenType.Identifier, id, _pos, _line));
                            continue;
                        }
                        else if (c == '-' || c == '+' || c == '.' || char.IsDigit(c))
                        {
                            tokens.Add(ParseNumber());
                        }
                        else
                        {
                            throw new FormatException($"Invalid character '${c}' at line {_line}, position {_pos}");
                        }
                        break;
                }
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
                if (char.IsWhiteSpace(c))
                {
                    _pos++;
                    continue;
                }
                // single line comment
                if (_pos + 1 < _input.Length && _input[_pos] == '/' && _input[_pos + 1] == '/')
                {
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

        private string ReadString(char quote)
        {
            var result = new StringBuilder();
            int startLine = _line;
            int startPos = _pos;

            // skip quote
            _pos++;

            while (_pos < _input.Length)
            {
                char c = _input[_pos];

                if (c == '\\')
                {
                    _pos++;
                    if (_pos >= _input.Length)
                        throw new FormatException($"Unterminated string literal at line {_line}, pos {_pos}");

                    char esc = _input[_pos];
                    switch (esc)
                    {
                        case '\"': result.Append('\"'); break;
                        case '\'': result.Append('\''); break;
                        case '\\': result.Append('\\'); break;
                        case 'b': result.Append('\b'); break;
                        case 'f': result.Append('\f'); break;
                        case 'n': result.Append('\n'); break;
                        case 'r': result.Append('\r'); break;
                        case 't': result.Append('\t'); break;
                        case 'v': result.Append('\v'); break;
                        case '0': result.Append('\0'); break;
                        case 'u':
                            if (_pos + 4 >= _input.Length)
                                throw new FormatException($"Invalid unicode escape at line {_line}, pos {_pos}");
                            string hex = _input.Substring(_pos + 1, 4);
                            result.Append((char)Convert.ToInt32(hex, 16));
                            _pos += 4;
                            break;
                        // newline escaped
                        case '\n':
                            _line++;
                            result.Append(Environment.NewLine);
                            break;
                        // newline escaped
                        case '\r':
                            // Windows line endings
                            if (_pos + 1 < _input.Length && _input[_pos + 1] == '\n')
                                _pos++;
                            _line++;
                            result.Append(Environment.NewLine);
                            break;
                        default:
                            // literal escapes
                            result.Append(esc);
                            break;
                    }
                }
                else if (c == '\n' || c == '\r')
                {
                    // newline not escaped
                    throw new FormatException($"Unterminated string literal '\\{(c == '\r' ? 'r' : 'n')}' at line {_line}, pos {_pos}");
                }
                else if (c == quote)
                {
                    _pos++;
                    return result.ToString();
                }
                else
                {
                    result.Append(c);
                }

                _pos++;
            }

            throw new FormatException($"Unterminated string literal starting at line {startLine}, pos {startPos}");
        }


        private string ReadIdentifier()
        {
            int start = _pos;
            while (_pos < _input.Length)
            {
                char c = _input[_pos];
                if (char.IsLetterOrDigit(c) || c == '_' || c == '$')
                    _pos++;
                else
                    break;
            }
            return _input.Substring(start, _pos - start);
        }

        private Json5Token ParseNumber()
        {
            int start = _pos;
            int startLine = _line;

            // sign
            if (_pos < _input.Length && (_input[_pos] == '+' || _input[_pos] == '-'))
            {
                _pos++;
            }

            // Infinity / NaN 
            if (_pos + 8 < _input.Length && _input.Substring(_pos, 8) == "Infinity")
            {
                _pos += 8;
                return new Json5Token(Json5TokenType.Float64, _input.Substring(start, _pos - start), _pos, _line);
            }
            else if (_pos + 3 < _input.Length && _input.Substring(_pos, 3) == "NaN")
            {
                _pos += 3;
                return new Json5Token(Json5TokenType.Float64, _input.Substring(start, _pos - start), _pos, _line);
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
                        if (numberBaseChar == 'x' && IsHexDigit(c)) _pos++;
                        else if (numberBaseChar == 'b' && (c == '0' || c == '1')) _pos++;
                        else if (numberBaseChar == 'o' && (c >= '0' && c <= '7')) _pos++;
                        else break;
                    }

                    if (_pos - basedNumberStartIndex <= 0)
                        throw new FormatException($"Invalid {numberBaseChar}-based literal at line {startLine}, position {start}");

                    return new Json5Token(Json5TokenType.Int64, _input.Substring(start, _pos - start), _pos, _line);
                }
            }

            bool hasDigits = false;
            bool isFloat = false;

            // integer
            while (_pos < _input.Length && char.IsDigit(_input[_pos]))
            {
                _pos++;
                hasDigits = true;
            }

            // fractional
            if (_pos < _input.Length && _input[_pos] == '.')
            {
                isFloat = true;
                _pos++;
                while (_pos < _input.Length && char.IsDigit(_input[_pos]))
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
                while (_pos < _input.Length && char.IsDigit(_input[_pos]))
                    _pos++;
            }

            if (!hasDigits)
                throw new FormatException($"Invalid number starting at position at line {startLine}, position {start}");

            string literal = _input.Substring(start, _pos - start);

            if (isFloat)
            {
                return new Json5Token(Json5TokenType.Float64, literal, _pos, _line);
            }
            else
            {
                return new Json5Token(Json5TokenType.Int64, literal, _pos, _line);
            }
        }

        private static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'f') ||
                   (c >= 'A' && c <= 'F');
        }
    }
}
