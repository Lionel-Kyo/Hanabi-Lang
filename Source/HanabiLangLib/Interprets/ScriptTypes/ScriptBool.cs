using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptBool : ScriptClass
    {
        public ScriptBool() : 
            base("bool", isStatic: false)
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
                    _this.BuildInObject = Convert.ToBoolean(ScriptInt.AsCSharp(value));
                }
                else if (value.ClassType is ScriptDecimal)
                {
                    _this.BuildInObject = Convert.ToBoolean(ScriptDecimal.AsCSharp(value));
                }
                else if (value.ClassType is ScriptFloat)
                {
                    _this.BuildInObject = Convert.ToBoolean(ScriptFloat.AsCSharp(value));
                }
                else if (value.ClassType is ScriptStr)
                {
                    _this.BuildInObject = Convert.ToBoolean(ScriptStr.AsCSharp(value));
                }

                return ScriptValue.Null;
            });
        }
        public override ScriptObject Create() => new ScriptObject(this, false);
        public ScriptObject Create(bool value) => new ScriptObject(this, value);

        public static ScriptObject True => BasicTypes.Bool.Create(true);
        public static ScriptObject False => BasicTypes.Bool.Create(false);

        public override ScriptObject Not(ScriptObject _this)
        {
            return BasicTypes.Bool.Create(!(bool)_this.BuildInObject);
        }

        public override ScriptObject And(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptBool)
            {
                return BasicTypes.Bool.Create((bool)_this.BuildInObject && (bool)value.BuildInObject);
            }
            return base.And(_this, value);
        }
        public override ScriptObject Or(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptBool)
            {
                return BasicTypes.Bool.Create((bool)_this.BuildInObject || (bool)value.BuildInObject);
            }
            return base.Or(_this, value);
        }

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptBool)
            {
                return BasicTypes.Bool.Create((bool)_this.BuildInObject == (bool)value.BuildInObject);
            }
            return ScriptBool.False;
        }

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create((bool)_this.BuildInObject ? "true" : "false");
        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            return (string)this.ToStr(_this).BuildInObject;
        }

        public static bool AsCSharp(ScriptObject _this)
        {
            return (bool)_this.BuildInObject;
        }
    }
}
