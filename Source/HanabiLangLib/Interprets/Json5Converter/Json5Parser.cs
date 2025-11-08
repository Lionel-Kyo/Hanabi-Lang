using HanabiLang;
using HanabiLang.Interprets;
using HanabiLang.Interprets.ScriptTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HanabiLangLib.Interprets.Json5Converter
{
    public class Json5Parser
    {
        private readonly List<Json5Token> _tokens;
        private readonly ScriptClass _targetType;
        private int _pos;

        public Json5Parser(List<Json5Token> tokens, ScriptClass targetType)
        {
            _tokens = tokens;
            _pos = 0;
            _targetType = targetType;
        }

        private Json5Token Current => _pos < _tokens.Count ? _tokens[_pos] : _tokens[_tokens.Count - 1];

        private Json5Token Consume(Json5TokenType expectedType)
        {
            var token = Current;
            if (token.Type == expectedType)
                return _tokens[_pos++];
            throw new FormatException($"Expected {expectedType} but got {Current.Type} at line {token.Line}, position {token.Pos}");
        }

        public ScriptValue Parse()
        {
            var value = ParseValue();
            var lastToken = Current;
            if (lastToken.Type != Json5TokenType.EOF)
                throw new FormatException($"Unexpected data after JSON value at line {lastToken.Line}, position {lastToken.Pos}");
            return value;
        }

        private ScriptValue ParseValue()
        {
            switch (Current.Type)
            {
                case Json5TokenType.LeftBrace: return ParseObject(_targetType);
                case Json5TokenType.LeftBracket: return ParseArray();
                case Json5TokenType.String:
                    return new ScriptValue(Consume(Json5TokenType.String).Text);
                case Json5TokenType.Int64:
                    {
                        var i64Token = Consume(Json5TokenType.Int64);
                        var i64Text = i64Token.Text;
                        var isPositive = i64Text.Length > 0 && i64Text[0] == '-' ? false : true;
                        var i64TrimSignText = i64Text.TrimStart('+', '-');

                        long value;
                        if (i64TrimSignText.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                        {
                            value = Convert.ToInt64(i64TrimSignText.Substring(2), 16);
                        }
                        else if (i64TrimSignText.StartsWith("0o", StringComparison.InvariantCultureIgnoreCase))
                        {
                            value = Convert.ToInt64(i64TrimSignText.Substring(2), 8);
                        }
                        else if (i64TrimSignText.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
                        {
                            value = Convert.ToInt64(i64TrimSignText.Substring(2), 2);
                        }
                        else if (long.TryParse(i64TrimSignText, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                        {
                        }
                        else
                        {
                            throw new FormatException($"Invalid integer format '{i64Text}' at line {i64Token.Line}, position {i64Token.Pos}");
                        }
                        if (!isPositive)
                            value = -value;
                        return new ScriptValue(value);
                    }
                case Json5TokenType.Float64:
                    {
                        var f64Token = Consume(Json5TokenType.Float64);
                        var f64Text = f64Token.Text;
                        var isPositive = f64Text.Length > 0 && f64Text[0] == '-' ? false : true;
                        var i64TrimSignText = f64Text.TrimStart('+', '-');

                        // NaN / Infinity
                        if (i64TrimSignText.Equals("NaN"))
                            return new ScriptValue(double.NaN);
                        else if (i64TrimSignText.Equals("Infinity"))
                            return new ScriptValue(isPositive ? double.PositiveInfinity : double.NegativeInfinity);

                        if (double.TryParse(f64Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double f))
                            return new ScriptValue(f);
                        throw new FormatException($"Invalid float format '{f64Text}' at line {f64Token.Line}, position {f64Token.Pos}");
                    }
                case Json5TokenType.Boolean:
                    var booleanToken = Consume(Json5TokenType.Boolean);
                    if (booleanToken.Text.Equals("true"))
                        return new ScriptValue(true);
                    else if (booleanToken.Text.Equals("false"))
                        return new ScriptValue(false);
                    else
                        throw new FormatException($"Invalid boolean format '{booleanToken.Text}' at line {booleanToken.Line}, position {booleanToken.Pos}");
                case Json5TokenType.Null:
                    Consume(Json5TokenType.Null);
                    return new ScriptValue();
                default:
                    throw new FormatException($"Unexpected token {Current.Type} at line {Current.Line}, position {Current.Pos}");
            }
        }

        private ScriptValue ParseObject(ScriptClass targetType)
        {
            Consume(Json5TokenType.LeftBrace);
            var obj = targetType == null ? BasicTypes.Dict.Create() : (targetType.Call(null, new List<HanabiLang.Parses.Nodes.AstNode>(), new Dictionary<string, HanabiLang.Parses.Nodes.AstNode>()).TryObject);
            var scriptValue = new ScriptValue(obj);
            var dictProperties = targetType == null ? ScriptDict.AsCSharp(obj) : null;
            // empty object
            if (Current.Type == Json5TokenType.RightBrace)
            {
                Consume(Json5TokenType.RightBrace);
                return scriptValue;
            }
            while (true)
            {
                string key;
                if (Current.Type == Json5TokenType.String)
                    key = Consume(Json5TokenType.String).Text;
                else
                    key = Consume(Json5TokenType.Identifier).Text;
                Consume(Json5TokenType.Colon);
                if (dictProperties == null)
                {
                    if (obj.TryGetValue(key, out ScriptVariable member))
                    {
                        if (!(member is ScriptVariable))
                            continue;
                        ScriptVariable variable = member;

                        if (variable.IsStatic || variable.Level != AccessibilityLevel.Public || variable.Set == null)
                            continue;

                        var varReference = variable.GetValueReference(obj, AccessibilityLevel.Public);

                        if (Current.Type == Json5TokenType.LeftBrace)
                        {
                            if (variable.DataTypes == null || variable.DataTypes.Contains(BasicTypes.Dict))
                            {
                                varReference.Ref = ParseObject(null);
                            }
                            else
                            {
                                var type = variable.DataTypes.First();
                                varReference.Ref = ParseObject(type);
                            }
                        }
                        else
                        {
                            var value = ParseValue();
                            var valueObj = value.TryObject;
                            var valueType = valueObj.ClassType;
                            if (variable.DataTypes == null || variable.DataTypes.Contains(valueType))
                            {
                                varReference.Ref = value;
                            }
                            else if (valueObj.IsTypeOrSubOf(BasicTypes.Int) && variable.DataTypes.Contains(BasicTypes.Decimal))
                            {
                                varReference.Ref = new ScriptValue((decimal)ScriptInt.AsCSharp(valueObj));
                            }
                            else if (valueObj.IsTypeOrSubOf(BasicTypes.Int) && variable.DataTypes.Contains(BasicTypes.Float))
                            {
                                varReference.Ref = new ScriptValue((double)ScriptInt.AsCSharp(valueObj));
                            }
                            else if (valueObj.IsTypeOrSubOf(BasicTypes.Float) && variable.DataTypes.Contains(BasicTypes.Decimal))
                            {
                                varReference.Ref = new ScriptValue((decimal)ScriptFloat.AsCSharp(valueObj));
                            }
                            else
                            {
                                throw new SystemException($"Cannot apply {valueType.Name} to object");
                            }
                        }
                    }
                }
                else
                {
                    dictProperties[new ScriptValue(key)] = ParseValue();
                }

                if (Current.Type == Json5TokenType.Comma)
                {
                    Consume(Json5TokenType.Comma);
                    // trailing comma
                    if (Current.Type == Json5TokenType.RightBrace)
                    {
                        Consume(Json5TokenType.RightBrace);
                        break;
                    }
                    continue;
                }
                else if (Current.Type == Json5TokenType.RightBrace)
                {
                    Consume(Json5TokenType.RightBrace);
                    break;
                }
                else
                {
                    throw new FormatException($"Expected ',' or '}}' in object at line {Current.Line}, position {Current.Pos}");
                }
            }
            return scriptValue;
        }

        private ScriptValue ParseArray()
        {
            Consume(Json5TokenType.LeftBracket);
            var list = BasicTypes.List.Create();
            var scriptValue = new ScriptValue(list);
            var listItems = ScriptList.AsCSharp(list);
            if (Current.Type == Json5TokenType.RightBracket)
            {
                // empty array
                Consume(Json5TokenType.RightBracket);
                return scriptValue;
            }
            while (true)
            {
                var value = ParseValue();
                listItems.Add(value);
                if (Current.Type == Json5TokenType.Comma)
                {
                    Consume(Json5TokenType.Comma);
                    // trailing comma
                    if (Current.Type == Json5TokenType.RightBracket)
                    {
                        Consume(Json5TokenType.RightBracket);
                        break;
                    }
                    continue;
                }
                else if (Current.Type == Json5TokenType.RightBracket)
                {
                    Consume(Json5TokenType.RightBracket);
                    break;
                }
                else
                {
                    throw new FormatException($"Expected ',' or ']' in array at line {Current.Line}, position {Current.Pos}");
                }
            }
            return scriptValue;
        }
    }
}
