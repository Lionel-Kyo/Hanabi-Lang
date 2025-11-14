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

                foreach (var lri in a.Zip(b, (l, r) => Tuple.Create(l,r)).Select((lr, i) => Tuple.Create(lr.Item1, lr.Item2, i)))
                {
                    if (ScriptBool.AsCSharp(ScriptValue.NotEquals(lri.Item1.Key, lri.Item2.Key).TryObject) ||
                        ScriptBool.AsCSharp(ScriptValue.NotEquals(lri.Item1.Value, lri.Item2.Value).TryObject))
                        return false;
                }
                return true;
            }
            return false;
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
