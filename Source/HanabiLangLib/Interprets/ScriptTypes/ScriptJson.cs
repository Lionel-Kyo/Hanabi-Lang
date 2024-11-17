using HanabiLang.Interprets;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Lexers;
using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptJson : ScriptClass
    {
        public ScriptJson() : base("Json", true)
        {
            this.AddFunction("Deserialize", new List<FnParameter>()
            {
                new FnParameter("jsonText", BasicTypes.Str),
                new FnParameter("objType", new HashSet<ScriptClass>{ BasicTypes.TypeClass, BasicTypes.Null }, new ScriptValue())
            }, args =>
            {
                string[] jsonLines = ((string)((ScriptObject)args[0].Value).BuildInObject).Replace("\r", "").Split('\n');
                var tokens = Lexer.Tokenize(jsonLines);
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                if (ast.Nodes.Count != 1)
                    throw new SystemException("Incorrect format of json");

                var jsonValue = Interpreter.InterpretJson(ast.Nodes.First());

                if (args.Count >= 2 && !args[1].IsNull)
                {
                    ScriptObject typeObj = args[1].TryObject;
                    ScriptClass targetType = (ScriptClass)typeObj.BuildInObject;
                    if (jsonValue.Ref.TryObject.ClassType == targetType)
                        return jsonValue.Ref;

                    if (jsonValue.Ref.TryObject.ClassType != BasicTypes.Dict)
                        throw new SystemException($"Cannot apply {jsonValue.Ref.TryObject.ClassType.Name} to object");

                    return new ScriptValue(ToScriptObject(jsonValue.Ref.TryObject, targetType));
                }

                return jsonValue.Ref;
            });

            this.AddFunction("Serialize", new List<FnParameter>()
            {
                new FnParameter("obj"),
                new FnParameter("indent", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("ensureAscii", BasicTypes.Bool, new ScriptValue(false)),
                new FnParameter("skipListIndent", BasicTypes.Bool, new ScriptValue(false)),
            }, args =>
            {
                if (!args[0].IsObject)
                    throw new SystemException("Only object can be serialize to json");
                return new ScriptValue(ToJsonString(args[0].TryObject, false, 
                    (int)ScriptInt.AsCSharp(args[1].TryObject), null, ScriptBool.AsCSharp(args[2].TryObject), ScriptBool.AsCSharp(args[3].TryObject)));

            }, true, AccessibilityLevel.Public);
        }

        private static ScriptObject ToScriptObject(ScriptObject jsonDict, ScriptClass targetType)
        {
            ScriptObject result = targetType.Call(null, new List<Parses.Nodes.AstNode>(), new Dictionary<string, Parses.Nodes.AstNode>()).TryObject;
            foreach (var kv in (Dictionary<ScriptValue, ScriptValue>)jsonDict.BuildInObject)
            {
                if (result.TryGetValue(kv.Key.TryObject.ToString(), out ScriptType member))
                {
                    if (!(member is ScriptVariable))
                        continue;
                    ScriptVariable variable = (ScriptVariable)member;

                    if (variable.IsStatic || variable.Level != AccessibilityLevel.Public || variable.Set == null)
                        continue;

                    var varReference = variable.GetValueReference(result, AccessibilityLevel.Public);
                    var valueType = kv.Value.TryObject.ClassType;
                    if (variable.DataTypes == null || variable.DataTypes.Contains(valueType))
                    {
                        varReference.Ref = kv.Value;
                    }
                    else if (valueType == BasicTypes.Int && variable.DataTypes.Contains(BasicTypes.Decimal))
                    {
                        varReference.Ref = new ScriptValue((decimal)((long)kv.Value.TryObject.BuildInObject));
                    }
                    else if (valueType == BasicTypes.Float && variable.DataTypes.Contains(BasicTypes.Decimal))
                    {
                        varReference.Ref = new ScriptValue((decimal)((double)kv.Value.TryObject.BuildInObject));
                    }
                    else
                    {
                        if (valueType != BasicTypes.Dict)
                            throw new SystemException($"Cannot apply {valueType.Name} to object");

                        var type = variable.DataTypes.First();
                        varReference.Ref = new ScriptValue(ToScriptObject(kv.Value.TryObject, type));
                    }
                }
            }
            return result;
        }

        private static string ToJsonString(ScriptObject obj, bool isKey, int basicIndent, int? subIntent, bool ensureAscii, bool skipListIndent)
        {
            if (basicIndent < 0)
                basicIndent = 0;
            int indentation = subIntent.HasValue ? subIntent.Value : basicIndent;
            if (obj.ClassType == BasicTypes.Null)
                return "null";
            else if (obj.ClassType == BasicTypes.Int || obj.ClassType == BasicTypes.Float || obj.ClassType == BasicTypes.Decimal)
                return isKey ? $"\"{obj.BuildInObject}\"" : obj.BuildInObject.ToString();
            else if (obj.ClassType == BasicTypes.Bool)
                return isKey ? $"\"{(ScriptBool.AsCSharp(obj) ? "true" : "false")}\"" : ((bool)obj.BuildInObject) ? "true" : "false";
            else if (obj.ClassType == BasicTypes.Str)
                return StringToJsonString(ScriptStr.AsCSharp(obj), ensureAscii);
            else if (obj.ClassType == BasicTypes.List)
            {
                var list = ScriptList.AsCSharp(obj);
                if (list.Count <= 0)
                    return isKey ? "\"[]\"" : "[]";

                StringBuilder result = new StringBuilder();
                result.Append('[');

                foreach (var item in list)
                {
                    if (!item.IsObject)
                        throw new SystemException("Only object can be serialized to json");

                    if (!skipListIndent && indentation > 0)
                    {
                        result.Append('\n');
                        result.Append(' ', indentation);
                    }
                    result.Append(ToJsonString(item.TryObject, false, basicIndent, indentation + basicIndent, ensureAscii, skipListIndent));
                    result.Append(", ");
                }

                if (result.Length >= 2)
                    result.Remove(result.Length - 2, 2);

                if (!skipListIndent && indentation > 0)
                {
                    result.Append('\n');
                    result.Append(' ', indentation - basicIndent);
                }
                result.Append(']');

                return isKey ? $"\"{result}\"" : result.ToString();
            }
            else if (obj.ClassType == BasicTypes.Dict)
            {
                var dict = ScriptDict.AsCSharp(obj);
                if (dict.Count <= 0)
                    return isKey ? "\"{}\"" : "{}";

                StringBuilder result = new StringBuilder();
                result.Append("{");

                foreach (var item in dict)
                {
                    if (!item.Key.IsObject || !item.Value.IsObject)
                        throw new SystemException("Only object can be serialized to json");

                    if (indentation > 0)
                    {
                        result.Append('\n');
                        result.Append(' ', indentation);
                    }
                    result.Append(ToJsonString(item.Key.TryObject, true, basicIndent, indentation + basicIndent, ensureAscii, skipListIndent));
                    result.Append(": ");
                    result.Append(ToJsonString(item.Value.TryObject, false, basicIndent, indentation + basicIndent, ensureAscii, skipListIndent));

                    result.Append(", ");
                }

                if (result.Length >= 2)
                    result.Remove(result.Length - 2, 2);

                if (indentation > 0)
                {
                    result.Append('\n');
                    result.Append(' ', indentation - basicIndent);
                }
                result.Append("}");

                return isKey ? $"\"{result}\"" : result.ToString();
            }
            else
            {
                var variables = obj.Scope.Variables.Where(x => !x.Value.IsStatic && x.Value.Level == AccessibilityLevel.Public && x.Value.Get != null).ToArray();

                if (variables.Length <= 0)
                    return isKey ? "\"{}\"" : "{}";

                StringBuilder result = new StringBuilder();
                result.Append("{");

                foreach (var item in variables)
                {
                    var varReference = item.Value.GetValueReference(obj, AccessibilityLevel.Public);
                    if (!varReference.Ref.IsObject)
                        throw new SystemException("Only object can be serialized to json");
                    if (indentation > 0)
                    {
                        result.Append('\n');
                        result.Append(' ', indentation);
                    }
                    result.Append('\"');
                    result.Append(item.Key);
                    result.Append('\"');
                    result.Append(": ");
                    result.Append(ToJsonString(varReference.Ref.TryObject, false, basicIndent, indentation + basicIndent, ensureAscii, skipListIndent));
                    result.Append(", ");
                }

                if (result.Length >= 2)
                    result.Remove(result.Length - 2, 2);

                if (indentation > 0)
                {
                    result.Append('\n');
                    result.Append(' ', indentation - basicIndent);
                }
                result.Append("}");

                return isKey ? $"\"{result}\"" : result.ToString();
            }
        }


        public static string StringToJsonString(string text, bool ensureAscii)
        {
            StringBuilder result = new StringBuilder();
            result.Append('\"');
            foreach (char c in text)
            {
                if (ensureAscii)
                {
                    if (c >= ' ' && c <= '~')
                        result.Append(c);
                    else
                        result.Append("\\u" + ((int)c).ToString("X4"));
                }
                else
                {
                    switch (c)
                    {
                        case '\'':
                            result.Append("\\'");
                            continue;
                        case '\"':
                            result.Append("\\\"");
                            continue;
                        case '\\':
                            result.Append("\\\\");
                            continue;
                        case '\b':
                            result.Append("\\b");
                            continue;
                        case '\f':
                            result.Append("\\f");
                            continue;
                        case '\n':
                            result.Append("\\n");
                            continue;
                        case '\r':
                            result.Append("\\r");
                            continue;
                        case '\t':
                            result.Append("\\t");
                            continue;
                    }

                    if (char.IsControl(c) || char.IsLowSurrogate(c) || char.IsHighSurrogate(c))
                        result.Append("\\u" + ((int)c).ToString("X4"));
                    else
                        result.Append(c);
                }
            }
            result.Append('\"');
            return result.ToString();
        }
    }
}
