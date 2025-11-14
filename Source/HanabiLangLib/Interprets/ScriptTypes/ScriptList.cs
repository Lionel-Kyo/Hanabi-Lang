using HanabiLangLib.Interprets.Json5Converter;
using HanabiLangLib.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptList : ScriptClass
    {
        public ScriptList() :
            base("List", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
            this.InitializeOperators();

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
                    int size = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(value));
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
                var result = AsCSharp(_this).FindLastIndex(startIndex, x =>
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
                var result = AsCSharp(_this).FindLastIndex(startIndex, count, x =>
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
                    return new ScriptValue(AsCSharp(_this).LastIndexOf(item, startIndex));
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
                if (ScriptBool.AsCSharp(ScriptValue.Less(args[0], args[1]).TryObject))
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
                    return ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(compare));
                });
                return ScriptValue.Null;
            }); 

            this.AddFunction("__GetIndexer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("indexes", BasicTypes.List) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                List<ScriptValue> listValue = AsCSharp(_this);

                List<ScriptValue> indexes = ScriptList.AsCSharp(args[1].TryObject);
                if (indexes.Count > 1)
                    throw new ArgumentException("Only 1 indexer is allowed for List");

                var indexObj = indexes[0].TryObject;
                if (indexObj?.ClassType == BasicTypes.Slice)
                {
                    var slice = ScriptSlice.AsCSharp(indexObj);
                    return new ScriptValue(ScriptIterable.Slice(AsCSharp(_this), slice.Start, slice.End, slice.Step));
                }

                if (indexObj?.ClassType != BasicTypes.Int)
                    throw new ArgumentException("Only int/slice is allowed for List indexer");

                long index = ScriptInt.AsCSharp(indexObj);

                return listValue[ScriptInt.ValidateToInt32(ScriptRange.GetModuloIndex(index, listValue.Count))];
            });
            this.AddFunction("__SetIndexer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("indexes", BasicTypes.List), new FnParameter("value") }, args =>
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

                listValue[ScriptInt.ValidateToInt32(ScriptRange.GetModuloIndex(index, listValue.Count))] = args[2];

                return ScriptValue.Null;
            });

            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(ToStr(_this, null));
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
                else if (ScriptIterable.TryGetIterable(_other, out var otherIter))
                {
                    resultObject = BasicTypes.List.Create(AsCSharp(_this).Concat(otherIter).ToList());
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
                    long value = ScriptInt.AsCSharp(_other);
                    List<ScriptValue> leftList = AsCSharp(_this);
                    List<ScriptValue> result = new List<ScriptValue>(ScriptInt.ValidateToInt32(leftList.Count * value));
                    for (long i = 0; i < value; i++)
                    {
                        result.AddRange(leftList);
                    }
                    resultObject = BasicTypes.List.Create(result.ToList());
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

        public override ScriptObject Create() => new ScriptObject(this, new List<ScriptValue>());
        public ScriptObject Create(List<ScriptValue> value) => new ScriptObject(this, value);

        private bool OperatorEquals(ScriptValue value1, ScriptValue value2)
        {
            ScriptObject _this = value1.TryObject;
            ScriptObject _other = value2.TryObject;
            if (_other.IsTypeOrSubOf(BasicTypes.List))
            {
                if (object.ReferenceEquals(_this, _other))
                    return true;

                var a = AsCSharp(_this);
                var b = AsCSharp(_other);
                if (a.Equals(b))
                    return true;
                if (a.Count != b.Count)
                    return false;

                for (int i = 0; i < a.Count; i++)
                {
                    if (ScriptBool.AsCSharp(ScriptValue.NotEquals(a[i], b[i]).TryObject))
                        return false;
                }
                return true;
            }
            return false;
        }

        public static string ToStr(ScriptObject _this)
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
            return result.ToString();
        }

        public static string ToStr(ScriptObject _this, string indent = null)
        {
            return ToStr(_this, indent, 0, new HashSet<ScriptValue>(new HashSet<ScriptValue>(ReferenceEqualityComparer.Instance)));
        }

        public static string ToStr(ScriptObject _this, string indent, int level, HashSet<ScriptValue> visited)
        {
            var list = AsCSharp(_this);
            StringBuilder result = new StringBuilder();
            if (list.Count <= 0)
                return "[]";

            result.Append('[');
            if (indent != null)
                result.AppendLine();

            int count = 0;
            foreach (var item in AsCSharp(_this))
            {
                var _object = item.TryObject;
                if (visited.Contains(item))
                {
                    result.Append("[...]");
                }
                else
                {
                    visited.Add(item);
                    if (_object?.IsTypeOrSubOf(BasicTypes.Str) ?? false)
                        result.Append(Json5Serializer.QuoteString(ScriptStr.AsCSharp(_object), '"', false, true));
                    else if (_object?.IsTypeOrSubOf(BasicTypes.List) ?? false)
                        result.Append(ToStr(_object, indent, level + 1, visited));
                    else if (_object?.IsTypeOrSubOf(BasicTypes.Dict) ?? false)
                        result.Append(ScriptDict.ToStr(_object, indent, level + 1, visited));
                    else
                        result.Append(item.ToString());
                    visited.Remove(item);
                }

                count++;
                if (count < list.Count)
                    result.Append(", ");
                if (indent != null)
                    result.AppendLine();
            }
            result.Append(ScriptDict.GetIndent(indent, level));
            result.Append(']');
            return result.ToString();
        }

        public static List<ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (List<ScriptValue>)_this.BuildInObject;
        }
    }
}
