﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptFloat : ScriptClass
    {
        public ScriptFloat() :
            base("float", isStatic: false)
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

        public override ScriptObject Positive(ScriptObject _this)
        {
            return BasicTypes.Decimal.Create(+(double)_this.BuildInObject);
        }
        public override ScriptObject Negative(ScriptObject _this)
        {
            return BasicTypes.Decimal.Create(-(double)_this.BuildInObject);
        }
        public override ScriptObject Add(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Float.Create((double)_this.BuildInObject + (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create((double)_this.BuildInObject + (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create((double)_this.BuildInObject + (double)(decimal)value.BuildInObject);
            }
            return base.Add(_this, value);
        }
        public override ScriptObject Minus(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Float.Create((double)_this.BuildInObject - (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create((double)_this.BuildInObject - (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create((double)_this.BuildInObject - (double)(decimal)value.BuildInObject);
            }
            return base.Minus(_this, value);
        }
        public override ScriptObject Multiply(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Float.Create((double)_this.BuildInObject * (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create((double)_this.BuildInObject * (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create((double)_this.BuildInObject * (double)(decimal)value.BuildInObject);
            }
            return base.Multiply(_this, value);
        }
        public override ScriptObject Divide(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Float.Create((double)_this.BuildInObject / (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create((double)_this.BuildInObject / (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create((double)_this.BuildInObject / (double)(decimal)value.BuildInObject);
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
                return BasicTypes.Float.Create(Modulo((double)_this.BuildInObject, (long)value.BuildInObject));
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Float.Create(Modulo((double)_this.BuildInObject, (double)value.BuildInObject));
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Decimal.Create(Modulo((double)_this.BuildInObject, (double)(decimal)value.BuildInObject));
            }
            return base.Modulo(_this, value);
        }
        public override ScriptObject Larger(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject > (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject > (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject > (double)value.BuildInObject);
            }
            return base.Larger(_this, value);
        }
        public override ScriptObject LargerEquals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject >= (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject >= (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject >= (double)value.BuildInObject);
            }
            return base.LargerEquals(_this, value);
        }
        public override ScriptObject Less(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject < (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject < (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject < (double)(decimal)value.BuildInObject);
            }
            return base.Less(_this, value);
        }
        public override ScriptObject LessEquals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject <= (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject <= (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject <= (double)(decimal)value.BuildInObject);
            }
            return base.LessEquals(_this, value);
        }
        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptInt)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject == (long)value.BuildInObject);
            }
            else if (value.ClassType is ScriptFloat)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject == (double)value.BuildInObject);
            }
            else if (value.ClassType is ScriptDecimal)
            {
                return BasicTypes.Bool.Create((double)_this.BuildInObject == (double)(decimal)value.BuildInObject);
            }
            return base.Equals(_this, value);
        }

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
