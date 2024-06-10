using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    internal class ScriptTypeClass : ScriptClass
    {
        public ScriptTypeClass() : base("Type", false)
        {
            this.AddObjectFn(this.Name, new List<FnParameter>()
            {
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                if (args[1].Value is ScriptClass)
                {
                    _this.BuildInObject = args[1].Value;
                }
                else if (args[1].Value is ScriptObject)
                {
                    _this.BuildInObject = ((ScriptObject)args[1].Value).ClassType;
                }
                else
                {
                    throw new SystemException($"{args[1].Value} is not class or object");
                }

                return ScriptValue.Null;
            });

            this.AddObjectFn("IsSuperOf", new List<FnParameter>()
            {
                new FnParameter("value")
            }, args =>
            {
                ScriptClass valueType = null;
                if (args[1].Value is ScriptObject)
                {
                    if (((ScriptObject)args[1].Value).ClassType == BasicTypes.TypeClass)
                        valueType = (ScriptClass)((ScriptObject)args[1].Value).BuildInObject;
                }
                if (valueType == null)
                    throw new SystemException($"input is not Type object");

                ScriptClass thisType = (ScriptClass)((ScriptObject)args[0].Value).BuildInObject;
                if (thisType == valueType)
                    return new ScriptValue(true);
                return new ScriptValue(valueType.SuperClasses?.Contains(thisType) ?? false);
            });

            this.AddVariable("Value", args => new ScriptValue((ScriptClass)((ScriptObject)args[0].Value).BuildInObject), null, false, null);
        }

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptTypeClass)
            {
                return BasicTypes.Bool.Create(_this.BuildInObject == value.BuildInObject);
            }
            return base.Equals(_this, value);
        }

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create($"<Type: {((ScriptClass)_this.BuildInObject).Name}>");
    }
}
