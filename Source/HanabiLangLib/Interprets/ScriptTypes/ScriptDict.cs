using HanabiLang.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptDict : ScriptClass
    {
        public ScriptDict() :
            base("Dict", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
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
        }

        public override ScriptObject Create() => new ScriptObject(this, new Dictionary<ScriptValue, ScriptValue>());
        public ScriptObject Create(Dictionary<ScriptValue, ScriptValue> value) => new ScriptObject(this, value);

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptDict)
            {
                var a = AsCSharp(_this);
                var b = AsCSharp(value);

                if (a.Equals(b))
                    return ScriptBool.True;

                if (a.Count != b.Count)
                    return ScriptBool.False;

                var aList = a.ToList();
                var bList = b.ToList();
                for (int i = 0; i < a.Count; i++)
                {
                    if (!aList[i].Key.Equals(bList[i].Key) || !aList[i].Value.Equals(bList[i].Value))
                        return ScriptBool.False;
                }
                return ScriptBool.True;
            }
            return ScriptBool.False;
        }

        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            StringBuilder result = new StringBuilder();
            //result.Append(' ', currentIndent);
            result.Append('{');
            if (basicIndent != 0)
            {
                result.AppendLine();
                currentIndent += 2;
            }
            int count = 0;
            foreach (var item in AsCSharp(_this))
            {
                if (!(item.Key.IsObject && item.Value.IsObject))
                    throw new SystemException("Cannot convert Dict to str, Key not object or Value is not object");

                ScriptObject keyObject = (ScriptObject)item.Key.Value;
                ScriptObject valueObject = (ScriptObject)item.Value.Value;
                result.Append(' ', currentIndent);
                result.Append($"{keyObject.ClassType.ToJsonString(keyObject, basicIndent, currentIndent)}");
                result.Append(": ");
                result.Append($"{valueObject.ClassType.ToJsonString(valueObject, basicIndent, currentIndent)}");
                if (count < AsCSharp(_this).Count - 1)
                {
                    result.Append(", ");
                    if (basicIndent != 0)
                        result.AppendLine();
                }
                count++;
            }
            if (basicIndent != 0)
            {
                currentIndent -= 2;
                result.Append(' ', currentIndent);
                result.AppendLine();
            }
            result.Append(' ', currentIndent);
            result.Append('}');
            return result.ToString();
        }

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create(this.ToJsonString(_this, 0));

        public static Dictionary<ScriptValue, ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject;
        }
    }
}
