using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptDecimal : ScriptObject
    {
        public static ScriptClass CreateBuildInClass()
        {
            var newScrope = new ScriptScope(ScopeType.Class);
            return new ScriptClass("decimal", null, new List<string>(),
                newScrope, false, () => new ScriptDecimal());
        }
        public decimal Value { get; private set; }

        public ScriptDecimal() :
            base(CreateBuildInClass())
        {
            this.Value = 0;
            this.AddObjectFn(this.ObjectClass.Name, args =>
            {
                if (args.Count == 1)
                {
                    ScriptValue value = args[0];
                    if (value.Value is ScriptDecimal)
                    {
                        this.Value = ((ScriptDecimal)value.Value).Value;
                    }
                    else if (value.Value is ScriptFloat)
                    {
                        this.Value = ((ScriptInt)value.Value).Value;
                    }
                    else if (value.Value is ScriptInt)
                    {
                        this.Value = ((ScriptInt)value.Value).Value;
                    }
                    else if (value.Value is ScriptStr)
                    {
                        this.Value = long.Parse(((ScriptStr)value.Value).Value);
                    }
                }
                return ScriptValue.Null;
            });
        }
        public ScriptDecimal(decimal value) : this()
        {
            this.Value = value;
        }
        public ScriptDecimal(float value) : this()
        {
            this.Value = (decimal)value;
        }
        public ScriptDecimal(double value) : this()
        {
            this.Value = (decimal)value;
        }

        public override ScriptObject Positive()
        {
            return new ScriptDecimal(+this.Value);
        }
        public override ScriptObject Negative()
        {
            return new ScriptDecimal(-this.Value);
        }
        public override ScriptObject Add(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptDecimal(this.Value + obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptDecimal(this.Value + (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal(this.Value + obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Minus(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptDecimal(this.Value - obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptDecimal(this.Value - (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal(this.Value - obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Multiply(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptDecimal(this.Value * obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptDecimal(this.Value * (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal(this.Value * obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Divide(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptDecimal(this.Value / obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptDecimal(this.Value / (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal(this.Value / obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Modulo(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptDecimal(this.Value % obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptDecimal(this.Value % (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal(this.Value % obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Larger(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptBool(this.Value > obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptBool(this.Value > (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool(this.Value > obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject LargerEquals(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptBool(this.Value >= obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptBool(this.Value >= (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool(this.Value >= obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Less(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptBool(this.Value < obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptBool(this.Value < (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool(this.Value < obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject LessEquals(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptBool(this.Value <= obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptBool(this.Value <= (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool(this.Value <= obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Equals(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptBool(this.Value == obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptBool(this.Value == (decimal)obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool(this.Value == obj.Value);
            }
            return base.Add(value);
        }

        public override ScriptStr ToStr() => new ScriptStr(this.ToString());

        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0) => this.Value.ToString();
        public override string ToString() => this.Value.ToString();

        public override ScriptObject Copy()
        {
            return new ScriptDecimal(this.Value);
        }

        /*public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }*/
    }
}
