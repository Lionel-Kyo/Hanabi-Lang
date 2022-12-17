using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

namespace HanabiLang.Interprets
{
    class ScriptValue
    {
        private ScriptType value { get; set; }
        public ScriptType Value => value;

        public ScriptValue(ScriptObject obj)
        {
            this.value = obj;
        }

        public ScriptValue(ScriptFn fn)
        {
            this.value = fn;
        }

        public ScriptValue(ScriptClass _class)
        {
            this.value = _class;
        }

        public ScriptValue(byte value)
        {
            this.value = new ScriptInt(value);
        }
        public ScriptValue(ushort value)
        {
            this.value = new ScriptInt(value);
        }
        public ScriptValue(uint value)
        {
            this.value = new ScriptInt(value);
        }
        public ScriptValue(sbyte value)
        {
            this.value = new ScriptInt(value);
        }
        public ScriptValue(short value)
        {
            this.value = new ScriptInt(value);
        }
        public ScriptValue(int value)
        {
            this.value = new ScriptInt( value);
        }
        public ScriptValue(long value)
        {
            this.value = new ScriptInt(value);
        }
        public ScriptValue(float value)
        {
            this.value = new ScriptFloat(value);
        }
        public ScriptValue(double value)
        {
            this.value = new ScriptFloat(value);
        }
        public ScriptValue(decimal value)
        {
            this.value = new ScriptDecimal(value); 
        }
        public ScriptValue(bool value)
        {
            this.value = new ScriptBool(value);
        }
        public ScriptValue(string value)
        {
            this.value = new ScriptStr(value);
        }
        public ScriptValue(char value)
        {
            this.value = new ScriptStr(value);
        }
        public ScriptValue(StringBuilder value)
        {
            this.value = new ScriptStr(value);
        }
        public ScriptValue(List<ScriptValue> value)
        {
            this.value = new ScriptList(value);
        }
        public ScriptValue(Dictionary<ScriptValue, ScriptValue> value)
        {
            this.value = new ScriptDict(value);
        }
        public ScriptValue()
        {
            this.value = new ScriptNull();
        }

        public static ScriptValue Null => new ScriptValue();

        public bool IsFunction => this.value is ScriptFn;
        public bool IsClass => this.value is ScriptClass;
        public bool IsObject => this.value is ScriptObject;


        private static bool CheckLRType(ScriptValue a, Func<ScriptValue, bool> checkA,
            ScriptValue b, Func<ScriptValue, bool> checkB, out Tuple<ScriptType, ScriptType> orderdValue)
        {
            if (checkA(a) && checkB(b))
            {
                orderdValue = Tuple.Create(a.value, b.value);
                return true;
            }
            else if (checkB(a) && checkA(b))
            {
                orderdValue = Tuple.Create(b.value, a.value);
                return true;
            }
            orderdValue = null;
            return false;
        }

        public static ScriptValue operator !(ScriptValue a)
        {
            if (a.value is ScriptObject)
            {
                var obj = (ScriptObject)a.value;
                return new ScriptValue(obj.Not());
            }
            throw new SystemException("operator ! is not defined");
        }
        public static ScriptValue operator +(ScriptValue a)
        {
            if (a.value is ScriptObject)
            {
                var obj = (ScriptObject)a.value;
                return new ScriptValue(obj.Positive());
            }
            throw new SystemException("operator + is not defined");
        }
        public static ScriptValue operator -(ScriptValue a)
        {
            if (a.value is ScriptObject)
            {
                var obj = (ScriptObject)a.value;
                return new ScriptValue(obj.Negative());
            }
            throw new SystemException("operator - is not defined");
        }

        public static ScriptValue operator +(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.Add(right));
            }
            throw new SystemException($"cannot + value between {a.value} and {b.value}");
        }

        public static ScriptValue operator -(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.Minus(right));
            }
            throw new SystemException($"cannot - value between {a.value} and {b.value}");
        }

        public static ScriptValue operator *(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.Multiply(right));
            }
            throw new SystemException($"cannot * value between {a.value} and {b.value}");
        }

        public static ScriptValue operator /(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.Divide(right));
            }
            throw new SystemException($"cannot / value between {a.value} and {b.value}");
        }

        public static ScriptValue operator %(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.Modulo(right));
            }
            throw new SystemException($"cannot % value between {a.value} and {b.value}");
        }

        public static ScriptValue operator >(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.Larger(right));
            }
            throw new SystemException($"cannot > value between {a.value} and {b.value}");
        }
        public static ScriptValue operator <(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.Less(right));
            }
            throw new SystemException($"cannot < value between {a.value} and {b.value}");
        }
        public static ScriptValue operator >=(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.LargerEquals(right));
            }
            throw new SystemException($"cannot >= value between {a.value} and {b.value}");
        }
        public static ScriptValue operator <=(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.LessEquals(right));
            }
            throw new SystemException($"cannot <= value between {a.value} and {b.value}");
        }

        public static ScriptValue And(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.And(right));
            }
            throw new SystemException($"cannot && value between {a.value} and {b.value}");
        }

        public static ScriptValue Or(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.Or(right));
            }
            throw new SystemException($"cannot || value between {a.value} and {b.value}");
        }

        public string ToJsonString(int basicIndent = 2, int currentIndent = 0)
        {
            return this.value.ToJsonString(basicIndent, currentIndent);
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public bool Equals(ScriptValue value)
        {
            if (this.value is ScriptObject && value.value is ScriptObject)
            {
                var left = (ScriptObject)this.value;
                var right = (ScriptObject)value.value;
                return ((ScriptBool)left.Equals(right)).Value;
            }
            throw new SystemException($"cannot > value between {this.value} and {value.value}");
        }

        public override bool Equals(object obj)
        {
            if (obj is ScriptValue) return Equals((ScriptValue)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
