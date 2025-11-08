using HanabiLangLib;
using HanabiLangLib.Interprets;
using HanabiLangLib.Interprets.ScriptTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace HanabiLangLib.Interprets.Json5Converter
{
    public class Json5Serializer
    {
        private readonly string _indent;
        private readonly bool _checkCircularRef;
        private readonly bool _quoteAllKeys;
        private readonly char _quoteStyle;
        private readonly bool _nanAsNull;
        private readonly bool _infinityAsNull;
        private readonly bool _trailingComma;
        private readonly bool _ensureAscii;
        private readonly bool _allowJson5EscapeSequences;
        private readonly string _doubleFormat;

        public Json5Serializer(
            string indent = null,
            bool checkCircularRef = true,
            bool quoteAllKeys = true,
            bool useSingleQuote = false,
            bool nanAsNull = false,
            bool infinityAsNull = false,
            bool trailingComma = false,
            bool allowScientific = false,
            bool allowJson5EscapeSequences = false,
            bool ensureAscii = false
        )
        {
            _indent = indent;
            _checkCircularRef = checkCircularRef;
            _quoteAllKeys = quoteAllKeys;
            _quoteStyle = useSingleQuote ? '\'' : '\"';
            _nanAsNull = nanAsNull;
            _infinityAsNull = infinityAsNull;
            _trailingComma = trailingComma;
            _doubleFormat = allowScientific ? "G" : ("0.0" + new string('#', 30));
            _allowJson5EscapeSequences = allowJson5EscapeSequences;
            _ensureAscii = ensureAscii;
        }

        public string Serialize(ScriptValue node)
        {
            var visited = _checkCircularRef ? new HashSet<ScriptValue>(ReferenceEqualityComparer.Instance) : null;
            return SerializeNode(node, 0, visited);
        }

        private string SerializeNode(ScriptValue node, int level, HashSet<ScriptValue> visited)
        {
            var obj = node.TryObject;
            if (obj == null)
                throw new Exception("Only object can be serialize to JSON.");

            if (_checkCircularRef && (obj.IsTypeOrSubOf(BasicTypes.List) || obj.IsTypeOrSubOf(BasicTypes.Dict)))
            {
                if (!visited.Add(node))
                    throw new Exception("Circular reference detected during JSON serialization.");
            }

            string result;

            if (obj.IsTypeOrSubOf(BasicTypes.Str))
                result = QuoteString(ScriptStr.AsCSharp(obj), _quoteStyle, _ensureAscii, _allowJson5EscapeSequences);
            else if (obj.IsTypeOrSubOf(BasicTypes.Int))
                result = ScriptInt.AsCSharp(obj).ToString(CultureInfo.InvariantCulture);
            else if (obj.IsTypeOrSubOf(BasicTypes.Float))
                result = SerializeFloat(ScriptFloat.AsCSharp(obj));
            else if (obj.IsTypeOrSubOf(BasicTypes.Decimal))
                result = SerializeDecimal(ScriptDecimal.AsCSharp(obj));
            else if (obj.IsTypeOrSubOf(BasicTypes.Bool))
                result = ScriptBool.AsCSharp(obj) ? "true" : "false";
            else if (obj.IsTypeOrSubOf(BasicTypes.Null))
                result = "null";
            else if (obj.IsTypeOrSubOf(BasicTypes.List))
                result = SerializeList(ScriptList.AsCSharp(obj), level, visited);
            else if (obj.IsTypeOrSubOf(BasicTypes.Dict))
                result = SerializeDict(ScriptDict.AsCSharp(obj), level, visited);
            else
                result = SerializeObject(obj, level, visited);

            if (_checkCircularRef && (obj.IsTypeOrSubOf(BasicTypes.List) || obj.IsTypeOrSubOf(BasicTypes.Dict)))
                visited.Remove(node);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string SerializeFloat(double f)
        {
            if (double.IsNaN(f))
                return (_nanAsNull ? "null" : "NaN");
            if (double.IsPositiveInfinity(f))
                return (_infinityAsNull ? "null" : "Infinity");
            if (double.IsNegativeInfinity(f))
                return (_infinityAsNull ? "null" : "-Infinity");
            return f.ToString(_doubleFormat, CultureInfo.InvariantCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string SerializeDecimal(decimal d)
        {
            return d.ToString(_doubleFormat, CultureInfo.InvariantCulture);
        }

        private string SerializeList(List<ScriptValue> list, int level, HashSet<ScriptValue> visited)
        {
            if (list.Count == 0)
                return "[]";

            var result = new StringBuilder();
            result.Append("[");
            if (_indent != null)
                result.AppendLine();

            for (int i = 0; i < list.Count; i++)
            {
                result.Append(GetIndent(level + 1));
                result.Append(SerializeNode(list[i], level + 1, visited));
                if (_trailingComma || i < list.Count - 1)
                    result.Append(",");
                if (_indent != null)
                    result.AppendLine();
            }

            result.Append(GetIndent(level));
            result.Append("]");
            return result.ToString();
        }

        private string SerializeDict(Dictionary<ScriptValue, ScriptValue> dict, int level, HashSet<ScriptValue> visited)
        {
            if (dict.Count == 0)
                return "{}";

            var result = new StringBuilder();
            result.Append("{");
            if (_indent != null)
                result.AppendLine();

            int count = 0;
            foreach (var kv in dict)
            {
                result.Append(GetIndent(level + 1));
                result.Append(SerializeKey(kv.Key.ToString()));
                result.Append(": ");
                result.Append(SerializeNode(kv.Value, level + 1, visited));
                count++;
                if (_trailingComma || count < dict.Count)
                    result.Append(",");
                if (_indent != null)
                    result.AppendLine();
            }

            result.Append(GetIndent(level));
            result.Append("}");
            return result.ToString();
        }

        private string SerializeObject(ScriptObject obj, int level, HashSet<ScriptValue> visited)
        {
            var variables = obj.Scope.Variables.Where(x => !x.Value.IsStatic && x.Value.Level == AccessibilityLevel.Public && x.Value.Get != null).ToList();
            if (variables.Count == 0)
                return "{}";

            var result = new StringBuilder();
            result.Append("{");
            if (_indent != null)
                result.AppendLine();

            int count = 0;
            foreach (var kv in obj.Scope.Variables)
            {
                var varReference = kv.Value.GetValueReference(obj, AccessibilityLevel.Public);
                var varValue = varReference.Ref;
                if (!varValue.IsObject)
                    throw new Exception("Only object can be serialize to JSON.");
                result.Append(GetIndent(level + 1));
                result.Append(SerializeKey(kv.Key));
                result.Append(": ");
                result.Append(SerializeNode(varValue, level + 1, visited));
                count++;
                if (_trailingComma || count < variables.Count)
                    result.Append(",");
                if (_indent != null)
                    result.AppendLine();
            }

            result.Append(GetIndent(level));
            result.Append("}");
            return result.ToString();
        }

        private string SerializeKey(string key)
        {
            if (_quoteAllKeys || !IsValidIdentifier(key))
                return QuoteString(key, _quoteStyle, _ensureAscii, _allowJson5EscapeSequences);
            return key;
        }

        private static bool IsValidIdentifier(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            for (int i = 0; i < key.Length; i++)
            {
                char c = key[i];
                if (!(char.IsLetter(c) || c == '_' || c == '$' || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LetterNumber))
                    return false;
            }
            return true;
        }

        public static string QuoteString(string value, char quoteStyle, bool ensureAscii, bool allowJson5EscapeSequences)
        {
            var result = new StringBuilder();
            result.Append(quoteStyle);
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\"': result.Append(quoteStyle == c ? "\\\"" : "\""); break;
                    case '\'': result.Append(quoteStyle == c ? "\\\'" : "\'"); break;
                    case '\\': result.Append("\\\\"); break;
                    case '\b': result.Append("\\b"); break;
                    case '\f': result.Append("\\f"); break;
                    case '\n': result.Append("\\n"); break;
                    case '\r': result.Append("\\r"); break;
                    case '\t': result.Append("\\t"); break;
                    // case '\v': result.Append("\\v"); break; // JSON5 Only
                    // case '\0': result.Append("\\0"); break; // JSON5 Only
                    default:
                        if (allowJson5EscapeSequences && (c == '\v' || c == '\0'))
                           result.Append(c == '\v' ? "\\v" : "\\0");
                        else if (ensureAscii && c > 0x7F)
                            result.Append("\\u" + ((int)c).ToString("x4"));
                        else if (char.IsControl(c) || char.IsLowSurrogate(c) || char.IsHighSurrogate(c))
                            result.Append("\\u" + ((int)c).ToString("x4"));
                        else if (c == '\u2028' || c == '\u2029' || // Line Separator, Paragraph Separator
                             (c >= '\u202A' && c <= '\u202E') || (c >= '\u2066' && c <= '\u2069') || // Bidi controls
                             (c >= '\uFDD0' && c <= '\uFDEF') || (c >= '\uFFFE' && c <= '\uFFFF') || // Noncharacters
                             (c >= '\u200B' && c <= '\u200D') || (c == '\uFEFF') || (c == '\u00A0') || // Zero-width, BOM, NBSP
                             // The following characters are Harmless
                             (c >= '\u2000' && c <= '\u200A') || // En space-hair space
                             (c == '\u202F' || c == '\u205F' || c == '\u3000') // Narrow no-break, medium mathematical, ideographic space
                            )
                            result.Append("\\u" + ((int)c).ToString("x4"));
                        else
                            result.Append(c);
                        break;
                }
            }
            result.Append(quoteStyle);
            return result.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetIndent(int level)
        {
            if (_indent == null || _indent.Length <= 0 || level <= 0)
                return "";

            if (level == 1)
                return _indent;

            var result = new StringBuilder(_indent.Length * level);
            for (int i = 0; i < level; i++)
                result.Append(_indent);
            return result.ToString();
        }
    }
}
