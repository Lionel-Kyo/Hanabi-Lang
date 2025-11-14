using HanabiLangLib.Interprets.Json5Converter;
using HanabiLangLib.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptDict : ScriptClass
    {
        public ScriptDict() :
            base("Dict", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
            this.InitializeOperators();

            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                _this.BuildInObject = new Dictionary<ScriptValue, ScriptValue>();
                return ScriptValue.Null;
            });

            AddVariable("Length", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(AsCSharp(_this).Count);
            }, null, false, null);

            AddVariable("Iter", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var result = BasicTypes.Iterable.Create(AsCSharp(_this).Select(kv => new ScriptValue(BasicTypes.KeyValuePair.Create(kv))));
                return new ScriptValue(result);
            }, null, false, null);

            AddVariable("Keys", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(AsCSharp(_this).Keys.ToList());
            }, null, false, null);

            AddVariable("Values", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(AsCSharp(_this).Values.ToList());
            }, null, false, null);

            this.AddFunction("Clear", new List<FnParameter>() { new FnParameter("this") }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                AsCSharp(_this).Clear();
                return ScriptValue.Null;
            });

            this.AddFunction("ContainsKey", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("key")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue key = args[1];
                return new ScriptValue(AsCSharp(_this).ContainsKey(key));
            });

            this.AddFunction("ContainsValue", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue value = args[1];
                return new ScriptValue(AsCSharp(_this).ContainsValue(value));
            });

            this.AddFunction("Remove", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("key")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue key = args[1];
                return new ScriptValue(AsCSharp(_this).Remove(key));
            });

            this.AddFunction("GetValue", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("key")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue key = args[1];
                var dict = AsCSharp(_this);
                if (dict.TryGetValue(key, out var value))
                    return value;
                return ScriptValue.Null;
            });

            this.AddFunction("ToObject", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject obj = BasicTypes.ObjectClass.Create();
                foreach (var kv in AsCSharp(_this))
                {
                    if (kv.Key.TryObject?.ClassType == BasicTypes.Str)
                    {
                        string name = ScriptStr.AsCSharp(kv.Key.TryObject);
                        obj.Scope.Variables[name] = new ScriptVariable(name, null, kv.Value, true, false, AccessibilityLevel.Public);
                    }
                }
                return new ScriptValue(obj);
            });

            this.AddFunction("__GetIndexer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("key", BasicTypes.List) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                List<ScriptValue> indexes = ScriptList.AsCSharp(args[1].TryObject);
                if (indexes.Count > 1)
                    throw new ArgumentException("Only 1 indexer is allowed for Dict");
                ScriptValue key = indexes[0];

                return AsCSharp(_this)[key];
            });
            this.AddFunction("__SetIndexer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("key", BasicTypes.List), new FnParameter("value") }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                List<ScriptValue> indexes = ScriptList.AsCSharp(args[1].TryObject);
                if (indexes.Count > 1)
                    throw new ArgumentException("Only 1 indexer is allowed for Dict");
                ScriptValue key = indexes[0];

                AsCSharp(_this)[key] = args[2];
                return ScriptValue.Null;
            });

            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(BasicTypes.Str.Create(ToStr(_this, null)));
            });
        }

        private void InitializeOperators()
        {
            this.AddFunction(OPEARTOR_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                return new ScriptValue(OperatorEquals(args[0], args[1]));
            });
            this.AddFunction(OPEARTOR_NOT_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                return new ScriptValue(!OperatorEquals(args[0], args[1]));
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, new Dictionary<ScriptValue, ScriptValue>());
        public ScriptObject Create(Dictionary<ScriptValue, ScriptValue> value) => new ScriptObject(this, value);

        private bool OperatorEquals(ScriptValue value1, ScriptValue value2)
        {
            ScriptObject _this = value1.TryObject;
            ScriptObject _other = value2.TryObject;
            if (_other.IsTypeOrSubOf(BasicTypes.Dict))
            {
                var a = AsCSharp(_this);
                var b = AsCSharp(_other);

                if (a.Equals(b))
                    return true;

                if (a.Count != b.Count)
                    return false;

                foreach (var (l, r) in a.Zip(b, (l, r) => (l, r)))
                {
                    if (ScriptBool.AsCSharp(ScriptValue.NotEquals(l.Key, r.Key).TryObject) ||
                        ScriptBool.AsCSharp(ScriptValue.NotEquals(l.Value, r.Value).TryObject))
                        return false;
                }
                return true;
            }
            return false;
        }

        public static string ToStr(ScriptObject _this, string indent = null)
        {
            return ToStr(_this, indent, 0, new HashSet<ScriptValue>(new HashSet<ScriptValue>(ReferenceEqualityComparer.Instance)));
        }

        public static string ToStr(ScriptObject _this, string indent, int level, HashSet<ScriptValue> visited)
        {
            var dict = AsCSharp(_this);
            if (dict.Count == 0)
                return "{}";

            var result = new StringBuilder();
            result.Append('{');
            if (indent != null)
                result.AppendLine();

            int count = 0;
            foreach (var kv in dict)
            {
                result.Append(GetIndent(indent, level + 1));
                var keyObject = kv.Key.TryObject;
                if (visited.Contains(kv.Key))
                {
                    result.Append("{...}");
                }
                else
                {
                    visited.Add(kv.Key);
                    if (keyObject?.IsTypeOrSubOf(BasicTypes.Str) ?? false)
                        result.Append(Json5Serializer.QuoteString(ScriptStr.AsCSharp(keyObject), '"', false, true));
                    else if (keyObject?.IsTypeOrSubOf(BasicTypes.List) ?? false)
                        result.Append(ScriptList.ToStr(keyObject, indent, level + 1, visited));
                    else if (keyObject?.IsTypeOrSubOf(BasicTypes.Dict) ?? false)
                        result.Append(ToStr(keyObject, indent, level + 1, visited));
                    else
                        result.Append(keyObject.ToString());
                    visited.Remove(kv.Key);
                }

                result.Append(": ");
                var valueObject = kv.Value.TryObject;
                if (visited.Contains(kv.Value))
                {
                    result.Append("{...}");
                }
                else
                {
                    visited.Add(kv.Value);
                    if (valueObject?.IsTypeOrSubOf(BasicTypes.Str) ?? false)
                        result.Append(Json5Serializer.QuoteString(ScriptStr.AsCSharp(valueObject), '"', false, true));
                    else if (valueObject?.IsTypeOrSubOf(BasicTypes.List) ?? false)
                        result.Append(ScriptList.ToStr(valueObject, indent, level + 1, visited));
                    else if (valueObject?.IsTypeOrSubOf(BasicTypes.Dict) ?? false)
                        result.Append(ToStr(valueObject, indent, level + 1, visited));
                    else
                        result.Append(valueObject.ToString());
                    visited.Remove(kv.Value);
                }

                count++;
                if (count < dict.Count)
                    result.Append(", ");
                if (indent != null)
                    result.AppendLine();
            }

            result.Append(GetIndent(indent, level));
            result.Append('}');
            return result.ToString();
        }

        public static string GetIndent(string indent, int level)
        {
            if (indent == null || indent.Length <= 0 || level <= 0)
                return "";

            if (level == 1)
                return indent;

            var result = new StringBuilder(indent.Length * level);
            for (int i = 0; i < level; i++)
                result.Append(indent);
            return result.ToString();
        }

        public static Dictionary<ScriptValue, ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject;
        }
    }
}
