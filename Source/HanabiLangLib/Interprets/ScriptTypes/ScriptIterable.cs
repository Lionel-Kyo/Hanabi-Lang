using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static HanabiLangLib.Interprets.ScriptTypes.ScriptRange;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptIterable : ScriptClass
    {
        public ScriptIterable() :
            base("Iterable", isStatic: false)
        {
            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("iter", BasicTypes.Iterable),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                TryGetIterable(args[1].TryObject, out var iter);
                _this.BuildInObject = iter;

                return ScriptValue.Null;
            });

            this.AddFunction("Iterator", new List<FnParameter>()
            {
                new FnParameter("currentFn"),
                new FnParameter("moveNextFn"),
                new FnParameter("resetFn")
            }, args =>
            {
                foreach (var arg in args)
                    if (!arg.IsFunction)
                        throw new SystemException($"{arg} is not a function");

                var currentFn = args[0];
                var moveNextFn = args[1];
                var resetFn = args[2];

                return new ScriptValue(BasicTypes.Iterable.Create(new ScriptIterator(currentFn, moveNextFn, resetFn)));
            }, true);

            AddVariable(GET_ITERABLE, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var result = BasicTypes.Iterable.Create(AsCSharp(_this));
                return new ScriptValue(result);
            }, null, false, null);

            this.AddFunction("All", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("predicate")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns predicate = args[1].TryFunction;
                var result = iter.All(x =>
                {
                    ScriptObject callResult = (ScriptObject)predicate.Call(null, x).Value;
                    return ScriptBool.AsCSharp(callResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("Any", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("predicate")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns predicate = args[1].TryFunction;
                var result = iter.Any(x =>
                {
                    ScriptObject callResult = (ScriptObject)predicate.Call(null, x).Value;
                    return ScriptBool.AsCSharp(callResult);
                });
                return new ScriptValue(result);
            });

            this.AddFunction("Average", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("selector", (ScriptClass)null, new ScriptValue(), false)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                double result;
                if (args[1].IsNull)
                {
                    result = iter.Average(x =>
                    {
                        if (!x.IsObject)
                            throw new ArgumentException();
                        if (x.IsNull)
                            throw new ArgumentException();
                        ScriptObject obj = x.TryObject;
                        if (obj?.ClassType == BasicTypes.Int)
                            return (double)ScriptInt.AsCSharp(obj);
                        else if (obj?.ClassType == BasicTypes.Float)
                            return (double)ScriptFloat.AsCSharp(obj);
                        else if (obj?.ClassType == BasicTypes.Decimal)
                            return (double)ScriptDecimal.AsCSharp(obj);

                        throw new ArgumentException();
                    });
                }
                else
                {
                    ScriptFns selector = args[1].TryFunction;
                    result = iter.Average(x =>
                    {
                        if (!x.IsObject)
                            throw new ArgumentException();

                        ScriptObject callResult = (ScriptObject)selector.Call(null, x).Value;
                        if (callResult?.ClassType == BasicTypes.Int)
                            return (double)ScriptInt.AsCSharp(callResult);
                        else if (callResult?.ClassType == BasicTypes.Float)
                            return (double)ScriptFloat.AsCSharp(callResult);
                        else if (callResult?.ClassType == BasicTypes.Decimal)
                            return (double)ScriptDecimal.AsCSharp(callResult);

                        throw new ArgumentException();
                    });
                }
                return new ScriptValue(result);
            });

            this.AddFunction("Concat", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("second", BasicTypes.Iterable)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter1);
                TryGetIterable(args[1].TryObject, out var iter2);
                var result = iter1.Concat(iter2);
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("Contains", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Contains(args[1]);
                return new ScriptValue(result);
            });

            this.AddFunction("Count", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Count();
                return new ScriptValue(result);
            });

            this.AddFunction("Distinct", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Distinct();
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("ElementAt", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("index", BasicTypes.Int)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.ElementAt(ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject)));
                return result;
            });

            this.AddFunction("Except", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("second", BasicTypes.Iterable)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter1);
                TryGetIterable(args[1].TryObject, out var iter2);
                var result = iter1.Except(iter2);
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("First", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.First();
                return result;
            });

            //this.AddFunction("GroupBy", new List<FnParameter>()
            //{
            //     new FnParameter("this"),
            //     new FnParameter("predicate")
            //}, args =>
            //{
            //    TryGetIterable(args[0].TryObject, out var iter);
            //    ScriptFns predicate = args[1].TryFunction;
            //    var result = iter.GroupBy(x =>
            //    {
            //        ScriptObject matchResult = (ScriptObject)predicate.Call(null, x).Value;
            //        return matchResult;
            //    });
            //    return new ScriptValue(BasicTypes.Enumerator.Create(result));
            //});

            //this.AddFunction("GroupJoin", new List<FnParameter>()
            //{
            //     new FnParameter("this"),
            //     new FnParameter("predicate")
            //}, args =>
            //{
            //    TryGetIterable(args[0].TryObject, out var iter);
            //    ScriptFns predicate = args[1].TryFunction;
            //    var result = iter.GroupJoin(x =>
            //    {
            //        ScriptObject matchResult = (ScriptObject)predicate.Call(null, x).Value;
            //        return matchResult;
            //    });
            //    return new ScriptValue(BasicTypes.Enumerator.Create(result));
            //});

            this.AddFunction("Intersect", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("second", BasicTypes.Iterable)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter1);
                TryGetIterable(args[1].TryObject, out var iter2);
                var result = iter1.Intersect(iter2);
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            //this.AddFunction("Join", new List<FnParameter>()
            //{
            //     new FnParameter("this"),
            //     new FnParameter("predicate")
            //}, args =>
            //{
            //    TryGetIterable(args[0].TryObject, out var iter);
            //    ScriptFns predicate = args[1].TryFunction;
            //    var result = iter.Join(x =>
            //    {
            //        ScriptObject matchResult = (ScriptObject)predicate.Call(null, x).Value;
            //        return matchResult;
            //    });
            //    return new ScriptValue(BasicTypes.Enumerator.Create(result));
            //});

            this.AddFunction("Last", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Last();
                return result;
            });

            this.AddFunction("Max", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Max();
                return result;
            });

            this.AddFunction("Min", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Min();
                return result;
            });

            this.AddFunction("OrderBy", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("keySelector")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                var result = iter.OrderBy(x =>
                {
                    ScriptValue callResult = keySelector.Call(null, x);
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("OrderByDescending", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("keySelector")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                var result = iter.OrderByDescending(x =>
                {
                    ScriptValue callResult = keySelector.Call(null, x);
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("Reverse", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Reverse();
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("Select", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("selector")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                IEnumerable<ScriptValue> result;
                result = iter.Select((x) =>
                {
                    ScriptValue callResult = keySelector.Call(null, x);
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("SelectWithIndex", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("selector")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                IEnumerable<ScriptValue> result;
                result = iter.Select((x, i) =>
                {
                    ScriptValue callResult = keySelector.Call(null, x, new ScriptValue(i));
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("SelectMany", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("selector")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                var result = iter.SelectMany(x =>
                {
                    ScriptObject callResult = keySelector.Call(null, x).TryObject;
                    return AsCSharp(callResult);
                });
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("SelectManyWithIndex", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("selector")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                var result = iter.SelectMany((x, i) =>
                {
                    ScriptObject callResult = keySelector.Call(null, x, new ScriptValue(i)).TryObject;
                    return AsCSharp(callResult);
                });
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("SequenceEqual", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("second", BasicTypes.Iterable)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter1);
                TryGetIterable(args[1].TryObject, out var iter2);
                var result = iter1.SequenceEqual(iter2);
                return new ScriptValue(result);
            });

            this.AddFunction("Single", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Single();
                return result;
            });

            this.AddFunction("Skip", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("count", BasicTypes.Int)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Skip(ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject)));
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("Sum", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("selector", (ScriptClass)null, new ScriptValue(), false)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                double result;
                if (args[1].IsNull)
                {
                    result = iter.Sum(x =>
                    {
                        if (!x.IsObject)
                            throw new ArgumentException();
                        if (x.IsNull)
                            throw new ArgumentException();
                        ScriptObject obj = x.TryObject;
                        if (obj?.ClassType == BasicTypes.Int)
                            return (double)ScriptInt.AsCSharp(obj);
                        else if (obj?.ClassType == BasicTypes.Float)
                            return (double)ScriptFloat.AsCSharp(obj);
                        else if (obj?.ClassType == BasicTypes.Decimal)
                            return (double)ScriptDecimal.AsCSharp(obj);

                        throw new ArgumentException();
                    });
                }
                else
                {
                    ScriptFns selector = args[1].TryFunction;
                    result = iter.Sum(x =>
                    {
                        ScriptObject callResult = selector.Call(null, x).TryObject;
                        if (callResult?.ClassType == BasicTypes.Int)
                            return (double)ScriptInt.AsCSharp(callResult);
                        else if (callResult?.ClassType == BasicTypes.Float)
                            return (double)ScriptFloat.AsCSharp(callResult);
                        else if (callResult?.ClassType == BasicTypes.Decimal)
                            return (double)ScriptDecimal.AsCSharp(callResult);

                        throw new ArgumentException();
                    });
                }
                return new ScriptValue(result);
            });

            this.AddFunction("Take", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("count", BasicTypes.Int)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.Take(ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(args[1].TryObject)));
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("ToDict", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("keySelector"),
                new FnParameter("elementSelector")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                ScriptFns elementSelector = args[2].TryFunction;
                var result = iter.ToDictionary(x => keySelector.Call(null, x), y => elementSelector.Call(null, y));
                return new ScriptValue(result);
            });

            this.AddFunction("ToList", new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                var result = iter.ToList();
                return new ScriptValue(result);
            });

            this.AddFunction("Union", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("second", BasicTypes.Iterable)
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter1);
                TryGetIterable(args[1].TryObject, out var iter2);
                var result = iter1.Union(iter2);
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("Where", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("predicate")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter);
                ScriptFns predicate = args[1].TryFunction;
                var result = iter.Where(x =>
                {
                    ScriptObject callResult = predicate.Call(null, x).TryObject;
                    return ScriptBool.AsCSharp(callResult);
                });
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction("Zip", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("second", BasicTypes.Iterable),
                new FnParameter("resultSelector")
            }, args =>
            {
                TryGetIterable(args[0].TryObject, out var iter1);
                TryGetIterable(args[1].TryObject, out var iter2);
                ScriptFns resultSelector = args[2].TryFunction;
                var result = iter1.Zip(iter2, (x, y) =>
                {
                    ScriptValue callResult = resultSelector.Call(null, x, y);
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterable.Create(result));
            });

            this.AddFunction(GET_INDEXER, new List<FnParameter> { new FnParameter("this"), new FnParameter("indexes", BasicTypes.List) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                if (!TryGetIterable(_this, out var iter))
                    throw new SystemException($"{_this} is not iterable");
                List<ScriptValue> indexes = ScriptList.AsCSharp(args[1].TryObject);

                if (indexes.Count > 1)
                    throw new ArgumentException("Only 1 indexer is allowed for Iterable");

                var indexObj = indexes[0].TryObject;

                if (indexObj?.ClassType == BasicTypes.Slice)
                {
                    var slice = ScriptSlice.AsCSharp(indexes[0].TryObject);

                    return new ScriptValue(Slice(iter, slice.Start, slice.End, slice.Step));
                }

                throw new ArgumentException("Only slice is allowed for iterable indexer");
            });

            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(BasicTypes.Str.Create($"<object: {_this.ClassType.Name}>"));
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, (IEnumerable<ScriptValue>)new List<ScriptValue>());
        public ScriptObject Create(IEnumerable<ScriptValue> value) => new ScriptObject(this, value);

        public static bool TryGetIterable(ScriptObject obj, out IEnumerable<ScriptValue> result)
        {
            result = null;

            if (obj == null) 
                return false;

            if (obj.ClassType == BasicTypes.Iterable)
            {
                result = AsCSharp(obj);
                return true;
            }
            if (!obj.Scope.TryGetValue(GET_ITERABLE, out ScriptVariable iter)) 
                return false;

            ScriptObject iterObj;
            var getFns = (iter).Get;
            if (getFns == null)
                iterObj = (iter).Value.TryObject;
            else
                iterObj = getFns.Call(obj).TryObject;

            if (iterObj == null || iterObj.ClassType != BasicTypes.Iterable) 
                return false;

            result = AsCSharp(iterObj);
            return true;
        }

        public static List<T> Slice<T>(IEnumerable<T> iterable, long? start = null, long? end = null, long? step = null)
        {
            if (iterable == null)
                throw new ArgumentNullException("iterable");

            var list = iterable is IList<T> ? (IList<T>)iterable : new List<T>(iterable);
            int length = list.Count;

            var adjusted = ScriptSlice.Slice.FillNullValues(length, start, end, step);
            int i32Start = ScriptInt.ValidateToInt32(adjusted.Start.Value);
            int i32End = ScriptInt.ValidateToInt32(adjusted.End.Value);
            int i32Step = ScriptInt.ValidateToInt32(adjusted.Step.Value);

            if (i32Step == 1)
            {
                if (i32Start > i32End)
                    return new List<T>();

                if (list is List<T>)
                {
                    return ((List<T>)list).GetRange(i32Start, i32End - i32Start);
                }
                else if (list is T[])
                {
                    return new List<T>(iterable).GetRange(i32Start, i32End - i32Start);
                }
            }

            List<T> result = new List<T>();
            if (i32Step > 0)
            {
                for (int i = i32Start; i < i32End; i += i32Step)
                {
                    result.Add(list[i]);
                }
            }
            else
            {
                for (int i = i32Start; i > i32End; i += i32Step)
                {
                    result.Add(list[i]);
                }
            }
            return result;
        }

        public static IEnumerable<ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (IEnumerable<ScriptValue>)_this.BuildInObject;
        }

        class ScriptIterator : IEnumerator<ScriptValue>, IEnumerable<ScriptValue>
        {
            private ScriptFns currentFn;
            private ScriptFns moveNextFn;
            private ScriptFns resetFn;
            public ScriptIterator(ScriptValue current, ScriptValue moveNext, ScriptValue reset)
            {
                this.currentFn = current.TryFunction;
                this.moveNextFn = moveNext.TryFunction;
                this.resetFn = reset.TryFunction;
                if (this.currentFn == null)
                    throw new SystemException("current must be function");
                if (this.moveNextFn == null)
                    throw new SystemException("moveNext must be function");
                if (this.resetFn == null)
                    throw new SystemException("reset must be function");
            }

            public object Current => currentFn.Call((ScriptObject)null);

            ScriptValue IEnumerator<ScriptValue>.Current => currentFn.Call((ScriptObject)null);

            public void Dispose() => resetFn.Call((ScriptObject)null);

            public bool MoveNext()
            {
                var result = moveNextFn.Call((ScriptObject)null);
                var resultObj = result.TryObject;
                if (resultObj == null || !(resultObj.ClassType is ScriptBool))
                    throw new SystemException("bool return is required in move next function");
                return ScriptBool.AsCSharp(resultObj);
            }

            public void Reset() => resetFn.Call((ScriptObject)null);

            public IEnumerator<ScriptValue> GetEnumerator() => this;
            IEnumerator IEnumerable.GetEnumerator() => this;
        }

    }
}
