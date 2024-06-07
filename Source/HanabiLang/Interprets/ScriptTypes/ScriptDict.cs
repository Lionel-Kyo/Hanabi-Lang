using HanabiLang.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptDict : ScriptClass
    {
        public ScriptDict() :
            base("Dict", isStatic: false)
        {
            AddVariable("Length", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((Dictionary<ScriptValue, ScriptValue>)((ScriptObject)args[0].Value).BuildInObject).Count);
            }, null, false, null);

            AddVariable("Keys", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((Dictionary<ScriptValue, ScriptValue>)((ScriptObject)args[0].Value).BuildInObject).Keys.ToList());
            }, null, false, null);

            AddVariable("Values", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((Dictionary<ScriptValue, ScriptValue>)((ScriptObject)args[0].Value).BuildInObject).Values.ToList());
            }, null, false, null);

            this.AddObjectFn("Clear", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ((Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject).Clear();
                return ScriptValue.Null;
            });

            this.AddObjectFn("ContainsKey", new List<FnParameter>()
            {
                new FnParameter("key")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptValue key = args[1];
                return new ScriptValue(((Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject).ContainsKey(key));
            });

            this.AddObjectFn("ContainsValue", new List<FnParameter>()
            {
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptValue value = args[1];
                return new ScriptValue(((Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject).ContainsValue(value));
            });

            this.AddObjectFn("Remove", new List<FnParameter>()
            {
                new FnParameter("key")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptValue key = args[1];
                return new ScriptValue(((Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject).Remove(key));
            });

            this.AddObjectFn("GetValue", new List<FnParameter>()
            {
                new FnParameter("key")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptValue key = args[1];
                var dict = ((Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject);
                if (dict.TryGetValue(key, out var value))
                    return value;
                return ScriptValue.Null;
            });

            this.AddObjectFn("GetEnumerator", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var result = BasicTypes.Enumerator.Create();
                result.BuildInObject = DictIterator((Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject);
                return new ScriptValue(result);
            });

            this.AddObjectFn("get_[]", new List<FnParameter> { new FnParameter("index") }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue index = args[1];
                var dictValue = (Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject;

                return dictValue[index];
            });
            this.AddObjectFn("set_[]", new List<FnParameter> { new FnParameter("index"), new FnParameter("value") }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue index = args[1];
                var dictValue = (Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject; 
                dictValue[index] = args[2];
                return ScriptValue.Null;
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, new Dictionary<ScriptValue, ScriptValue>());
        public ScriptObject Create(Dictionary<ScriptValue, ScriptValue> value) => new ScriptObject(this, value);

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptDict)
            {
                var a = (Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject;
                var b = (Dictionary<ScriptValue, ScriptValue>)value.BuildInObject;

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

        private static IEnumerable<ScriptValue> DictIterator(Dictionary<ScriptValue, ScriptValue> value)
        {
            foreach (var c in value)
            {
                yield return new ScriptValue(BasicTypes.KeyValuePair.Create(c));
            }
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
            foreach (var item in (Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject)
            {
                if (!(item.Key.IsObject && item.Value.IsObject))
                    throw new SystemException("item.Key.IsObject or item.Value.IsObject is not object");

                ScriptObject keyObject = (ScriptObject)item.Key.Value;
                ScriptObject valueObject = (ScriptObject)item.Value.Value;
                result.Append(' ', currentIndent);
                result.Append($"{keyObject.ClassType.ToJsonString(keyObject, basicIndent, currentIndent)}");
                result.Append(": ");
                result.Append($"{valueObject.ClassType.ToJsonString(valueObject, basicIndent, currentIndent)}");
                if (count < ((Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject).Count - 1)
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
    }
}
