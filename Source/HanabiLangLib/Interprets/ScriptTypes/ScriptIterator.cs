using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptIterator : ScriptClass
    {
        public ScriptIterator() :
            base("Iterator", isStatic: false)
        {
            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("currentFn"),
                new FnParameter("moveNextFn"),
                new FnParameter("resetFn")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;

                foreach (var arg in args.Skip(1))
                    if (!arg.IsFunction)
                        throw new SystemException($"{arg} is not a function");

                var currentFn = args[1];
                var moveNextFn = args[2];
                var resetFn = args[3];

                _this.BuildInObject = new ScriptEnumerator(currentFn, moveNextFn, resetFn);

                return ScriptValue.Null;
            });

            AddVariable("Iter", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var result = BasicTypes.Iterator.Create(AsCSharp(_this));
                return new ScriptValue(result);
            }, null, false, null);

            this.AddFunction("All", new List<FnParameter>()
            {
                 new FnParameter("predicate")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
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
                 new FnParameter("predicate")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
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
                 new FnParameter("selector", (ScriptClass)null, new ScriptValue(), false)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
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
                 new FnParameter("second", BasicTypes.Iterator)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptObject second = args[1].TryObject;
                var result = iter.Concat(AsCSharp(second));
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("Contains", new List<FnParameter>()
            {
                 new FnParameter("value")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Contains(args[1]);
                return new ScriptValue(result);
            });

            this.AddFunction("Count", new List<FnParameter>()
            {
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Count();
                return new ScriptValue(result);
            });

            this.AddFunction("Distinct", new List<FnParameter>()
            {
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Distinct();
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("ElementAt", new List<FnParameter>()
            {
                new FnParameter("index", BasicTypes.Int)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.ElementAt((int)ScriptInt.AsCSharp(args[1].TryObject));
                return result;
            });

            this.AddFunction("Except", new List<FnParameter>()
            {
                 new FnParameter("second", BasicTypes.Iterator)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptObject second = args[1].TryObject;
                var result = iter.Except(AsCSharp(second));
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("First", new List<FnParameter>()
            {
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.First();
                return result;
            });

            //this.AddFunction("GroupBy", new List<FnParameter>()
            //{
            //     new FnParameter("predicate")
            //}, args =>
            //{
            //    TryGetIterator(args[0].TryObject, out var iter);
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
            //     new FnParameter("predicate")
            //}, args =>
            //{
            //    TryGetIterator(args[0].TryObject, out var iter);
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
                 new FnParameter("second", BasicTypes.Iterator)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptObject second = args[1].TryObject;
                var result = iter.Intersect(AsCSharp(second));
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            //this.AddFunction("Join", new List<FnParameter>()
            //{
            //     new FnParameter("predicate")
            //}, args =>
            //{
            //    TryGetIterator(args[0].TryObject, out var iter);
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
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Last();
                return result;
            });

            this.AddFunction("Max", new List<FnParameter>()
            {
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Max();
                return result;
            });

            this.AddFunction("Min", new List<FnParameter>()
            {
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Min();
                return result;
            });

            this.AddFunction("OrderBy", new List<FnParameter>()
            {
                 new FnParameter("keySelector")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                var result = iter.OrderBy(x =>
                {
                    ScriptValue callResult = keySelector.Call(null, x);
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("OrderByDescending", new List<FnParameter>()
            {
                 new FnParameter("keySelector")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                var result = iter.OrderByDescending(x =>
                {
                    ScriptValue callResult = keySelector.Call(null, x);
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("Reverse", new List<FnParameter>()
            {
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Reverse();
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("Select", new List<FnParameter>()
            {
                 new FnParameter("selector")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                IEnumerable<ScriptValue> result;
                result = iter.Select((x) =>
                {
                    ScriptValue callResult = keySelector.Call(null, x);
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("SelectWithIndex", new List<FnParameter>()
            {
                 new FnParameter("selector")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                IEnumerable<ScriptValue> result;
                result = iter.Select((x, i) =>
                {
                    ScriptValue callResult = keySelector.Call(null, x, new ScriptValue(i));
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("SelectMany", new List<FnParameter>()
            {
                 new FnParameter("selector")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                var result = iter.SelectMany(x =>
                {
                    ScriptObject callResult = keySelector.Call(null, x).TryObject;
                    return AsCSharp(callResult);
                });
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("SelectManyWithIndex", new List<FnParameter>()
            {
                 new FnParameter("selector")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                var result = iter.SelectMany((x, i) =>
                {
                    ScriptObject callResult = keySelector.Call(null, x, new ScriptValue(i)).TryObject;
                    return AsCSharp(callResult);
                });
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("SequenceEqual", new List<FnParameter>()
            {
                 new FnParameter("second", BasicTypes.Iterator)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptObject second = args[1].TryObject;
                var result = iter.SequenceEqual(AsCSharp(second));
                return new ScriptValue(result);
            });

            this.AddFunction("Single", new List<FnParameter>()
            {
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Single();
                return result;
            });

            this.AddFunction("Skip", new List<FnParameter>()
            {
                new FnParameter("count", BasicTypes.Int)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Skip((int)ScriptInt.AsCSharp(args[1].TryObject));
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("Sum", new List<FnParameter>()
            {
                 new FnParameter("selector", (ScriptClass)null, new ScriptValue(), false)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
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
                new FnParameter("count", BasicTypes.Int)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.Take((int)ScriptInt.AsCSharp(args[1].TryObject));
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("ToDict", new List<FnParameter>()
            {
                new FnParameter("keySelector"),
                new FnParameter("elementSelector")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptFns keySelector = args[1].TryFunction;
                ScriptFns elementSelector = args[2].TryFunction;
                var result = iter.ToDictionary(x => keySelector.Call(null, x), y => elementSelector.Call(null, y));
                return new ScriptValue(result);
            });

            this.AddFunction("ToList", new List<FnParameter>()
            {
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                var result = iter.ToList();
                return new ScriptValue(result);
            });

            this.AddFunction("Union", new List<FnParameter>()
            {
                 new FnParameter("second", BasicTypes.Iterator)
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptObject second = args[1].TryObject;
                var result = iter.Union(AsCSharp(second));
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("Where", new List<FnParameter>()
            {
                 new FnParameter("predicate")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptFns predicate = args[1].TryFunction;
                var result = iter.Where(x =>
                {
                    ScriptObject callResult = predicate.Call(null, x).TryObject;
                    return ScriptBool.AsCSharp(callResult);
                });
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });

            this.AddFunction("Zip", new List<FnParameter>()
            {
                new FnParameter("second", BasicTypes.Iterator),
                new FnParameter("resultSelector")
            }, args =>
            {
                TryGetIterator(args[0].TryObject, out var iter);
                ScriptObject second = args[1].TryObject;
                if (!TryGetIterator(second, out var iter2))
                    throw new SystemException("second is not Iterator");
                ScriptFns resultSelector = args[2].TryFunction;
                var result = iter.Zip(iter2, (x, y) =>
                {
                    ScriptValue callResult = resultSelector.Call(null, x, y);
                    return callResult;
                });
                return new ScriptValue(BasicTypes.Iterator.Create(result));
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, (IEnumerable<ScriptValue>)new List<ScriptValue>());
        public ScriptObject Create(IEnumerable<ScriptValue> value) => new ScriptObject(this, value);

        public static bool TryGetIterator(ScriptObject obj, out IEnumerable<ScriptValue> result)
        {
            if (obj.ClassType == BasicTypes.Iterator)
            {
                result = AsCSharp(obj);
                return true;
            }
            result = null;
            if (!obj.Scope.TryGetValue("Iter", out ScriptType iter)) 
                return false;

            if (!(iter is ScriptVariable))
                return false;

            ScriptObject iterObj;
            var getFns = ((ScriptVariable)iter).Get;
            if (getFns == null)
                iterObj = ((ScriptVariable)iter).Value.TryObject;
            else
                iterObj = getFns.Call(obj).TryObject;

            if (iterObj == null || iterObj.ClassType != BasicTypes.Iterator) 
                return false;

            result = AsCSharp(iterObj);
            return true;
        }

        public override ScriptObject ToStr(ScriptObject _this)
        {
            return BasicTypes.Str.Create($"<object: {_this.ClassType.Name}>");
        }

        public static IEnumerable<ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (IEnumerable<ScriptValue>)_this.BuildInObject;
        }

        class ScriptEnumerator : IEnumerator<ScriptValue>, IEnumerable<ScriptValue>
        {
            private ScriptFns currentFn;
            private ScriptFns moveNextFn;
            private ScriptFns resetFn;
            public ScriptEnumerator(ScriptValue current, ScriptValue moveNext, ScriptValue reset)
            {
                if (!current.IsFunction)
                    throw new SystemException("Current must be function");
                if (!moveNext.IsFunction)
                    throw new SystemException("MoveNext must be function");
                if (!reset.IsFunction)
                    throw new SystemException("Reset must be function");
                this.currentFn = (ScriptFns)current.Value;
                this.moveNextFn = (ScriptFns)moveNext.Value;
                this.resetFn = (ScriptFns)reset.Value;
            }

            public object Current => currentFn.Call(null);

            ScriptValue IEnumerator<ScriptValue>.Current => currentFn.Call(null);

            public void Dispose() => resetFn.Call(null);

            public bool MoveNext()
            {
                var result = moveNextFn.Call(null);
                var resultObj = result.TryObject;
                if (resultObj == null || !(resultObj.ClassType is ScriptBool))
                    throw new SystemException("bool return is required in move next function");
                return ScriptBool.AsCSharp(resultObj);
            }

            public void Reset() => resetFn.Call(null);

            public IEnumerator<ScriptValue> GetEnumerator() => this;
            IEnumerator IEnumerable.GetEnumerator() => this;
        }

    }
}
