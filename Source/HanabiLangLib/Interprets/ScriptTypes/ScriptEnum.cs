using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptEnum : ScriptClass
    {
        public ScriptEnum() :
            base("Enum", isStatic: false)
        {
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

            this.AddFunction("==", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("right")
            }, args =>
            {
                ScriptObject left = args[0].TryObject;
                ScriptObject right = args[1].TryObject;
                return new ScriptValue(this.Equals(left, right));
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

        public override ScriptObject Equals(ScriptObject left, ScriptObject right)
        {
            if (!left.IsTypeOrSubOf(BasicTypes.Enum))
                return ScriptBool.False;

            if (right == null || (left.ClassType != right.ClassType))
                return ScriptBool.False;

            return BasicTypes.Bool.Create(GetEnumValue(left).Equals(GetEnumValue(right)));
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
