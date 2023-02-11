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
    class ScriptList : ScriptObject, IEnumerable<ScriptValue>
    {
        public static ScriptClass CreateBuildInClass()
        {
            var newScrope = new ScriptScope(ScopeType.Class);
            return new ScriptClass("List", null, new List<string>(),
                newScrope, false, () => new ScriptList());
        }
        public List<ScriptValue> Value { get; private set; }
        public ScriptList() :
            base(CreateBuildInClass())
        {
            this.Value = new List<ScriptValue>();
            this.AddObjectFn("Length", args =>
            {
                return new ScriptValue(this.Value.Count);
            });
            this.AddObjectFn("Add", args =>
            {
                this.Value.Add(args[0]);
                return ScriptValue.Null;
            });
        }
        public ScriptList(List<ScriptValue> value) : this()
        {
            this.Value = value;
        }

        public override ScriptObject Negative()
        {
            var result = Value.ToList();
            result.Reverse();
            return new ScriptList(result);
        }

        public override ScriptObject Add(ScriptObject value)
        {
            if (value is ScriptList)
            {
                ScriptList obj = (ScriptList)value;
                List<ScriptValue> list = new List<ScriptValue>(this.Value);
                list.AddRange(obj.Value);
                return new ScriptList(list);
            }
            return base.Add(value);
        }

        public override ScriptObject Multiply(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                List<ScriptValue> list = new List<ScriptValue>();
                for (long i = 0; i < obj.Value; i++)
                {
                    list.AddRange(this.Value);
                }
                return new ScriptList(list);
            }
            return base.Add(value);
        }

        public override ScriptObject Equals(ScriptObject value)
        {
            if (value is ScriptList)
            {
                List<ScriptValue> a = this.Value;
                List<ScriptValue> b = ((ScriptList)value).Value;
                if (a.Count == b.Count)
                {
                    for (int i = 0; i < a.Count; i++)
                    {
                        if (!a[i].Equals(b[i]))
                            return new ScriptBool(false);
                    }
                    return new ScriptBool(true);
                }
                return new ScriptBool(false);
            }
            return new ScriptBool(false);
        }


        public IEnumerator<ScriptValue> GetEnumerator() => this.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Value.GetEnumerator();

        public override ScriptStr ToStr() => new ScriptStr(this.ToString());
        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0)
        {
            StringBuilder result = new StringBuilder();
            result.Append('[');
            if (basicIndent != 0)
            {
                result.AppendLine();
                currentIndent += 2;
            }
            int count = 0;
            foreach (var item in this.Value)
            {
                result.Append(' ', currentIndent);
                result.Append($"{item.ToJsonString(basicIndent, currentIndent)}");

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
            result.Append(']');
            return result.ToString();
        }
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append('[');
            foreach (var item in this.Value)
            {
                if (item.Value is ScriptStr)
                    result.Append($"\"{item}\", ");
                else
                    result.Append($"{item}, ");

            }
            if (result.Length > 1)
                result.Remove(result.Length - 2, 2);
            result.Append(']');
            return result.ToString();
        }

        /*public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }*/
    }
}
