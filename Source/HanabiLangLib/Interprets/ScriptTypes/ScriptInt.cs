using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptInt : ScriptClass
    {
        public ScriptInt() :
            base("int", isStatic: false)
        {
            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptObject value = (ScriptObject)args[1].Value;
                if (value.ClassType is ScriptInt)
                {
                    _this.BuildInObject = AsCSharp(value);
                }
                else if (value.ClassType is ScriptDecimal)
                {
                    _this.BuildInObject = (long)ScriptDecimal.AsCSharp(value);
                }
                else if (value.ClassType is ScriptFloat)
                {
                    _this.BuildInObject = (long)ScriptFloat.AsCSharp(value);
                }
                else if (value.ClassType is ScriptStr)
                {
                    _this.BuildInObject = long.Parse(ScriptStr.AsCSharp(value));
                }
                return ScriptValue.Null;
            });

            this.AddFunction("CompareTo", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value"),
            }, args =>
            {
                long _this = AsCSharp(args[0].TryObject);
                ScriptObject value = (ScriptObject)args[1].Value;
                if (value.ClassType is ScriptInt)
                {
                    return new ScriptValue(_this.CompareTo(AsCSharp(value)));
                }
                else if (value.ClassType is ScriptDecimal)
                {
                    return new ScriptValue(_this.CompareTo(ScriptDecimal.AsCSharp(value)));
                }
                else if (value.ClassType is ScriptFloat)
                {
                    return new ScriptValue(_this.CompareTo(ScriptFloat.AsCSharp(value)));
                }
                return new ScriptValue(0);
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

        public static ulong ValidateToUInt64(long i)
        {
            if (i < 0)
                throw new ArgumentOutOfRangeException($"{i} < 0");
            return (ulong)i;
        }

        public static int ValidateToInt32(long i)
        {
            if (i < int.MinValue)
                throw new ArgumentOutOfRangeException($"{i} < {int.MinValue}");
            else if (i > int.MaxValue)
                throw new ArgumentOutOfRangeException($"{i} > {int.MaxValue}");
            return (int)i;
        }

        public static uint ValidateToUInt32(long i)
        {
            if (i < uint.MinValue)
                throw new ArgumentOutOfRangeException($"{i} < {uint.MinValue}");
            else if (i > uint.MaxValue)
                throw new ArgumentOutOfRangeException($"{i} > {uint.MaxValue}");
            return (uint)i;
        }

        public static short ValidateToInt16(long i)
        {
            if (i < short.MinValue)
                throw new ArgumentOutOfRangeException($"{i} < {short.MinValue}");
            else if (i > short.MaxValue)
                throw new ArgumentOutOfRangeException($"{i} > {short.MaxValue}");
            return (short)i;
        }

        public static ushort ValidateToUInt16(long i)
        {
            if (i < ushort.MinValue)
                throw new ArgumentOutOfRangeException($"{i} < {ushort.MinValue}");
            else if (i > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"{i} > {ushort.MaxValue}");
            return (ushort)i;
        }

        public static byte ValidateToInt8(long i)
        {
            if (i < byte.MinValue)
                throw new ArgumentOutOfRangeException($"{i} < {byte.MinValue}");
            else if (i > byte.MaxValue)
                throw new ArgumentOutOfRangeException($"{i} > {byte.MaxValue}");
            return (byte)i;
        }

        public static sbyte ValidateToUInt8(long i)
        {
            if (i < sbyte.MinValue)
                throw new ArgumentOutOfRangeException($"{i} < {sbyte.MinValue}");
            else if (i > sbyte.MaxValue)
                throw new ArgumentOutOfRangeException($"{i} > {sbyte.MaxValue}");
            return (sbyte)i;
        }

        public static long AsCSharp(ScriptObject _this)
        {
            return (long)_this.BuildInObject;
        }
    }
}
