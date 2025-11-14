using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptTypeClass : ScriptClass
    {
        public ScriptTypeClass() : base("Type", false)
        {
            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
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
                else if (args[1].Value is ScriptFns)
                {
                    _this.BuildInObject = BasicTypes.FunctionClass;
                }
                else
                {
                    throw new SystemException($"{args[1].Value} is not class or object");
                }

                return ScriptValue.Null;
            });

            this.AddFunction("IsSubOf", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value")
            }, args =>
            {
                ScriptObject valueObject = args[1].TryObject;
                ScriptClass valueClass = null;
                if (valueObject != null && valueObject.ClassType == BasicTypes.TypeClass)
                    valueClass = AsCSharp(valueObject);

                if (valueClass == null)
                    throw new SystemException($"input is not Type object");

                ScriptClass thisType = AsCSharp(args[0].TryObject);
                if (thisType == valueClass)
                    return new ScriptValue(true);
                return new ScriptValue(thisType.SuperClasses?.Contains(valueClass) ?? false);
            });

            this.AddFunction("IsSuperOf", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value")
            }, args =>
            {
                ScriptObject valueObject = args[1].TryObject;
                ScriptClass valueClass = null;
                if (valueObject != null && valueObject.ClassType == BasicTypes.TypeClass)
                    valueClass = AsCSharp(valueObject);

                if (valueClass == null)
                    throw new SystemException($"input is not Type object");

                ScriptClass thisType = AsCSharp(args[0].TryObject);
                if (thisType == valueClass)
                    return new ScriptValue(true);
                return new ScriptValue(valueClass.SuperClasses?.Contains(thisType) ?? false);
            });

            this.AddVariable("Value", args => new ScriptValue((ScriptClass)((ScriptObject)args[0].Value).BuildInObject), null, false, null);
        }

        private void InitializeOperators()
        {
            this.AddFunction(OPEARTOR_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                return new ScriptValue(OperatorEquals(args[0], args[1]));
            });
            this.AddFunction(OPEARTOR_NOT_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                return new ScriptValue(!OperatorEquals(args[0], args[1]));
            });
        }

        private bool OperatorEquals(ScriptValue value1, ScriptValue value2)
        {
            ScriptObject _this = value1.TryObject;
            ScriptObject _other = value2.TryObject;
            if (_other.IsTypeOrSubOf(BasicTypes.TypeClass))
            {
                if (object.ReferenceEquals(_this, _other))
                    return true;

                return AsCSharp(_this) == AsCSharp(_other);
            }
            return false;
        }

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create($"<Type: {((ScriptClass)_this.BuildInObject).Name}>");

        public static ScriptClass AsCSharp(ScriptObject _this)
        {
            return (ScriptClass)_this.BuildInObject;
        }
    }
}
