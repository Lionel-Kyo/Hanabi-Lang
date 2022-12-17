using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptFloat : ScriptObject
    {
        public static ScriptClass CreateBuildInClass()
        {
            var newScrope = new ScriptScope(ScopeType.Class);
            return new ScriptClass("float", null, new List<string>(),
                newScrope, false, () => new ScriptFloat());
        }
        public double Value { get; private set; }

        public ScriptFloat() :
            base(CreateBuildInClass())
        {
            this.Value = 0;
            this.AddObjectFn(this.ObjectClass.Name, args =>
            {
                if (args.Count == 1)
                {
                    ScriptValue value = args[0];
                    if (value.Value is ScriptFloat)
                    {
                        this.Value = ((ScriptInt)value.Value).Value;
                    }
                    else if(value.Value is ScriptDecimal)
                    {
                        this.Value = (double)((ScriptDecimal)value.Value).Value;
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
        public ScriptFloat(float value) : this()
        {
            this.Value = value;
        }
        public ScriptFloat(double value) : this()
        {
            this.Value = value;
        }

        public override ScriptObject Positive()
        {
            return new ScriptFloat(+this.Value);
        }
        public override ScriptObject Negative()
        {
            return new ScriptFloat(-this.Value);
        }
        public override ScriptObject Add(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptFloat(this.Value + obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptFloat(this.Value + obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal((decimal)this.Value + obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Minus(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptFloat(this.Value - obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptFloat(this.Value - obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal((decimal)this.Value - obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Multiply(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptFloat(this.Value * obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptFloat(this.Value * obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal((decimal)this.Value * obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Divide(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptFloat(this.Value / obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptFloat(this.Value / obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal((decimal)this.Value / obj.Value);
            }
            return base.Add(value);
        }
        public override ScriptObject Modulo(ScriptObject value)
        {
            if (value is ScriptInt)
            {
                ScriptInt obj = (ScriptInt)value;
                return new ScriptFloat(this.Value % obj.Value);
            }
            else if (value is ScriptFloat)
            {
                ScriptFloat obj = (ScriptFloat)value;
                return new ScriptFloat(this.Value % obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptDecimal((decimal)this.Value % obj.Value);
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
                return new ScriptBool(this.Value > obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool((decimal)this.Value > obj.Value);
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
                return new ScriptBool(this.Value >= obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool((decimal)this.Value >= obj.Value);
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
                return new ScriptBool(this.Value < obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool((decimal)this.Value < obj.Value);
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
                return new ScriptBool(this.Value <= obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool((decimal)this.Value <= obj.Value);
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
                return new ScriptBool(this.Value == obj.Value);
            }
            else if (value is ScriptDecimal)
            {
                ScriptDecimal obj = (ScriptDecimal)value;
                return new ScriptBool((decimal)this.Value == obj.Value);
            }
            return base.Add(value);
        }

        public override ScriptStr ToStr() => new ScriptStr(this.ToString());
        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0) => this.Value.ToString();
        public override string ToString() => this.Value.ToString();

        public override ScriptObject Copy()
        {
            return new ScriptFloat(this.Value);
        }

        /*public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }*/
    }
}
