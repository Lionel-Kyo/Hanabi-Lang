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
            base("Dict", null, null, BasicTypes.ObjectClass, false, AccessibilityLevel.Public)
        {
            AddVariable("Length", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((Dictionary<ScriptValue, ScriptValue>)((ScriptObject)args[0].Value).BuildInObject).Count);
            }, null, true, null);

            this.AddObjectFn("GetEnumerator", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var result = BasicTypes.Enumerator.Create();
                result.BuildInObject = DictIterator((Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject);
                return new ScriptValue(result);
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, new Dictionary<ScriptValue, ScriptValue>());
        public ScriptObject Create(Dictionary<ScriptValue, ScriptValue> value) => new ScriptObject(this, value);

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptDict)
            {
                if (_this.Equals(value))
                    return ScriptBool.True;

                var a = (Dictionary<ScriptValue, ScriptValue>)_this.BuildInObject;
                var b = (Dictionary<ScriptValue, ScriptValue>)value.BuildInObject;
                if (a.Count == b.Count)
                {
                    var list1 = a.ToList();
                    var list2 = a.ToList();
                    for (int i = 0; i < a.Count; i++)
                    {
                        if (!list1[i].Equals(list2[2]))
                            return ScriptBool.False;
                    }
                    return ScriptBool.True;
                }
                return ScriptBool.False;
            }
            return ScriptBool.False;
        }

        private static IEnumerable<ScriptValue> DictIterator(Dictionary<ScriptValue, ScriptValue> value)
        {
            foreach (var c in value)
            {
                yield return new ScriptValue(new List<ScriptValue>() { c.Key, c.Value });
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

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create(this.ToJsonString(_this));
    }
}
