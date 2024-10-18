using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptInt : ScriptClass
    {
        public ScriptInt() :
            base("int", isStatic: false)
        {
            this.AddFunction(this.Name, new List<FnParameter>()
            {
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptObject value = (ScriptObject)args[1].Value;
                if (value.ClassType is ScriptInt)
                {
                    _this.BuildInObject = value.BuildInObject;
                }
                else if (value.ClassType is ScriptDecimal)
                {
                    _this.BuildInObject = (long)(decimal)value.BuildInObject;
                }
                else if (value.ClassType is ScriptFloat)
                {
                    _this.BuildInObject = (long)(double)value.BuildInObject;
                }
                else if (value.ClassType is ScriptStr)
                {
                    _this.BuildInObject = long.Parse((string)value.BuildInObject);
                }
                return ScriptValue.Null;
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, (long)0);
        public ScriptObject Create(long value) => new ScriptObject(this, value);
        public ScriptObject Create(byte value) => this.Create((long)value);
        public ScriptObject Create(short value) => this.Create((long)value);
        public ScriptObject Create(int value) => this.Create((long)value);
        public ScriptObject Create(sbyte value) => this.Create((long)value);
        public ScriptObject Create(ushort value) => this.Create((long)value);
        public ScriptObject Create(uint value) => this.Create((long)value);
        public ScriptObject Create(string value) => this.Create(long.Parse(value));
        public ScriptObject Create(StringBuilder value) => this.Create(long.Parse(value.ToString()));

        public override ScriptObject Positive(ScriptObject _this)
        {
            return BasicTypes.Int.Create(+(long)_this.BuildInObject);
        }
        public override ScriptObject Negative(ScriptObject _this)
        {
            return BasicTypes.Int.Create(-(long)_this.BuildInObject);
        }
        public override ScriptObject Add(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Int.Create((long)_this.BuildInObject + (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create((double)((long)_this.BuildInObject + (double)value.BuildInObject));
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create((decimal)((long)_this.BuildInObject + (decimal)value.BuildInObject));
            }
            return base.Add(_this, value);
        }
        public override ScriptObject Minus(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Int.Create((long)_this.BuildInObject - (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create((double)((long)_this.BuildInObject - (double)value.BuildInObject));
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create((decimal)((long)_this.BuildInObject - (decimal)value.BuildInObject));
            }
            return base.Minus(_this, value);
        }
        public override ScriptObject Multiply(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Int.Create((long)_this.BuildInObject * (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create((double)((long)_this.BuildInObject * (double)value.BuildInObject));
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create((decimal)((long)_this.BuildInObject * (decimal)value.BuildInObject));
            }
            return base.Multiply(_this, value);
        }
        public override ScriptObject Divide(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Float.Create((double)(long)_this.BuildInObject / (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create((double)((long)_this.BuildInObject / (double)value.BuildInObject));
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create((decimal)((long)_this.BuildInObject / (decimal)value.BuildInObject));
            }
            return base.Divide(_this, value);
        }

        public static long Modulo(long k, long n) => ((k %= n) < 0) ? k + n : k;
        public static double Modulo(double k, double n) => ((k %= n) < 0) ? k + n : k;
        public static decimal Modulo(decimal k, decimal n) => ((k %= n) < 0) ? k + n : k;

        public override ScriptObject Modulo(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Int.Create(Modulo((long)_this.BuildInObject, (long)value.BuildInObject));
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create(Modulo((double)(long)_this.BuildInObject, (double)value.BuildInObject));
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create(Modulo((decimal)(long)_this.BuildInObject, (decimal)value.BuildInObject));
            }
            return base.Modulo(_this, value);
        }
        public override ScriptObject Larger(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject > (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject > (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject > (decimal)value.BuildInObject);
            }
            return base.Larger(_this, value);
        }
        public override ScriptObject LargerEquals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject >= (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject >= (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject >= (decimal)value.BuildInObject);
            }
            return base.LargerEquals(_this, value);
        }
        public override ScriptObject Less(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject < (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject < (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject < (decimal)value.BuildInObject);
            }
            return base.Less(_this, value);
        }
        public override ScriptObject LessEquals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject <= (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject <= (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject <= (decimal)value.BuildInObject);
            }
            return base.LessEquals(_this, value);
        }
        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject == (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject == (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((long)_this.BuildInObject == (decimal)value.BuildInObject);
            }
            return base.Equals(_this, value);
        }

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create(_this.BuildInObject.ToString());
        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            return (string)this.ToStr(_this).BuildInObject;
        }

        public static long AsCSharp(ScriptObject _this)
        {
            return (long)_this.BuildInObject;
        }
    }
}
