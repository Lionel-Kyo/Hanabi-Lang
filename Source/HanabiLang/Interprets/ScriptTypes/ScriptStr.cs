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
    class ScriptStr : ScriptObject, IEnumerable<ScriptValue>
    {
        public static ScriptClass CreateBuildInClass()
        {
            var newScrope = new ScriptScope(ScopeType.Class);
            return new ScriptClass("str", null, new List<string>(),
                newScrope, false, () => new ScriptStr());
        }

        public string Value { get; private set; }

        public ScriptStr() :
            base(CreateBuildInClass())
        {
            this.Value = "";
            this.AddObjectFn(this.ObjectClass.Name, args =>
            {
                if (args.Count == 1)
                {
                    ScriptValue value = args[0];
                    if (value.Value is ScriptObject)
                    {
                        this.Value = ((ScriptObject)value.Value).ToString();
                    }
                }
                return ScriptValue.Null;
            });
            this.AddObjectFn("SubStr", args =>
            {
                if (args.Count == 1)
                {
                    ScriptValue startIndex = args[0];
                    if (startIndex.Value is ScriptObject)
                    {
                        return new ScriptValue(this.Value.Substring((int)((ScriptInt)startIndex.Value).Value));
                    }
                }
                else if (args.Count == 2)
                {
                    ScriptValue startIndex = args[0];
                    ScriptValue length = args[1];
                    if (startIndex.Value is ScriptInt && length.Value is ScriptInt)
                    {
                        return new ScriptValue(this.Value.Substring((int)((ScriptInt)startIndex.Value).Value, (int)((ScriptInt)length.Value).Value));
                    }
                }
                return ScriptValue.Null;
            });

            this.AddObjectFn("Length", args =>
            {
                return new ScriptValue(this.Value.Length);
            });
        }

        public override ScriptObject Add(ScriptObject value)
        {
            StringBuilder result = new StringBuilder(this.Value);
            result.Append(value.ToString());
            return new ScriptStr(result);
        }

        public override ScriptObject Multiply(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                StringBuilder result = new StringBuilder(this.Value);
                long number = ((ScriptInt)value).Value;
                for (long i = 0;i< number; i++)
                {
                    result.Append(this.Value);
                }
                return new ScriptStr(result);
            }

            return base.Multiply(value);
        }

        public ScriptStr(char value) : this()
        {
            this.Value = value.ToString();
        }

        public ScriptStr(string value) : this()
        {
            this.Value = value;
        }
        public ScriptStr(StringBuilder strbdr) : this()
        {
            this.Value = strbdr.ToString();
        }

        private static IEnumerable<ScriptValue> StrIterator(string value)
        {
            foreach (char c in value)
            {
                yield return new ScriptValue(c);
            }
        }

        public override ScriptObject Equals(ScriptObject value)
        {
            if (value is ScriptStr)
            {
                return new ScriptBool(this.Value.Equals(((ScriptStr)value).Value));
            }
            return ScriptBool.False;
        }

        public IEnumerator<ScriptValue> GetEnumerator() => StrIterator(this.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => StrIterator(this.Value).GetEnumerator();

        public override ScriptStr ToStr() => new ScriptStr(this.ToString());
        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0) => $"\"{this.Value}\"";
        public override string ToString() => this.Value;

        public override ScriptObject Copy()
        {
            return new ScriptStr(this.Value);
        }

        /*public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }*/
    }
}
