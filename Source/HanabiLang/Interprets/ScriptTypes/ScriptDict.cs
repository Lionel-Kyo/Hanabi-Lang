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
    class ScriptDict : ScriptObject, IEnumerable<ScriptValue>
    {
        public static ScriptClass CreateBuildInClass()
        {
            var newScrope = new ScriptScope(ScopeType.Class);
            return new ScriptClass("Dict", null, new List<string>(),
                newScrope, false, () => new ScriptDict());
        }
        public Dictionary<ScriptValue, ScriptValue> Value { get; private set; }
        public ScriptDict() :
            base(CreateBuildInClass())
        {
            this.Value = new Dictionary<ScriptValue, ScriptValue>();
            this.AddObjectFn("Length", args =>
            {
                return new ScriptValue(this.Value.Count);
            });
        }
        public ScriptDict(Dictionary<ScriptValue, ScriptValue> value) : this()
        {
            this.Value = value;
        }

        public override ScriptObject Equals(ScriptObject value)
        {
            if (value is ScriptDict)
            {
                Dictionary<ScriptValue, ScriptValue> a = this.Value;
                Dictionary<ScriptValue, ScriptValue> b = ((ScriptDict)value).Value;
                if (a.Count == b.Count)
                {
                    var list1 = a.ToList();
                    var list2 = a.ToList();
                    for (int i = 0; i < a.Count; i++)
                    {
                        if (!list1[i].Equals(list2[2]))
                            return new ScriptBool(false);
                    }
                    return new ScriptBool(true);
                }
                return new ScriptBool(false);
            }
            return new ScriptBool(false);
        }

        private static IEnumerable<ScriptValue> DictIterator(Dictionary<ScriptValue, ScriptValue> value)
        {
            foreach (var c in value)
            {
                yield return new ScriptValue(new List<ScriptValue>() { c.Key, c.Value });
            }
        }

        public IEnumerator<ScriptValue> GetEnumerator() => DictIterator(this.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => DictIterator(this.Value).GetEnumerator();

        public override ScriptStr ToStr() => new ScriptStr(this.ToString());

        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0)
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
            foreach (var item in this.Value)
            {
                result.Append(' ', currentIndent);
                result.Append($"{item.Key.ToJsonString(basicIndent, currentIndent)}");
                result.Append(": ");
                result.Append($"{item.Value.ToJsonString(basicIndent, currentIndent)}");
                if (count < this.Value.Count - 1)
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

        public override string ToString()
        {
            return ToJsonString(2);
        }

        /*public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }*/
    }
}
