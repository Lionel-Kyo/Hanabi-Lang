using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

namespace HanabiLang.Interprets
{
    public class ScriptValue
    {
        private ScriptType value { get; set; }
        public ScriptType Value => value;

        public ScriptValue(ScriptObject obj)
        {
            this.value = obj;
        }

        public ScriptValue(ScriptFns fn)
        {
            this.value = fn;
        }

        public ScriptValue(ScriptClass _class)
        {
            this.value = _class;
        }

        public ScriptValue(byte value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(ushort value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(uint value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(sbyte value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(short value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(int value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(long value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(float value)
        {
            this.value = BasicTypes.Float.Create(value); 
        }
        public ScriptValue(double value)
        {
            this.value = BasicTypes.Float.Create(value);
        }
        public ScriptValue(decimal value)
        {
            this.value = BasicTypes.Decimal.Create(value);
        }
        public ScriptValue(bool value)
        {
            this.value = BasicTypes.Bool.Create(value);
        }
        public ScriptValue(string value)
        {
            this.value = BasicTypes.Str.Create(value);
        }
        public ScriptValue(char value)
        {
            this.value = BasicTypes.Str.Create(value);
        }
        public ScriptValue(StringBuilder value)
        {
            this.value = BasicTypes.Str.Create(value);
        }
        public ScriptValue(List<ScriptValue> value)
        {
            this.value = BasicTypes.List.Create(value);
        }
        public ScriptValue(Dictionary<ScriptValue, ScriptValue> value)
        {
            this.value = BasicTypes.Dict.Create(value);
        }
        public ScriptValue()
        {
            this.value = BasicTypes.NullValue;
        }

        public static ScriptValue Null => new ScriptValue();

        public bool IsFunction => this.value is ScriptFns;
        public bool IsClass => this.value is ScriptClass;
        public bool IsObject => this.value is ScriptObject;
        public bool IsNull => this.value is ScriptObject && ((ScriptObject)this.value).ClassType is ScriptNull;

        public static ScriptValue operator !(ScriptValue a)
        {
            if (a.value is ScriptObject)
            {
                var obj = (ScriptObject)a.value;
                return new ScriptValue(obj.ClassType.Not(obj));
            }
            throw new SystemException("operator ! is not defined");
        }
        public static ScriptValue operator +(ScriptValue a)
        {
            if (a.value is ScriptObject)
            {
                var obj = (ScriptObject)a.value;
                return new ScriptValue(obj.ClassType.Positive(obj));
            }
            throw new SystemException("operator + is not defined");
        }
        public static ScriptValue operator -(ScriptValue a)
        {
            if (a.value is ScriptObject)
            {
                var obj = (ScriptObject)a.value;
                return new ScriptValue(obj.ClassType.Negative(obj));
            }
            throw new SystemException("operator - is not defined");
        }

        public static ScriptValue operator +(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                if (left.ClassType is ScriptStr ||right.ClassType is ScriptStr)
                    return new ScriptValue(left.ToString() + right.ToString());
                return new ScriptValue(left.ClassType.Add(left, right));
            }
            throw new SystemException($"cannot + value between {a.value} and {b.value}");
        }

        public static ScriptValue operator -(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.ClassType.Minus(left, right));
            }
            throw new SystemException($"cannot - value between {a.value} and {b.value}");
        }

        public static ScriptValue operator *(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                if ((left.ClassType is ScriptStr && right.ClassType is ScriptInt) ||
                    (right.ClassType is ScriptStr && left.ClassType is ScriptInt))
                    return new ScriptValue(left.ClassType.Multiply(left, right));

                if ((left.ClassType is ScriptList && right.ClassType is ScriptInt) ||
                    (right.ClassType is ScriptList && left.ClassType is ScriptInt))
                    return new ScriptValue(left.ClassType.Multiply(left, right));

                return new ScriptValue(left.ClassType.Multiply(left, right));
            }
            throw new SystemException($"cannot * value between {a.value} and {b.value}");
        }

        public static ScriptValue operator /(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.ClassType.Divide(left, right));
            }
            throw new SystemException($"cannot / value between {a.value} and {b.value}");
        }

        public static ScriptValue operator %(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.ClassType.Modulo(left, right));
            }
            throw new SystemException($"cannot % value between {a.value} and {b.value}");
        }

        public static ScriptValue operator >(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.ClassType.Larger(left, right));
            }
            throw new SystemException($"cannot > value between {a.value} and {b.value}");
        }
        public static ScriptValue operator <(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.ClassType.Less(left, right));
            }
            throw new SystemException($"cannot < value between {a.value} and {b.value}");
        }
        public static ScriptValue operator >=(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.ClassType.LargerEquals(left, right));
            }
            throw new SystemException($"cannot >= value between {a.value} and {b.value}");
        }
        public static ScriptValue operator <=(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.ClassType.LessEquals(left, right));
            }
            throw new SystemException($"cannot <= value between {a.value} and {b.value}");
        }

        public static ScriptValue And(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.ClassType.And(left, right));
            }
            throw new SystemException($"cannot && value between {a.value} and {b.value}");
        }

        public static ScriptValue Or(ScriptValue a, ScriptValue b)
        {
            if (a.value is ScriptObject && b.value is ScriptObject)
            {
                var left = (ScriptObject)a.value;
                var right = (ScriptObject)b.value;
                return new ScriptValue(left.ClassType.Or(left, right));
            }
            throw new SystemException($"cannot || value between {a.value} and {b.value}");
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public bool Equals(ScriptValue value)
        {
            if (this.value is ScriptObject && value.value is ScriptObject)
            {
                ScriptObject oleft = (ScriptObject)this.value;
                ScriptObject oright = (ScriptObject)value.value;
                return (bool)oleft.ClassType.Equals(oleft, oright).BuildInObject;
            }
            ScriptType left = this.value;
            ScriptType right = value.value;
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is ScriptValue) 
                return this.Equals((ScriptValue)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }
    }
}
