using HanabiLang.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptList : ScriptClass
    {
        public ScriptList() :
            base("List", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                _this.BuildInObject = new List<ScriptValue>();
                return ScriptValue.Null;
            });

            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject value = (ScriptObject)args[1].Value;

                if (value.ClassType == BasicTypes.Int)
                {
                    int size = (int)ScriptInt.AsCSharp(value);
                    _this.BuildInObject = Enumerable.Repeat(new ScriptValue(), size).ToList();
                }
                else
                {
                    if (!ScriptIterable.TryGetIterable(value, out var iter))
                        throw new SystemException("Create List failed, variable is not enumerable");

                    _this.BuildInObject = new List<ScriptValue>(iter);
                }
                return ScriptValue.Null;
            });

            AddVariable("Length", args =>
            {
                ScriptObject _this = args[0].TryObject; 
                return new ScriptValue(AsCSharp(_this).Count);
            }, null, false, null);

            AddVariable("Iter", args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(BasicTypes.Iterable.Create(AsCSharp(_this)));
            }, null, false, null);

            this.AddFunction("Add", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("item")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                AsCSharp(_this).Add(args[1]);
                return ScriptValue.Null;
            });

            this.AddFunction("AddRange", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("collection", BasicTypes.Iterable)
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptIterable.TryGetIterable(args[1].TryObject, out var collection);
                AsCSharp(_this).AddRange(collection);
                return ScriptValue.Null;
            });

            this.AddFunction("Clear", new List<FnParameter>() { new FnParameter("this"), }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                AsCSharp(_this).Clear();
                return ScriptValue.Null;
            });

            this.AddFunction("Contains", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("item")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(AsCSharp(_this).Contains(args[1]));
            });

            this.AddFunction("Exists", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = AsCSharp(_this).Exists(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("Find", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = AsCSharp(_this).Find(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return result;
            });

            this.AddFunction("FindLast", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = AsCSharp(_this).FindLast(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return result;
            });

            this.AddFunction("FindAll", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = AsCSharp(_this).FindAll(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("FindIndex", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = AsCSharp(_this).FindIndex(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("FindIndex", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                ScriptFns match = (ScriptFns)args[2].Value;
                var result = AsCSharp(_this).FindIndex(startIndex, x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("FindIndex", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                ScriptFns match = (ScriptFns)args[3].Value;
                var result = AsCSharp(_this).FindIndex(startIndex, count, x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("FindLastIndex", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = AsCSharp(_this).FindLastIndex(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("FindLastIndex", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                ScriptFns match = (ScriptFns)args[2].Value;
                var result = AsCSharp(_this).FindLastIndex((int)startIndex, x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("FindLastIndex", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                ScriptFns match = (ScriptFns)args[3].Value;
                var result = AsCSharp(_this).FindLastIndex((int)startIndex, (int)count, x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("ForEach", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns action = (ScriptFns)args[1].Value;
                AsCSharp(_this).ForEach(x =>
                {
                    ScriptObject result = (ScriptObject)action.Call(null, x).Value;
                });
                return ScriptValue.Null;
            });

            this.AddFunction("GetRange", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                var result = AsCSharp(_this).GetRange(startIndex, count);
                return new ScriptValue(result);
            });

            this.AddFunction("IndexOf", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("item"),
                new FnParameter("index", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue item = args[1];
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                if (args[3].IsNull)
                {
                    return new ScriptValue(AsCSharp(_this).IndexOf(item, startIndex));
                }
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[3].TryObject));
                var result = AsCSharp(_this).IndexOf(item, startIndex, count);
                return new ScriptValue(result);
            });

            this.AddFunction("LastIndexOf", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("item"),
                new FnParameter("index", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue item = args[1];
                int startIndex = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                if (args[3].IsNull)
                {
                    return new ScriptValue(AsCSharp(_this).LastIndexOf(item, (int)startIndex));
                }
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[3].TryObject));
                var result = AsCSharp(_this).LastIndexOf(item, startIndex, count);
                return new ScriptValue(result);
            });

            this.AddFunction("Insert", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("item")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int index = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                ScriptValue item = args[2];
                AsCSharp(_this).Insert(index, item);
                return ScriptValue.Null;
            });

            this.AddFunction("InsertRange", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("collection", BasicTypes.List)
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int index = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                ScriptIterable.TryGetIterable(args[2].TryObject, out var collection);
                AsCSharp(_this).InsertRange(index, collection);
                return ScriptValue.Null;
            });

            this.AddFunction("Remove", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("item")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue item = args[1];
                return new ScriptValue(AsCSharp(_this).Remove(item));
            });

            this.AddFunction("RemoveAll", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = AsCSharp(_this).RemoveAll(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("RemoveAt", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("index", BasicTypes.Int),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int index = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                AsCSharp(_this).RemoveAt(index);
                return ScriptValue.Null;
            });

            this.AddFunction("RemoveRange", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int index = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                AsCSharp(_this).RemoveRange(index, count);
                return ScriptValue.Null;
            });

            this.AddFunction("Reverse", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int index = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject));
                int count = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[2].TryObject));
                AsCSharp(_this).Reverse(index, count);
                return ScriptValue.Null;
            });

            this.AddFunction("Reverse", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                AsCSharp(_this).Reverse();
                return ScriptValue.Null;
            });

            this.AddFunction("TrimExcess", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                AsCSharp(_this).TrimExcess();
                return ScriptValue.Null;
            });

            this.AddFunction("TrueForAll", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = AsCSharp(_this).TrueForAll(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return ScriptBool.AsCSharp(matchResult);
                });
                return new ScriptValue(result);
            });

            ScriptFns defaultSortFns = new ScriptFns("");
            defaultSortFns.Fns.Add(new ScriptFn(
                new List<FnParameter> { new FnParameter("this"), new FnParameter("left"), new FnParameter("right") }, null,
            args =>
            {
                if (ScriptBool.AsCSharp((args[0] < args[1]).TryObject))
                    return new ScriptValue(-1);
                else if (args[0].Equals(args[1]))
                    return new ScriptValue(0);
                return new ScriptValue(1);
            }, true, AccessibilityLevel.Public));

            this.AddFunction("Sort", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("compareFn", defaultValue: new ScriptValue(defaultSortFns))
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptFns fns = (ScriptFns)args[1].Value;
                AsCSharp(_this).Sort((x, y) =>
                {
                    ScriptObject compare = (ScriptObject)fns.Call(null, x, y).Value;
                    return (int)ScriptInt.AsCSharp(compare);
                });
                return ScriptValue.Null;
            }); 

            this.AddFunction("__GetIndexer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("index", BasicTypes.List) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                List<ScriptValue> listValue = AsCSharp(_this);

                List<ScriptValue> indexes = ScriptList.AsCSharp(args[1].TryObject);
                if (indexes.Count > 1)
                    throw new ArgumentException("Only 1 indexer is allowed for List");

                var indexObj = indexes[0].TryObject;
                if (indexObj?.ClassType != BasicTypes.Int)
                    throw new ArgumentException("Only int is allowed for List indexer");
                long index = ScriptInt.AsCSharp(indexObj);

                if ((index >= listValue.Count) || index < 0 && index < (listValue.Count * -1))
                    throw new IndexOutOfRangeException();

                return listValue[(int)ScriptInt.Modulo(index, listValue.Count)];
            });
            this.AddFunction("__SetIndexer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("index", BasicTypes.List), new FnParameter("value") }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                List<ScriptValue> listValue = AsCSharp(_this);

                List<ScriptValue> indexes = ScriptList.AsCSharp(args[1].TryObject);
                if (indexes.Count > 1)
                    throw new ArgumentException("Only 1 indexer is allowed for List");

                var indexObj = indexes[0].TryObject;
                if (indexObj?.ClassType != BasicTypes.Int)
                    throw new ArgumentException("Only int is allowed for List indexer");
                long index = ScriptInt.AsCSharp(indexObj);

                if ((index >= listValue.Count) || index < 0 && index < (listValue.Count * -1))
                    throw new IndexOutOfRangeException();
                listValue[(int)ScriptInt.Modulo(index, listValue.Count)] = args[2];

                return ScriptValue.Null;
            });
            this.AddFunction("__GetSlicer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("slicer", BasicTypes.List) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                List<ScriptValue> slicer = ScriptList.AsCSharp(args[1].TryObject);

                if (slicer.Count > 1)
                    throw new ArgumentException("Only 1 slicer is allowed for List");

                List<ScriptValue> slicerValues = ScriptList.AsCSharp(slicer[0].TryObject);

                long? start = slicerValues[0].IsNull ? (long?)null : ScriptInt.AsCSharp(slicerValues[0].TryObject);
                long? end = slicerValues[1].IsNull ? (long?)null : ScriptInt.AsCSharp(slicerValues[1].TryObject);
                long? step = slicerValues[2].IsNull ? (long?)null : ScriptInt.AsCSharp(slicerValues[2].TryObject);
                return new ScriptValue(ScriptIterable.Slice(AsCSharp(_this), start, end, step));
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, new List<ScriptValue>());
        public ScriptObject Create(List<ScriptValue> value) => new ScriptObject(this, value);

        //public override ScriptObject Negative(ScriptObject left)
        //{
        //    var result = ((List<ScriptValue>)left.BuildInObject).ToList();
        //    result.Reverse();
        //    return BasicTypes.List.Create(result);
        //}

        public override ScriptObject Add(ScriptObject left, ScriptObject right)
        {
            if (ScriptIterable.TryGetIterable(right, out var rightIter))
            {
                return BasicTypes.List.Create(AsCSharp(left).Concat(rightIter).ToList());
            }
            return base.Add(left, right);
        }

        public override ScriptObject Multiply(ScriptObject left, ScriptObject right)
        {
            if (right.ClassType is ScriptInt)
            {
                long value = ScriptInt.AsCSharp(right);
                List<ScriptValue> leftList = AsCSharp(left);
                List<ScriptValue> result = new List<ScriptValue>((int)(leftList.Count * value));
                for (long i = 0; i < value; i++)
                {
                    result.AddRange(leftList);
                }
                return BasicTypes.List.Create(result.ToList());
            }
            return base.Add(left, right);
        }

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptList)
            {
                var a = AsCSharp(_this);
                var b = AsCSharp(value);
                if (a.Equals(b))
                    return ScriptBool.True;
                if (a.Count != b.Count)
                    return ScriptBool.False;

                for (int i = 0; i < a.Count; i++)
                {
                    if (!a[i].Equals(b[i]))
                        return ScriptBool.False;
                }
                return ScriptBool.True;
            }
            return ScriptBool.False;
        }

        public override ScriptObject ToStr(ScriptObject _this)
        {
            StringBuilder result = new StringBuilder();
            result.Append('[');
            foreach (var item in AsCSharp(_this))
            {
                if (item.TryObject == _this)
                    result.Append("[...], ");
                else if (item.TryObject?.ClassType is ScriptStr)
                    result.Append($"\"{item}\", ");
                else
                    result.Append($"{item}, ");
            }
            if (result.Length >= 2)
                result.Remove(result.Length - 2, 2);
            result.Append(']');
            return BasicTypes.Str.Create(result.ToString());
        }

        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            StringBuilder result = new StringBuilder();
            result.Append('[');
            if (basicIndent != 0)
            {
                result.AppendLine();
                currentIndent += 2;
            }
            int count = 0;
            foreach (var item in AsCSharp(_this))
            {
                if (!item.IsObject)
                    throw new SystemException("list item contain not object");

                ScriptObject itemObject = (ScriptObject)item.Value;
                result.Append(' ', currentIndent);
                result.Append($"{itemObject.ClassType.ToJsonString(itemObject, basicIndent, currentIndent)}");

                if (count < (AsCSharp(_this)).Count - 1)
                {
                    result.Append(", ");
                    if (basicIndent != 0)
                        result.AppendLine();
                }
                count++;
            }
            if (basicIndent != 0)
            {
                currentIndent -= 2;
                result.Append(' ', currentIndent);
                result.AppendLine();
            }
            result.Append(' ', currentIndent);
            result.Append(']');
            return result.ToString();
        }

        public static List<ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (List<ScriptValue>)_this.BuildInObject;
        }
    }
}
