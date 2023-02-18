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
            base("str", null, new ScriptScope(ScopeType.Class), false, AccessibilityLevel.Public)
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

            this.AddObjectFn("Length", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((List<ScriptValue>)((ScriptObject)args[0].Value).BuildInObject).Count);
            });

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
