using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLangLib.Interprets.ScriptTypes;
using HanabiLangLib.Parses.Nodes;
using System.Xml.Linq;

namespace HanabiLangLib.Interprets
{
    public class ScriptValue : IComparable<ScriptValue>
    {
        private ScriptType value { get; set; }
        public ScriptType Value => value;

        public ScriptValue(ScriptObject obj)
        {
            this.value = obj;
        }

        public ScriptValue(ScriptFns fn)
        {
            this.value = fn;
        }

        public ScriptValue(ScriptClass _class)
        {
            this.value = _class;
        }

        public ScriptValue(byte value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(ushort value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(uint value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(sbyte value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(short value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(int value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(long value)
        {
            this.value = BasicTypes.Int.Create(value);
        }
        public ScriptValue(float value)
        {
            this.value = BasicTypes.Float.Create(value);
        }
        public ScriptValue(double value)
        {
            this.value = BasicTypes.Float.Create(value);
        }
        public ScriptValue(decimal value)
        {
            this.value = BasicTypes.Decimal.Create(value);
        }
        public ScriptValue(bool value)
        {
            this.value = BasicTypes.Bool.Create(value);
        }

        public ScriptValue(byte? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Int.Create(value.Value);
        }
        public ScriptValue(ushort? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Int.Create(value.Value);
        }
        public ScriptValue(uint? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Int.Create(value.Value);
        }
        public ScriptValue(sbyte? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Int.Create(value.Value);
        }
        public ScriptValue(short? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Int.Create(value.Value);
        }
        public ScriptValue(int? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Int.Create(value.Value);
        }
        public ScriptValue(long? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Int.Create(value.Value);
        }
        public ScriptValue(float? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Float.Create(value.Value);
        }
        public ScriptValue(double? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Float.Create(value.Value);
        }
        public ScriptValue(decimal? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Decimal.Create(value.Value);
        }
        public ScriptValue(bool? value)
        {
            if (!value.HasValue)
                this.value = BasicTypes.NullValue;
            else
                this.value = BasicTypes.Bool.Create(value.Value);
        }

        public ScriptValue(string value)
        {
            this.value = BasicTypes.Str.Create(value);
        }
        public ScriptValue(char value)
        {
            this.value = BasicTypes.Str.Create(value);
        }
        public ScriptValue(StringBuilder value)
        {
            this.value = BasicTypes.Str.Create(value);
        }
        public ScriptValue(List<ScriptValue> value)
        {
            this.value = BasicTypes.List.Create(value);
        }
        public ScriptValue(Dictionary<ScriptValue, ScriptValue> value)
        {
            this.value = BasicTypes.Dict.Create(value);
        }

        public ScriptValue(DefinedTypes value)
        {
            this.value = value;
        }

        public ScriptValue(BreakType breakType)
        {
            this.value = breakType;
        }

        public ScriptValue(ContinueType continueType)
        {
            this.value = continueType;
        }

        public ScriptValue()
        {
            this.value = BasicTypes.NullValue;
        }

        public static ScriptValue Null => new ScriptValue();
        public static ScriptValue Break => new ScriptValue(new BreakType());
        public static ScriptValue Continue => new ScriptValue(new ContinueType());

        public bool IsFunction => this.value is ScriptFns;
        public ScriptFns TryFunction => this.value is ScriptFns ? (ScriptFns)this.value : null;
        public bool IsClass => this.value is ScriptClass;
        public ScriptClass TryClass => this.value is ScriptClass ? (ScriptClass)this.value : null;
        public bool IsObject => this.value is ScriptObject;
        public ScriptObject TryObject => this.value is ScriptObject ? (ScriptObject)this.value : null;
        public bool IsNull => this.value is ScriptObject && ((ScriptObject)this.value).ClassType is ScriptNull;
        public bool IsUnzipable => this.value is ScriptObject && ((ScriptObject)this.value).ClassType is ScriptUnzipable;
        public bool IsCatchedExpresion => this.value is ScriptObject && ((ScriptObject)this.value).ClassType is ScriptCatchedExpression;
        public IEnumerable<ScriptValue> TryUnzipable => IsUnzipable ? ScriptUnzipable.AsCSharp(this.TryObject) : null;
        public bool IsBreak => this.value is BreakType;
        public bool IsContinue => this.value is ContinueType;
        public bool IsDefinedTypes => this.value is DefinedTypes;

        public bool IsClassTypeOf(ScriptClass type)
        {
            ScriptObject obj = this.TryObject;
            return obj != null && obj.ClassType == type;
        }

        private static ScriptValue OperatorOneValue(ScriptValue value, string fnName, string operatorSymbol)
        {
            var _class = value.TryClass;
            var _object = value.TryObject;
            if (_object != null)
            {
                if (_object.ClassType.TryGetValue(fnName, out var scriptVariable) && scriptVariable.Value != null && scriptVariable.Value.IsFunction)
                {
                    var fns = scriptVariable.Value.TryFunction;
                    return fns.Call(_object);
                }
                else
                {
                    throw new Exception($"operator '{operatorSymbol}value' is not avalible for {_object.ClassType.Name}.");
                }
            }
            else if (_class != null)
            {
                if (_class.TryGetValue(fnName, out var scriptVariable) && scriptVariable.Value != null && scriptVariable.Value.IsFunction)
                {
                    var fns = scriptVariable.Value.TryFunction;
                    return fns.Call(null, value);
                }
                else
                {
                    throw new Exception($"operator '{operatorSymbol}value' is not avalible for {_class.Name}.");
                }
            }
            else
            {
                throw new Exception($"operator '{operatorSymbol}value' is only avalible for class and object.");
            }
        }

        private static ScriptValue OperatorTwoValues(ScriptValue value1, ScriptValue value2, string fnName, string operatorSymbol)
        {
            var _class = value1.TryClass;
            var _object = value1.TryObject;
            if (_object != null)
            {
                if (_object.ClassType.TryGetValue(fnName, out var scriptVariable) && scriptVariable.Value != null && scriptVariable.Value.IsFunction)
                {
                    var fns = scriptVariable.Value.TryFunction;
                    return fns.Call(_object, value2);
                }
                else
                {
                    throw new Exception($"operator 'value1 {operatorSymbol} value2' is not avalible for {_object.ClassType.Name}.");
                }
            }
            else if (_class != null)
            {
                if (_class.TryGetValue(fnName, out var scriptVariable) && scriptVariable.Value != null && scriptVariable.Value.IsFunction)
                {
                    var fns = scriptVariable.Value.TryFunction;
                    return fns.Call(null, value1, value2);
                }
                else
                {
                    throw new Exception($"operator 'value1 {operatorSymbol} value2' is not avalible for {_class.Name}.");
                }
            }
            else
            {
                throw new Exception($"operator 'value1 {operatorSymbol} value2' is only avalible for class and object.");
            }
        }

        public static ScriptValue BitNot(ScriptValue value) => OperatorOneValue(value, ScriptClass.OPEARTOR_BIT_NOT, "~");
        public static ScriptValue BitAnd(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_BIT_AND, "&");
        public static ScriptValue BitOr(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_BIT_OR, "|");
        public static ScriptValue BitXor(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_BIT_XOR, "^");
        public static ScriptValue BitLeftShift(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_BIT_LEFT_SHIFT, "<<");
        public static ScriptValue BitRightShift(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_BIT_RIGHT_SHIFT, ">>");
        public static ScriptValue Not(ScriptValue value) => OperatorOneValue(value, ScriptClass.OPEARTOR_NOT, "!");
        public static ScriptValue And(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_AND, "&&");
        public static ScriptValue Or(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_OR, "||");
        public static ScriptValue Positive(ScriptValue value) => OperatorOneValue(value, ScriptClass.OPEARTOR_POSITIVE, "+");
        public static ScriptValue Negative(ScriptValue value) => OperatorOneValue(value, ScriptClass.OPEARTOR_NEGATIVE, "-");
        public static ScriptValue Add(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_ADD, "+");
        public static ScriptValue Minus(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_MINUS, "-");
        public static ScriptValue Multiply(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_MULTIPLY, "*");
        public static ScriptValue Divide(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_DIVIDE, "/");
        public static ScriptValue Mudulo(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_MUDULO, "%");
        public static ScriptValue Larger(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_LARGER, ">");
        public static ScriptValue LargerEquals(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_LARGER_EQUALS, ">=");
        public static ScriptValue Less(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_LESS, "<");
        public static ScriptValue LessEquals(ScriptValue value1, ScriptValue value2) => OperatorTwoValues(value1, value2, ScriptClass.OPEARTOR_LESS_EQUALS, "<=");
        public static ScriptValue Equals(ScriptValue value1, ScriptValue value2)
        {
            var _object = value1.TryObject;
            if (_object != null)
            {
                if (_object.ClassType.TryGetValue(ScriptClass.OPEARTOR_EQUALS, out var scriptVariable) && scriptVariable.Value != null && scriptVariable.Value.IsFunction)
                {
                    var fns = scriptVariable.Value.TryFunction;
                    var result = fns.Call(_object, value2);
                    var resultObject = result.TryObject;
                    if (resultObject == null || !resultObject.IsTypeOrSubOf(BasicTypes.Bool))
                        throw new Exception($"{ScriptClass.OPEARTOR_EQUALS} must return a bool.");
                    return result;
                }
                else
                {
                    throw new Exception($"operator 'value1 == value2' is not avalible for {_object.ClassType.Name}.");
                }
            }
            return new ScriptValue(object.ReferenceEquals(value1.Value, value2.Value));
        }
        public static ScriptValue NotEquals(ScriptValue value1, ScriptValue value2)
        {
            var _object = value1.TryObject;
            if (_object != null)
            {
                if (_object.ClassType.TryGetValue(ScriptClass.OPEARTOR_NOT_EQUALS, out var scriptVariable1) && scriptVariable1.Value != null && scriptVariable1.Value.IsFunction)
                {
                    var fns = scriptVariable1.Value.TryFunction;
                    var result = fns.Call(_object, value2);
                    var resultObject = result.TryObject;
                    if (resultObject == null || !resultObject.IsTypeOrSubOf(BasicTypes.Bool))
                        throw new Exception($"{ScriptClass.OPEARTOR_NOT_EQUALS} must return a bool.");
                    return result;
                }
                else if (_object.ClassType.TryGetValue(ScriptClass.OPEARTOR_EQUALS, out var scriptVariable2) && scriptVariable2.Value != null && scriptVariable2.Value.IsFunction)
                {
                    var fns = scriptVariable2.Value.TryFunction;
                    var result = fns.Call(_object, value2);
                    var resultObject = result.TryObject;
                    if (resultObject == null || !resultObject.IsTypeOrSubOf(BasicTypes.Bool))
                        throw new Exception($"{ScriptClass.OPEARTOR_EQUALS} must return a bool.");
                    return new ScriptValue(!ScriptBool.AsCSharp(resultObject));
                }
                else
                {
                    throw new Exception($"operator 'value1 != value2' is not avalible for {_object.ClassType.Name}.");
                }
            }
            return new ScriptValue(!object.ReferenceEquals(value1.Value, value2.Value));
        }

        public static ScriptValue OperatorSingleUnzip(ScriptValue a)
        {
            var obj = a.TryObject;
            if (obj != null)
            {
                if (!ScriptIterable.TryGetIterable(obj, out var iter))
                    throw new SystemException($"Cannot unzip {((ScriptObject)a.value).ClassType.Name}");

                return new ScriptValue(BasicTypes.Unzipable.Create(iter));
            }
            throw new SystemException("operator * is not defined");
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public bool Equals(ScriptValue value)
        {
            return ScriptBool.AsCSharp(Equals(value, this).TryObject);
        }

        public bool NotEquals(ScriptValue value)
        {
            return ScriptBool.AsCSharp(NotEquals(value, this).TryObject);
        }

        public override bool Equals(object obj)
        {
            if (obj is ScriptValue) 
                return this.Equals((ScriptValue)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public int CompareTo(ScriptValue other)
        {
            if (this.IsObject)
            {
                ScriptObject _this = this.TryObject;
                if (_this.ClassType.TryGetValue("CompareTo", out ScriptVariable fns) && fns.Value.IsFunction)
                {
                    var _fns = fns.Value.TryFunction;
                    return Convert.ToInt32(_fns.Call(_this, other).TryObject.BuildInObject);
                }
            }
            return 0;
        }
    }
}
