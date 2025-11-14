using HanabiLangLib.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptEnum : ScriptClass
    {
        public ScriptEnum() :
            base("Enum", isStatic: false)
        {
            this.InitializeOperators();

            this.AddFunction(ConstructorName, new List<FnParameter>() { new FnParameter("this"), }, args => throw new NotSupportedException("Enum class can not be called"));

            AddVariable("Value", args =>
            {
                ScriptObject _this = args[0].TryObject;
                return GetEnumValue(_this);
            }, null, false, null);

            this.AddFunction("ToStr", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                return new ScriptValue(this.ToStr(args[0].TryObject));
            });

            //this.AddFunction("FromName", new List<FnParameter>()
            //{
            //    new FnParameter("type", BasicTypes.TypeClass),
            //    new FnParameter("name", BasicTypes.Str)
            //}, args =>
            //{
            //    ScriptClass type = ScriptTypeClass.AsCSharp(args[0].TryObject);
            //    string name = ScriptStr.AsCSharp(args[1].TryObject);
            //    if (!type.Scope.Variables.TryGetValue(name, out var variable))
            //    {
            //        throw new KeyNotFoundException($"{name} is not found in enum {this.Name}");
            //    }
            //    return variable.Value;
            //}, true, AccessibilityLevel.Public);

            //this.AddFunction("FromValue", new List<FnParameter>()
            //{
            //    new FnParameter("type", BasicTypes.TypeClass),
            //    new FnParameter("value")
            //}, args =>
            //{
            //    ScriptClass type = ScriptTypeClass.AsCSharp(args[0].TryObject);
            //    ScriptValue value = args[1];
            //    var found = type.Scope.Variables.Select(kv => ScriptEnum.AsCSharp(kv.Value.Value.TryObject)).FirstOrDefault(v => v.Equals(value));
            //    if (found == null)
            //    {
            //        throw new KeyNotFoundException($"{value} is not found in enum {this.Name}");
            //    }
            //    return found;
            //}, true, AccessibilityLevel.Public);

            //this.AddFunction("ToDict", new List<FnParameter>()
            //{                
            //    new FnParameter("type", BasicTypes.TypeClass),
            //}, args =>
            //{
            //    ScriptClass type = ScriptTypeClass.AsCSharp(args[0].TryObject);
            //    var result = type.Scope.Variables.ToDictionary(kv => new ScriptValue(kv.Key), kv => ScriptEnum.AsCSharp(kv.Value.Value.TryObject));
            //    return new ScriptValue(result);
            //}, true, AccessibilityLevel.Public);
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
            if (_other.IsTypeOrSubOf(BasicTypes.Enum))
            {
                if (_this.ClassType != _other.ClassType)
                    return false;

                return GetEnumValue(_this).Equals(GetEnumValue(_other));
            }
            return false;
        }

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create($"<enum {_this.ClassType.Name}.{GetEnumName(_this)}: {GetEnumValue(_this)}>");

        public static string GetEnumName(ScriptObject _this)
        {
            return AsCSharp(_this).Item1;
        }


        public static ScriptValue GetEnumValue(ScriptObject _this)
        {
            return AsCSharp(_this).Item2;
        }

        /// <returns>Name, Value</returns>
        public static Tuple<string, ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (Tuple<string, ScriptValue>)_this.BuildInObject;
        }
    }
}
