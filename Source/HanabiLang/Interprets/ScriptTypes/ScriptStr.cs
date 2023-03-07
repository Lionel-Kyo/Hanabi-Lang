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
    class ScriptStr : ScriptClass
    {
        public ScriptStr() :
            base("str", isStatic: false)
        {
            this.AddObjectFn(this.Name, new List<FnParameter>() 
            {
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                if (args.Count == 2)
                {
                    ScriptObject value = (ScriptObject)args[1].Value;
                    _this.BuildInObject = value.ToString();
                }
                return ScriptValue.Null;
            });

            this.AddObjectFn("SubStr", new List<FnParameter>()
            {
                new FnParameter("startIndex", BasicTypes.Int)
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long startIndex = (long)((ScriptObject)args[1].Value).BuildInObject;
                return new ScriptValue(((string)_this.BuildInObject).Substring((int)startIndex));
            });

            this.AddObjectFn("SubStr", new List<FnParameter>()
            {
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("length", BasicTypes.Int)
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long startIndex = (long)((ScriptObject)args[1].Value).BuildInObject;
                long length = (long)((ScriptObject)args[2].Value).BuildInObject;
                return new ScriptValue(((string)_this.BuildInObject).Substring((int)startIndex, (int)length));
            });

            this.AddObjectFn("Contains", new List<FnParameter>()
            {
                new FnParameter("value", BasicTypes.Str),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                string value = (string)((ScriptObject)args[1].Value).BuildInObject;
                return new ScriptValue(((string)_this.BuildInObject).Contains(value));
            });

            this.AddObjectFn("IndexOf", new List<FnParameter>()
            {
                new FnParameter("value", BasicTypes.Str),
                new FnParameter("startIndex", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                string value = (string)((ScriptObject)args[1].Value).BuildInObject;
                int startIndex = (int)((ScriptObject)args[2].Value).BuildInObject;
                if (args[3].IsNull)
                {
                    return new ScriptValue(((string)_this.BuildInObject).IndexOf(value, startIndex));
                }
                int count = (int)((ScriptObject)args[3].Value).BuildInObject;
                return new ScriptValue(((string)_this.BuildInObject).IndexOf(value, startIndex, count));
            });

            this.AddObjectFn("LastIndexOf", new List<FnParameter>()
            {
                new FnParameter("value", BasicTypes.Str),
                new FnParameter("startIndex", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                string value = (string)((ScriptObject)args[1].Value).BuildInObject;
                int startIndex = (int)((ScriptObject)args[2].Value).BuildInObject;
                if (args[3].IsNull)
                {
                    return new ScriptValue(((string)_this.BuildInObject).LastIndexOf(value, startIndex));
                }
                int count = (int)((ScriptObject)args[3].Value).BuildInObject;
                return new ScriptValue(((string)_this.BuildInObject).LastIndexOf(value, startIndex, count));
            });

            this.AddObjectFn("Remove", new List<FnParameter>()
            {
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                int startIndex = (int)((ScriptObject)args[1].Value).BuildInObject;
                if (args[2].IsNull)
                {
                    return new ScriptValue(((string)_this.BuildInObject).Remove(startIndex));
                }
                int count = (int)((ScriptObject)args[2].Value).BuildInObject;
                return new ScriptValue(((string)_this.BuildInObject).Remove(startIndex, count));
            });

            this.AddObjectFn("Replace", new List<FnParameter>()
            {
                new FnParameter("oldValue", BasicTypes.Str),
                new FnParameter("newValue", BasicTypes.Str),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                string oldValue = (string)((ScriptObject)args[1].Value).BuildInObject;
                string newValue = (string)((ScriptObject)args[2].Value).BuildInObject;
                return new ScriptValue(((string)_this.BuildInObject).Replace(oldValue, newValue));
            });

            this.AddObjectFn("Split", new List<FnParameter>()
            {
                new FnParameter("separator", BasicTypes.Str, null, true),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                string[] separator = new string[args.Count - 1];
                for (int i = 1; i < args.Count; i++)
                {
                    separator[i - 1] = (string)((ScriptObject)args[i].Value).BuildInObject;
                }
                List<ScriptValue> result = new List<ScriptValue>();
                foreach (string value in ((string)_this.BuildInObject).Split(separator, StringSplitOptions.None))
                {
                    result.Add(new ScriptValue(value));
                }
                return new ScriptValue(result);
            });

            this.AddObjectFn("StartsWith", new List<FnParameter>()
            {
                new FnParameter("value", BasicTypes.Str),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                string value = (string)((ScriptObject)args[1].Value).BuildInObject;
                return new ScriptValue(((string)_this.BuildInObject).StartsWith(value));
            });

            this.AddObjectFn("EndsWith", new List<FnParameter>()
            {
                new FnParameter("value", BasicTypes.Str),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                string value = (string)((ScriptObject)args[1].Value).BuildInObject;
                return new ScriptValue(((string)_this.BuildInObject).EndsWith(value));
            });

            this.AddObjectFn("ToLower", new List<FnParameter>()
            , args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((string)_this.BuildInObject).ToLower());
            });

            this.AddObjectFn("Trim", new List<FnParameter>()
            {
                new FnParameter("chars", BasicTypes.Str, null, true),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                char[] chars = new char[args.Count - 1];
                if (chars.Length == 0)
                    return new ScriptValue(((string)_this.BuildInObject).Trim());
                for (int i = 1; i < args.Count; i++)
                {
                    chars[i - 1] = ((string)((ScriptObject)args[i].Value).BuildInObject)[0];
                }
                return new ScriptValue(((string)_this.BuildInObject).Trim(chars));
            });

            this.AddObjectFn("TrimStart", new List<FnParameter>()
            {
                new FnParameter("chars", BasicTypes.Str, null, true),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                char[] chars = new char[args.Count - 1];
                if (chars.Length == 0)
                    return new ScriptValue(((string)_this.BuildInObject).TrimStart(' '));
                for (int i = 1; i < args.Count; i++)
                {
                    chars[i - 1] = ((string)((ScriptObject)args[i].Value).BuildInObject)[0];
                }
                return new ScriptValue(((string)_this.BuildInObject).TrimStart(chars));
            });

            this.AddObjectFn("TrimEnd", new List<FnParameter>()
            {
                new FnParameter("chars", BasicTypes.Str, null, true),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                char[] chars = new char[args.Count - 1];
                if (chars.Length == 0)
                    return new ScriptValue(((string)_this.BuildInObject).TrimStart(' '));
                for (int i = 1; i < args.Count; i++)
                {
                    chars[i - 1] = ((string)((ScriptObject)args[i].Value).BuildInObject)[0];
                }
                return new ScriptValue(((string)_this.BuildInObject).TrimEnd(chars));
            });

            AddVariable("Length", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((string)((ScriptObject)args[0].Value).BuildInObject).Length);
            }, null, false, null);

            this.AddObjectFn("GetEnumerator", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var result = BasicTypes.Enumerator.Create();
                result.BuildInObject = StrIterator((string)_this.BuildInObject);
                return new ScriptValue(result);
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, "");
        public ScriptObject Create(string value) => new ScriptObject(this, value);
        public ScriptObject Create(StringBuilder value) => new ScriptObject(this, value.ToString());
        public ScriptObject Create(char value) => new ScriptObject(this, value.ToString());

        public override ScriptObject Add(ScriptObject _this, ScriptObject value)
        {
            StringBuilder result = new StringBuilder((string)_this.BuildInObject);
            result.Append(value.ToString());
            return BasicTypes.Str.Create(result);
        }

        public override ScriptObject Multiply(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                StringBuilder result = new StringBuilder((string)_this.BuildInObject);
                long number = (long)value.BuildInObject;
                for (long i = 1; i < number; i++)
                {
                    result.Append((string)_this.BuildInObject);
                }
                return BasicTypes.Str.Create(result);
            }

            return base.Multiply(_this, value);
        }

        private static IEnumerable<ScriptValue> StrIterator(string value)
        {
            foreach (char c in value)
            {
                yield return new ScriptValue(c);
            }
        }

        public override ScriptObject ToStr(ScriptObject _this) => _this;

        public override ScriptObject Equals(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType is ScriptStr && right.ClassType is ScriptStr)
                return BasicTypes.Bool.Create(left.BuildInObject.Equals(right.BuildInObject));
            return base.Equals(left, right);
        }

        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            return "\"" + (string)_this.BuildInObject +"\"";
        }
    }
}
