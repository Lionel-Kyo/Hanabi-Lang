using HanabiLangLib.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptBool : ScriptClass
    {
        public ScriptBool() : 
            base("bool", isStatic: false)
        {
            this.InitializeOperators();

            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;

                ScriptObject value = args[1].TryObject;
                if (value == null)
                {
                    throw new ArgumentException("Non-object cannot convert to bool");
                }

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
                else
                {
                    throw new ArgumentException($"{value.ClassType.Name} cannot convert to bool");
                }

                return ScriptValue.Null;
            });
            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(BasicTypes.Str.Create(AsCSharp(_this) ? "true" : "false"));
            });
        }

        private void InitializeOperators()
        {
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
                else if (_other.IsTypeOrSubOf(BasicTypes.Bool))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) & AsCSharp(_other));
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
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) | AsCSharp(_other));
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
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) ^ AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} ^ {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_NOT, new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(BasicTypes.Bool.Create(!AsCSharp(_this)));
            });
            this.AddFunction(OPEARTOR_AND, new List<FnParameter>()
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
                else if (_other.IsTypeOrSubOf(BasicTypes.Bool))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) && AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} && {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_OR, new List<FnParameter>()
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
                else if (_other.IsTypeOrSubOf(BasicTypes.Bool))
                {
                    resultObject = BasicTypes.Bool.Create(AsCSharp(_this) || AsCSharp(_other));
                }

                if (resultObject == null)
                    throw new Exception($"{_this} || {_other} is not defined");
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
                if (_other.IsTypeOrSubOf(BasicTypes.Bool))
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
                if (_other.IsTypeOrSubOf(BasicTypes.Bool))
                    return new ScriptValue(AsCSharp(_this) != AsCSharp(_other));
                return new ScriptValue(true);
            });
        }
        public override ScriptObject Create() => new ScriptObject(this, false);
        public ScriptObject Create(bool value) => new ScriptObject(this, value);

        public static ScriptObject True => BasicTypes.Bool.Create(true);
        public static ScriptObject False => BasicTypes.Bool.Create(false);

        public static bool AsCSharp(ScriptObject _this)
        {
            return (bool)_this.BuildInObject;
        }
    }
}
