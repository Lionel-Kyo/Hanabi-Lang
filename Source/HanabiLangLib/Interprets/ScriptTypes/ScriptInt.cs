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
            this.InitializeOperators();

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

            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(BasicTypes.Str.Create(AsCSharp(_this).ToString()));
            });
        }

        private void InitializeOperators()
        {
            this.AddFunction(OPEARTOR_BIT_NOT, new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(BasicTypes.Int.Create(~AsCSharp(_this)));
            });
            this.AddFunction(OPEARTOR_BIT_AND, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Int.Create(AsCSharp(_this) & AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} & {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_BIT_OR, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Int.Create(AsCSharp(_this) | AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} | {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_BIT_XOR, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Int.Create(AsCSharp(_this) ^ AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} ^ {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_BIT_LEFT_SHIFT, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Int.Create(AsCSharp(_this) << ValidateToInt32(AsCSharp(_other)));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} ^ {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_BIT_RIGHT_SHIFT, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Int.Create(AsCSharp(_this) >> ValidateToInt32(AsCSharp(_other)));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} ^ {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_POSITIVE, new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(BasicTypes.Int.Create(+AsCSharp(_this)));
            });
            this.AddFunction(OPEARTOR_NEGATIVE, new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(BasicTypes.Int.Create(-AsCSharp(_this)));
            });
            this.AddFunction(OPEARTOR_ADD, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Int.Create(AsCSharp(_this) + AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create((double)AsCSharp(_this) + ScriptFloat.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create((decimal)AsCSharp(_this) + ScriptDecimal.AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} + {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_MINUS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Int.Create(AsCSharp(_this) - AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create((double)AsCSharp(_this) - ScriptFloat.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create((decimal)AsCSharp(_this) - ScriptDecimal.AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} - {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_MULTIPLY, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Int.Create(AsCSharp(_this) * AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create((double)AsCSharp(_this) * ScriptFloat.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create((decimal)AsCSharp(_this) * ScriptDecimal.AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} * {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_DIVIDE, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Float.Create((double)AsCSharp(_this) / AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create((double)AsCSharp(_this) / ScriptFloat.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create((decimal)AsCSharp(_this) / ScriptDecimal.AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} / {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_MUDULO, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Int.Create(Modulo(AsCSharp(_this), AsCSharp(_other)));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create(Modulo((double)AsCSharp(_this), ScriptFloat.AsCSharp(_other)));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create(Modulo((decimal)AsCSharp(_this), ScriptDecimal.AsCSharp(_other)));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} % {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_LESS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) < AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) < ScriptFloat.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) < ScriptDecimal.AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} < {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_LESS_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) <= AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) <= ScriptFloat.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) <= ScriptDecimal.AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} <= {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_LARGER, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) > AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) > ScriptFloat.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) > ScriptDecimal.AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} > {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_LARGER_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                ScriptObject resultObject = null;
                if (_other == null)
                {

                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Int))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) >= AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) >= ScriptFloat.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) >= ScriptDecimal.AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} > {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                if (_other.IsTypeOrSubOf(BasicTypes.Int))
                    return new ScriptValue(AsCSharp(_this) == AsCSharp(_other));
                return new ScriptValue(false);
            });
            this.AddFunction(OPEARTOR_NOT_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                if (_other.IsTypeOrSubOf(BasicTypes.Int))
                    return new ScriptValue(AsCSharp(_this) != AsCSharp(_other));
                return new ScriptValue(true);
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

        public static long Modulo(long k, long n) => ((k %= n) < 0) ? k + n : k;
        public static double Modulo(double k, double n) => ((k %= n) < 0) ? k + n : k;
        public static decimal Modulo(decimal k, decimal n) => ((k %= n) < 0) ? k + n : k;

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
