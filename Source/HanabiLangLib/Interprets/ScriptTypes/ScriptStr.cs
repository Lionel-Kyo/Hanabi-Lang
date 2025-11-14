using HanabiLangLib.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptStr : ScriptClass
    {
        public ScriptStr() :
            base("str", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
            this.InitializeOperators();

            this.AddFunction(ConstructorName, new List<FnParameter>() 
            {
                new FnParameter("this"),
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                if (args.Count == 2)
                {
                    ScriptObject value = args[1].TryObject;
                    _this.BuildInObject = value.ToString();
                }
                return ScriptValue.Null;
            });

            AddVariable("Iter", args =>
            {
                string text = AsCSharp(args[0].TryObject);
                var result = BasicTypes.Iterable.Create(text.Select(c => new ScriptValue(c)));
                // var result = BasicTypes.Iterable.Create(RuneIterable(text));
                return new ScriptValue(result);
            }, null, false, null);

            AddVariable("Length", args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(AsCSharp(_this).Length);
            }, null, false, null);

            this.AddFunction("SubStr", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("startIndex", BasicTypes.Int)
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                long startIndex = ScriptInt.AsCSharp(args[1].TryObject);
                return new ScriptValue(AsCSharp(_this).Substring(ScriptInt.ValidateToInt32(startIndex)));
            });

            this.AddFunction("SubStr", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("length", BasicTypes.Int)
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                long startIndex = ScriptInt.AsCSharp(args[1].TryObject);
                long length = ScriptInt.AsCSharp(args[2].TryObject);
                return new ScriptValue(AsCSharp(_this).Substring(ScriptInt.ValidateToInt32(startIndex), ScriptInt.ValidateToInt32(length)));
            });

            this.AddFunction("Contains", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value", BasicTypes.Str),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                string value = AsCSharp(args[1].TryObject);
                return new ScriptValue(AsCSharp(_this).Contains(value));
            });

            this.AddFunction("IndexOf", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value", BasicTypes.Str),
                new FnParameter("startIndex", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                string value = AsCSharp(args[1].TryObject);
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                if (args[3].IsNull)
                {
                    return new ScriptValue(AsCSharp(_this).IndexOf(value, startIndex));
                }
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[3].TryObject));
                return new ScriptValue(AsCSharp(_this).IndexOf(value, startIndex, count));
            });

            this.AddFunction("LastIndexOf", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value", BasicTypes.Str),
                new FnParameter("startIndex", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                string value = AsCSharp(args[1].TryObject);
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                if (args[3].IsNull)
                {
                    return new ScriptValue(AsCSharp(_this).LastIndexOf(value, startIndex));
                }
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[3].TryObject));
                return new ScriptValue(AsCSharp(_this).LastIndexOf(value, startIndex, count));
            });

            this.AddFunction("Remove", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                if (args[2].IsNull)
                {
                    return new ScriptValue(AsCSharp(_this).Remove(startIndex));
                }
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                return new ScriptValue(AsCSharp(_this).Remove(startIndex, count));
            });

            this.AddFunction("Replace", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("oldValue", BasicTypes.Str),
                new FnParameter("newValue", BasicTypes.Str),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                string oldValue = AsCSharp(args[1].TryObject);
                string newValue = AsCSharp(args[2].TryObject);
                return new ScriptValue(AsCSharp(_this).Replace(oldValue, newValue));
            });

            this.AddFunction("Split", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("separator", BasicTypes.Str, null, true),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject separators = args[1].TryObject;
                string[] separator = ScriptList.AsCSharp(separators).Select(v => AsCSharp(v.TryObject)).ToArray();
                List<ScriptValue> result = AsCSharp(_this).Split(separator, StringSplitOptions.None).Select(s =>  new ScriptValue(s)).ToList();
                return new ScriptValue(result);
            });

            this.AddFunction("StartsWith", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value", BasicTypes.Str),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                string value = AsCSharp(args[1].TryObject);
                return new ScriptValue(AsCSharp(_this).StartsWith(value));
            });

            this.AddFunction("EndsWith", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value", BasicTypes.Str),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                string value = AsCSharp(args[1].TryObject);
                return new ScriptValue(AsCSharp(_this).EndsWith(value));
            });

            this.AddFunction("ToUpper", new List<FnParameter>() { new FnParameter("this"), }
            , args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(AsCSharp(_this).ToUpper());
            });

            this.AddFunction("ToLower", new List<FnParameter>() { new FnParameter("this"), }
            , args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(AsCSharp(_this).ToLower());
            });

            this.AddFunction("Trim", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("chars", BasicTypes.Str, null, true),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject chars = args[1].TryObject;
                if (ScriptList.AsCSharp(chars).Count == 0)
                    return new ScriptValue(AsCSharp(_this).Trim());
                return new ScriptValue(AsCSharp(_this).Trim(ScriptList.AsCSharp(chars).Select(c => AsCSharp(c.TryObject)[0]).ToArray()));
            });

            this.AddFunction("TrimStart", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("chars", BasicTypes.Str, null, true),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject chars = args[1].TryObject;
                if (ScriptList.AsCSharp(chars).Count == 0)
                    return new ScriptValue(AsCSharp(_this).TrimStart());
                return new ScriptValue(AsCSharp(_this).TrimStart(ScriptList.AsCSharp(chars).Select(c => AsCSharp(c.TryObject)[0]).ToArray()));
            });

            this.AddFunction("TrimEnd", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("chars", BasicTypes.Str, null, true),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject chars = args[1].TryObject;
                if (ScriptList.AsCSharp(chars).Count == 0)
                    return new ScriptValue(AsCSharp(_this).TrimEnd());
                return new ScriptValue(AsCSharp(_this).TrimEnd(ScriptList.AsCSharp(chars).Select(c => AsCSharp(c.TryObject)[0]).ToArray()));
            });

            this.AddFunction("PadLeft", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("totalLength", BasicTypes.Int),
                new FnParameter("paddingChar", BasicTypes.Str, new ScriptValue()),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int totalLength = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                string paddingChar = AsCSharp(args[2].TryObject) ?? " ";
                if (paddingChar.Length <= 0)
                    throw new ArgumentException("paddingChar should not be empty");
                return new ScriptValue(AsCSharp(_this).PadLeft(totalLength, paddingChar[0]));
            });

            this.AddFunction("PadRight", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("totalLength", BasicTypes.Int),
                new FnParameter("paddingChar", BasicTypes.Str, new ScriptValue()),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int totalLength = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                string paddingChar = AsCSharp(args[2].TryObject) ?? " ";
                if (paddingChar.Length <= 0)
                    throw new ArgumentException("paddingChar should not be empty");
                return new ScriptValue(AsCSharp(_this).PadRight(totalLength, paddingChar[0]));
            });

            this.AddFunction("FromInt", new List<FnParameter>()
            {
                new FnParameter("character", BasicTypes.Int, null, false),
            }, args =>
            {
                var utf32 = char.ConvertFromUtf32(ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[0].TryObject)));
                return new ScriptValue(utf32);
            }, true);

            this.AddFunction("ToInt", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                string str = AsCSharp(args[0].TryObject);
                if (str.Length <= 0)
                    throw new IndexOutOfRangeException("Empty string cannot convert to int");
                return new ScriptValue(BitConverter.ToInt32(Encoding.UTF32.GetBytes(str), 0));
            }, false);

            this.AddFunction("Join", new List<FnParameter>()
            {
                new FnParameter("this", BasicTypes.Str, null, false),
                new FnParameter("values", BasicTypes.Iterable, null, false),
            }, args =>
            {
                string seperator = AsCSharp(args[0].TryObject);
                ScriptIterable.TryGetIterable(args[1].TryObject, out var values);
                return new ScriptValue(string.Join(seperator, values));
            }, false);

            this.AddFunction("CompareTo", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject value = args[1].TryObject;
                if (value != null && value.ClassType is ScriptStr)
                {
                    return new ScriptValue(AsCSharp(_this).CompareTo(AsCSharp(value)));
                }
                return new ScriptValue(0);
            });

            this.AddFunction("__GetIndexer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("indexes", BasicTypes.List) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                string value = AsCSharp(_this);

                List<ScriptValue> indexes = ScriptList.AsCSharp(args[1].TryObject);
                if (indexes.Count > 1)
                    throw new ArgumentException("Only 1 indexer is allowed for str");

                var indexObj = indexes[0].TryObject;

                if (indexObj?.ClassType == BasicTypes.Slice)
                {
                    var slice = ScriptSlice.AsCSharp(indexObj);
                    return new ScriptValue(Slice(AsCSharp(_this), slice.Start, slice.End, slice.Step));
                }

                if (indexObj?.ClassType != BasicTypes.Int)
                    throw new ArgumentException("Only int/slice is allowed for str indexer");

                long index = ScriptInt.AsCSharp(indexObj);

                return new ScriptValue(value[ScriptInt.ValidateToInt32(ScriptRange.GetModuloIndex(index, value.Length))]);
            });

            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(_this);
            });
        }

        private void InitializeOperators()
        {
            this.AddFunction(OPEARTOR_ADD, new List<FnParameter>()
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
                else
                {
                    if (_other.IsTypeOrSubOf(BasicTypes.Str))
                        resultObject = BasicTypes.Str.Create(AsCSharp(_this) + AsCSharp(_other));
                    else
                        resultObject = BasicTypes.Str.Create(AsCSharp(_this) + _other.ToString());
                }

                if (resultObject == null)
                    throw new Exception($"{_this} && {_other} is not defined");
                return new ScriptValue(resultObject);
            });
            this.AddFunction(OPEARTOR_MULTIPLY, new List<FnParameter>()
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
                    string text = AsCSharp(_this);
                    StringBuilder result = new StringBuilder();
                    long number = ScriptInt.AsCSharp(_other);
                    for (long i = 0; i < number; i++)
                    {
                        result.Append(text);
                    }
                    resultObject = BasicTypes.Str.Create(result);
                }

                if (resultObject == null)
                    throw new Exception($"{_this} && {_other} is not defined");
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
                if (_other.IsTypeOrSubOf(BasicTypes.Str))
                    return new ScriptValue(AsCSharp(_this).Equals(AsCSharp(_other)));
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
                if (_other.IsTypeOrSubOf(BasicTypes.Str))
                    return new ScriptValue(!AsCSharp(_this).Equals(AsCSharp(_other)));
                return new ScriptValue(true);
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, "");
        public ScriptObject Create(string value) => new ScriptObject(this, value);
        public ScriptObject Create(StringBuilder value) => new ScriptObject(this, value.ToString());
        public ScriptObject Create(char value) => new ScriptObject(this, value.ToString());

        private static IEnumerable<ScriptValue> RuneIterable(string text)
        {
            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                bool is32Bits = i + 1 < length && char.IsSurrogatePair(text[i], text[i + 1]);
                string utf32Char = is32Bits ? text.Substring(i, 2) : text.Substring(i, 1);
                if (is32Bits)
                    i++;
                yield return new ScriptValue(utf32Char);
            }
        }

        private static int GetRuneLength(string text)
        {
            int length = text.Length;
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                count++;
                if (i + 1 < length && char.IsSurrogatePair(text[i], text[i + 1]))
                    i++;
            }
            return count;
        }

        public static string Slice(string str, long? start = null, long? end = null, long? step = null)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            int length = str.Length;

            var adjusted = ScriptSlice.Slice.FillNullValues(length, start, end, step);
            int i32Start = ScriptInt.ValidateToInt32(adjusted.Start.Value);
            int i32End = ScriptInt.ValidateToInt32(adjusted.End.Value);
            int i32Step = ScriptInt.ValidateToInt32(adjusted.Step.Value);

            if (i32Step == 1)
            {
                if (i32Start > i32End)
                    return "";
                return str.Substring(i32Start, i32End - i32Start);
            }

            StringBuilder result = new StringBuilder();
            if (i32Step > 0)
            {
                for (int i = i32Start; i < i32End; i += i32Step)
                {
                    result.Append(str[i]);
                }
            }
            else
            {
                for (int i = i32Start; i > i32End; i += i32Step)
                {
                    result.Append(str[i]);
                }
            }
            return result.ToString();
        }

        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            return "\"" + (string)_this.BuildInObject +"\"";
        }

        public static string AsCSharp(ScriptObject _this)
        {
            return (string)_this.BuildInObject;
        }
    }
}
