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
                    if (jsonValue.Ref.TryObject.ClassType != BasicTypes.Dict)
                        throw new SystemException($"Cannot apply {jsonValue.Ref.TryObject.ClassType.Name} to object");

                    ScriptObject typeObj = args[1].TryObject;
                    ScriptClass targetType = (ScriptClass)typeObj.BuildInObject;
                    return new ScriptValue(ToScriptObject(jsonValue.Ref.TryObject, targetType));
                }

                return jsonValue.Ref;
            });

            this.AddFunction("Serialize", new List<FnParameter>()
            {
                new FnParameter("obj")
            }, args =>
            {
                if (!args[0].IsObject)
                    throw new SystemException("Only object can be serialize to json");
                return new ScriptValue(ToJsonString(args[0].TryObject));

            }, true, HanabiLang.AccessibilityLevel.Public);
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

        private static string ToJsonString(ScriptObject obj, bool isKey=false)
        {
            if (obj.ClassType == BasicTypes.Null)
                return "null";
            else if (obj.ClassType == BasicTypes.Int || obj.ClassType == BasicTypes.Float || obj.ClassType == BasicTypes.Decimal)
                return isKey ? $"\"{obj.BuildInObject}\"" : obj.BuildInObject.ToString();
            else if (obj.ClassType == BasicTypes.Bool)
                return isKey ? $"\"{(((bool)obj.BuildInObject) ? "true" : "false")}\"" : ((bool)obj.BuildInObject) ? "true" : "false";
            else if (obj.ClassType == BasicTypes.Str)
                return StringToJsonString((string)obj.BuildInObject);
            else if (obj.ClassType == BasicTypes.List)
            {
                var list = (List<ScriptValue>)obj.BuildInObject;
                if (list.Count <= 0)
                    return isKey ? "\"[]\"" : "[]";
                StringBuilder result = new StringBuilder();
                result.Append('[');
                foreach (var item in list)
                {
                    if (!item.IsObject)
                        throw new SystemException("Only object can be serialize to json");
                    result.Append($"{ToJsonString(item.TryObject)}, ");
                }
                result.Remove(result.Length - 2, 2);
                result.Append(']');
                return isKey ? $"\"{result}\"" : result.ToString();
            }
            else if (obj.ClassType == BasicTypes.Dict)
            {
                var dict = (Dictionary<ScriptValue, ScriptValue>)obj.BuildInObject;
                if (dict.Count <= 0)
                    return isKey ? "\"{}\"" : "{}";
                StringBuilder result = new StringBuilder();
                result.Append("{ ");
                foreach (var item in dict)
                {
                    if (!item.Key.IsObject || !item.Value.IsObject)
                        throw new SystemException("Only object can be serialize to json");
                    result.Append($"{ToJsonString(item.Key.TryObject, true)}: {ToJsonString(item.Value.TryObject)}, ");
                }
                result.Remove(result.Length - 2, 2);
                result.Append(" }");
                return isKey ? $"\"{result}\"" : result.ToString();
            }
            else
            {
                var variables = obj.Scope.Variables.Where(x => !x.Value.IsStatic && x.Value.Level == AccessibilityLevel.Public && x.Value.Get != null).ToArray();

                if (variables.Length <= 0)
                    return isKey ? "\"{}\"" : "{}";

                StringBuilder result = new StringBuilder();
                result.Append("{ ");
                foreach (var item in variables)
                {
                    var varReference = item.Value.GetValueReference(obj, AccessibilityLevel.Public);
                    result.Append($"\"{item.Key}\": {ToJsonString(varReference.Ref.TryObject)}, ");
                }
                result.Remove(result.Length - 2, 2);
                result.Append(" }");
                return isKey ? $"\"{result}\"" : result.ToString();
            }
        }

        public static string StringToJsonString(string text)
        {
            StringBuilder result = new StringBuilder();
            result.Append('\"');
            foreach (char c in text)
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

                if ((c >= ' ' && c <= '~') || c > '\u00A0') 
                    result.Append(c);
                else
                    result.Append("\\u" + ((int)c).ToString("X4"));
            }
            result.Append('\"');
            return result.ToString();
        }
    }
}
