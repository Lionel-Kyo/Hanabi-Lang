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
    class ScriptList : ScriptClass
    {
        public ScriptList() : 
            base("List", isStatic: false)
        {
            this.AddObjectFn(this.Name, new List<FnParameter>(), args => ScriptValue.Null);

            this.AddObjectFn(this.Name, new List<FnParameter>()
            {
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptObject value = (ScriptObject)args[1].Value;

                if (!value.Scope.TryGetValue("GetEnumerator", out ScriptType getEnumerator))
                    throw new SystemException("For loop running failed, variable is not enumerable");

                if (!(getEnumerator is ScriptFns))
                    throw new SystemException("For loop running failed, variable is not enumerable");

                var enumerator = ((ScriptFns)getEnumerator).Call(value);

                if (!(((ScriptObject)enumerator.Value).BuildInObject is IEnumerable<ScriptValue>))
                    throw new SystemException("For loop running failed, variable is not enumerable");

                var enumerable = (IEnumerable<ScriptValue>)(((ScriptObject)enumerator.Value).BuildInObject);
                _this.BuildInObject = enumerable.ToList();
                return ScriptValue.Null;
            });

            AddVariable("Length", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((List<ScriptValue>)_this.BuildInObject).Count);
            }, null, false, null);

            this.AddObjectFn("Add", new List<FnParameter>()
            {
                new FnParameter("item")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ((List<ScriptValue>)_this.BuildInObject).Add(args[1]);
                return ScriptValue.Null;
            });

            this.AddObjectFn("AddRange", new List<FnParameter>()
            {
                new FnParameter("list", BasicTypes.List)
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptObject collection = (ScriptObject)args[1].Value;
                ((List<ScriptValue>)_this.BuildInObject).AddRange((List<ScriptValue>)collection.BuildInObject);
                return ScriptValue.Null;
            });

            this.AddObjectFn("Clear", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ((List<ScriptValue>)_this.BuildInObject).Clear();
                return ScriptValue.Null;
            });

            this.AddObjectFn("Contains", new List<FnParameter>()
            {
                new FnParameter("item")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((List<ScriptValue>)_this.BuildInObject).Contains(args[1]));
            });

            this.AddObjectFn("Exists", new List<FnParameter>()
            {
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).Exists(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            this.AddObjectFn("Find", new List<FnParameter>()
            {
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).Find(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return result;
            });

            this.AddObjectFn("FindLast", new List<FnParameter>()
            {
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).FindLast(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return result;
            });

            this.AddObjectFn("FindAll", new List<FnParameter>()
            {
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).FindAll(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            this.AddObjectFn("FindIndex", new List<FnParameter>()
            {
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).FindIndex(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            this.AddObjectFn("FindIndex", new List<FnParameter>()
            {
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long startIndex = (long)((ScriptObject)args[1].Value).BuildInObject;
                ScriptFns match = (ScriptFns)args[2].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).FindIndex((int)startIndex, x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            this.AddObjectFn("FindIndex", new List<FnParameter>()
            {
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long startIndex = (long)((ScriptObject)args[1].Value).BuildInObject;
                long count = (long)((ScriptObject)args[2].Value).BuildInObject;
                ScriptFns match = (ScriptFns)args[3].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).FindIndex((int)startIndex, (int)count, x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            this.AddObjectFn("FindLastIndex", new List<FnParameter>()
            {
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).FindLastIndex(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            this.AddObjectFn("FindLastIndex", new List<FnParameter>()
            {
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long startIndex = (long)((ScriptObject)args[1].Value).BuildInObject;
                ScriptFns match = (ScriptFns)args[2].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).FindLastIndex((int)startIndex, x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            this.AddObjectFn("FindLastIndex", new List<FnParameter>()
            {
                new FnParameter("startIndex", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long startIndex = (long)((ScriptObject)args[1].Value).BuildInObject;
                long count = (long)((ScriptObject)args[2].Value).BuildInObject;
                ScriptFns match = (ScriptFns)args[3].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).FindLastIndex((int)startIndex, (int)count, x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            this.AddObjectFn("ForEach", new List<FnParameter>()
            {
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns match = (ScriptFns)args[1].Value;
                ((List<ScriptValue>)_this.BuildInObject).ForEach(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                });
                return ScriptValue.Null;
            });

            this.AddObjectFn("GetRange", new List<FnParameter>()
            {
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long startIndex = (long)((ScriptObject)args[1].Value).BuildInObject;
                long count = (long)((ScriptObject)args[2].Value).BuildInObject;
                var result = ((List<ScriptValue>)_this.BuildInObject).GetRange((int)startIndex, (int)count);
                return new ScriptValue(result);
            });

            this.AddObjectFn("IndexOf", new List<FnParameter>()
            {
                new FnParameter("item"),
                new FnParameter("index", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptValue item = args[1];
                long startIndex = (long)((ScriptObject)args[2].Value).BuildInObject;
                if (args[3].IsNull)
                {
                    return new ScriptValue(((List<ScriptValue>)_this.BuildInObject).IndexOf(item, (int)startIndex));
                }
                long count = (long)((ScriptObject)args[3].Value).BuildInObject;
                var result = ((List<ScriptValue>)_this.BuildInObject).IndexOf(item, (int)startIndex, (int)count);
                return new ScriptValue(result);
            });

            this.AddObjectFn("LastIndexOf", new List<FnParameter>()
            {
                new FnParameter("item"),
                new FnParameter("index", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("count", BasicTypes.Int, ScriptValue.Null),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptValue item = args[1];
                long startIndex = (long)((ScriptObject)args[2].Value).BuildInObject;
                if (args[3].IsNull)
                {
                    return new ScriptValue(((List<ScriptValue>)_this.BuildInObject).LastIndexOf(item, (int)startIndex));
                }
                long count = (long)((ScriptObject)args[3].Value).BuildInObject;
                var result = ((List<ScriptValue>)_this.BuildInObject).LastIndexOf(item, (int)startIndex, (int)count);
                return new ScriptValue(result);
            });

            this.AddObjectFn("Insert", new List<FnParameter>()
            {
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("item")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long index = (long)((ScriptObject)args[1].Value).BuildInObject;
                ScriptValue item = args[2];
                ((List<ScriptValue>)_this.BuildInObject).Insert((int)index, item);
                return ScriptValue.Null;
            });

            this.AddObjectFn("InsertRange", new List<FnParameter>()
            {
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("collection", BasicTypes.List)
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long index = (long)((ScriptObject)args[1].Value).BuildInObject;
                var collection = (List<ScriptValue>)((ScriptObject)args[2].Value).BuildInObject;
                ((List<ScriptValue>)_this.BuildInObject).InsertRange((int)index, collection);
                return ScriptValue.Null;
            });

            this.AddObjectFn("Remove", new List<FnParameter>()
            {
                new FnParameter("item")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptValue item = args[1];
                return new ScriptValue(((List<ScriptValue>)_this.BuildInObject).Remove(item));
            });

            this.AddObjectFn("RemoveAll", new List<FnParameter>()
            {
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).RemoveAll(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            this.AddObjectFn("RemoveAt", new List<FnParameter>()
            {
                new FnParameter("index", BasicTypes.Int),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long index = (long)((ScriptObject)args[1].Value).BuildInObject;
                ((List<ScriptValue>)_this.BuildInObject).RemoveAt((int)index);
                return ScriptValue.Null;
            });

            this.AddObjectFn("RemoveRange", new List<FnParameter>()
            {
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long index = (long)((ScriptObject)args[1].Value).BuildInObject;
                long count = (long)((ScriptObject)args[2].Value).BuildInObject;
                ((List<ScriptValue>)_this.BuildInObject).RemoveRange((int)index, (int)count);
                return ScriptValue.Null;
            });

            this.AddObjectFn("Reverse", new List<FnParameter>()
            {
                new FnParameter("index", BasicTypes.Int),
                new FnParameter("count", BasicTypes.Int),
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                long index = (long)((ScriptObject)args[1].Value).BuildInObject;
                long count = (long)((ScriptObject)args[2].Value).BuildInObject;
                ((List<ScriptValue>)_this.BuildInObject).Reverse((int)index, (int)count);
                return ScriptValue.Null;
            });

            this.AddObjectFn("Reverse", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ((List<ScriptValue>)_this.BuildInObject).Reverse();
                return ScriptValue.Null;
            });

            this.AddObjectFn("TrimExcess", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ((List<ScriptValue>)_this.BuildInObject).TrimExcess();
                return ScriptValue.Null;
            });

            this.AddObjectFn("TrueForAll", new List<FnParameter>()
            {
                new FnParameter("match")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns match = (ScriptFns)args[1].Value;
                var result = ((List<ScriptValue>)_this.BuildInObject).TrueForAll(x =>
                {
                    ScriptObject matchResult = (ScriptObject)match.Call(null, x).Value;
                    return (bool)matchResult.BuildInObject;
                });
                return new ScriptValue(result);
            });

            ScriptFns sortDefaultFns = new ScriptFns("");
            sortDefaultFns.Fns.Add(new ScriptFn(
                new List<FnParameter> { new FnParameter("left"), new FnParameter("right") }, null,
            args =>
            {
                if ((bool)((ScriptObject)(args[0] < args[1]).Value).BuildInObject)
                    return new ScriptValue(-1);
                else if (args[0].Equals(args[1]))
                    return new ScriptValue(0);
                return new ScriptValue(1);
            }, true, AccessibilityLevel.Public));

            this.AddObjectFn("Sort", new List<FnParameter>()
            {
                new FnParameter("compareFn", defaultValue:new ScriptValue(sortDefaultFns))
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptFns fns = (ScriptFns)args[1].Value;
                ((List<ScriptValue>)_this.BuildInObject).Sort((x, y) => 
                {
                    ScriptObject compare = (ScriptObject)fns.Call(null, x, y).Value;
                    return (int)(long)compare.BuildInObject;
                });
                return ScriptValue.Null;
            });;
            this.AddObjectFn("GetEnumerator", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var result =  BasicTypes.Enumerator.Create();
                result.BuildInObject = (List<ScriptValue>)_this.BuildInObject;
                return new ScriptValue(result);
            });
            this.AddObjectFn("get_[]", new List<FnParameter> { new FnParameter("index", BasicTypes.Int) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                long index = (long)args[1].TryObject.BuildInObject;
                List<ScriptValue> listValue = (List<ScriptValue>)_this.BuildInObject;

                if ((index >= listValue.Count) || index < 0 && index < (listValue.Count * -1))
                    throw new IndexOutOfRangeException();

                return listValue[(int)ScriptInt.Modulo(index, listValue.Count)];
            });
            this.AddObjectFn("set_[]", new List<FnParameter> { new FnParameter("index", BasicTypes.Int), new FnParameter("value") }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                long index = (long)args[1].TryObject.BuildInObject;
                List<ScriptValue> listValue = (List<ScriptValue>)_this.BuildInObject;

                if ((index >= listValue.Count) || index < 0 && index < (listValue.Count * -1))
                    throw new IndexOutOfRangeException();
                listValue[(int)ScriptInt.Modulo(index, listValue.Count)] = args[2];

                return ScriptValue.Null;
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, new List<ScriptValue>());
        public ScriptObject Create(List<ScriptValue> value) => new ScriptObject(this, value);

        public override ScriptObject Negative(ScriptObject left)
        {
            var result = ((List<ScriptValue>)left.BuildInObject).ToList();
            result.Reverse();
            return BasicTypes.List.Create(result);
        }

        public override ScriptObject Add(ScriptObject left, ScriptObject right)
        {
            if (right.ClassType is ScriptList)
            {
                var list = ((List<ScriptValue>)left.BuildInObject);
                List<ScriptValue> result = new List<ScriptValue>(list);
                result.AddRange(((List<ScriptValue>)right.BuildInObject));
                return BasicTypes.List.Create(result);
            }
            return base.Add(left, right);
        }

        public override ScriptObject Multiply(ScriptObject left, ScriptObject right)
        {
            if (right.ClassType is ScriptInt)
            {
                long value = (long)right.BuildInObject;
                List<ScriptValue> list = new List<ScriptValue>();
                for (long i = 0; i < value; i++)
                {
                    list.AddRange((List<ScriptValue>)left.BuildInObject);
                }
                return BasicTypes.List.Create(list);
            }
            return base.Add(left, right);
        }

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptList)
            {
                var a = (List<ScriptValue>)_this.BuildInObject;
                var b = (List<ScriptValue>)value.BuildInObject;
                if (a.Equals(b))
                    return ScriptBool.True;
                if (a.Count == b.Count)
                {
                    for (int i = 0; i < a.Count; i++)
                    {
                        if (!a[i].Equals(b[i]))
                            return ScriptBool.False;
                    }
                    return ScriptBool.True;
                }
                return ScriptBool.False;
            }
            return ScriptBool.False;
        }

        public override ScriptObject ToStr(ScriptObject _this)
        {
            StringBuilder result = new StringBuilder();
            result.Append('[');
            foreach (var item in (List<ScriptValue>)_this.BuildInObject)
            {
                if (item.IsObject && ((ScriptObject)item.Value).ClassType is ScriptStr)
                    result.Append($"\"{item}\", ");
                else
                    result.Append($"{item}, ");

            }
            if (result.Length > 1)
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
            foreach (var item in (List<ScriptValue>)_this.BuildInObject)
            {
                if (!item.IsObject)
                    throw new SystemException("list item contain not object");

                ScriptObject itemObject = (ScriptObject)item.Value;
                result.Append(' ', currentIndent);
                result.Append($"{itemObject.ClassType.ToJsonString(itemObject, basicIndent, currentIndent)}");

                if (count < ((List<ScriptValue>)_this.BuildInObject).Count - 1)
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
    }
}
