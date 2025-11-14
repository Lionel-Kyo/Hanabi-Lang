using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptFloat : ScriptClass
    {
        public ScriptFloat() :
            base("float", isStatic: false)
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
                    _this.BuildInObject = (double)(long)value.BuildInObject;
                }
                else if (value.ClassType is ScriptDecimal)
                {
                    _this.BuildInObject = (double)(decimal)value.BuildInObject;
                }
                else if (value.ClassType is ScriptFloat)
                {
                    _this.BuildInObject = (double)value.BuildInObject;
                }
                else if (value.ClassType is ScriptStr)
                {
                    _this.BuildInObject = double.Parse((string)value.BuildInObject);
                }
                return ScriptValue.Null;
            });

            this.AddFunction("CompareTo", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value"),
            }, args =>
            {
                double _this = AsCSharp(args[0].TryObject);
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

        private void InitializeOperators()
        {
            this.AddFunction(OPEARTOR_POSITIVE, new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(BasicTypes.Float.Create(+AsCSharp(_this)));
            });
            this.AddFunction(OPEARTOR_NEGATIVE, new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(BasicTypes.Float.Create(-AsCSharp(_this)));
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
                    resultObject = BasicTypes.Float.Create(AsCSharp(_this) + (double)ScriptInt.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create(AsCSharp(_this) + AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create(AsCSharp(_this) + (double)ScriptDecimal.AsCSharp(_other));
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
                    resultObject = BasicTypes.Float.Create(AsCSharp(_this) - (double)ScriptInt.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create(AsCSharp(_this) - AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create(AsCSharp(_this) - (double)ScriptDecimal.AsCSharp(_other));
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
                    resultObject = BasicTypes.Float.Create(AsCSharp(_this) * (double)ScriptInt.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create(AsCSharp(_this) * AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create(AsCSharp(_this) * (double)ScriptDecimal.AsCSharp(_other));
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
                    resultObject = BasicTypes.Float.Create(AsCSharp(_this) / (double)ScriptInt.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create(AsCSharp(_this) / AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create(AsCSharp(_this) / (double)ScriptDecimal.AsCSharp(_other));
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
                    resultObject = BasicTypes.Float.Create(Modulo(AsCSharp(_this), (double)ScriptInt.AsCSharp(_other)));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Float.Create(Modulo(AsCSharp(_this), AsCSharp(_other)));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Decimal.Create(Modulo(AsCSharp(_this), AsCSharp(_other)));
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
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) < ScriptInt.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) < AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) < (double)ScriptDecimal.AsCSharp(_other));
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
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) <= ScriptInt.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) <= AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) <= (double)ScriptDecimal.AsCSharp(_other));
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
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) > ScriptInt.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) > AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) > (double)ScriptDecimal.AsCSharp(_other));
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
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) >= ScriptInt.AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Float))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) >= AsCSharp(_other));
                }
                else if (_other.IsTypeOrSubOf(BasicTypes.Decimal))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) >= (double)ScriptDecimal.AsCSharp(_other));
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
                if (_other.IsTypeOrSubOf(BasicTypes.Float))
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
                if (_other.IsTypeOrSubOf(BasicTypes.Float))
                    return new ScriptValue(AsCSharp(_this) != AsCSharp(_other));
                return new ScriptValue(true);
            });
        }
        public override ScriptObject Create() => new ScriptObject(this, (double)0);
        public ScriptObject Create(double value) => new ScriptObject(this, value);
        public ScriptObject Create(decimal value) => this.Create(value);
        public ScriptObject Create(float value) => this.Create((double)value);
        public ScriptObject Create(byte value) => this.Create((double)value);
        public ScriptObject Create(short value) => this.Create((double)value);
        public ScriptObject Create(int value) => this.Create((double)value);
        public ScriptObject Create(sbyte value) => this.Create((double)value);
        public ScriptObject Create(ushort value) => this.Create((double)value);
        public ScriptObject Create(uint value) => this.Create((double)value);
        public ScriptObject Create(string value) => this.Create(double.Parse(value));
        public ScriptObject Create(StringBuilder value) => this.Create(double.Parse(value.ToString()));

        public static long Modulo(long k, long n) => ((k %= n) < 0) ? k + n : k;
        public static double Modulo(double k, double n) => ((k %= n) < 0) ? k + n : k;
        public static decimal Modulo(decimal k, decimal n) => ((k %= n) < 0) ? k + n : k;

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create(_this.BuildInObject.ToString());
        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            return (string)this.ToStr(_this).BuildInObject;
        }

        public static double AsCSharp(ScriptObject _this)
        {
            return (double)_this.BuildInObject;
        }
    }
}
