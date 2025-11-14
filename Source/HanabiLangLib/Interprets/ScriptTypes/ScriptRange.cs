using HanabiLangLib.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static HanabiLangLib.Interprets.ScriptTypes.ScriptRange;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptRange : ScriptClass
    {
        public ScriptRange() :
            base("Range", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
            this.InitializeOperators();

            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("end", BasicTypes.Int, null),
            }, args =>
            {
                long start = 0;
                long end = 0;
                long step = 1;
                ScriptObject _this = args[0].TryObject;

                end = ScriptInt.AsCSharp(args[1].TryObject);

                _this.BuildInObject = new Range(start, end, step);
                return ScriptValue.Null;
            });

            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("start", BasicTypes.Int, null),
                new FnParameter("end", BasicTypes.Int, null)
            }, args =>
            {
                long start = 0;
                long end = 0;
                long step = 1;
                ScriptObject _this = args[0].TryObject;

                start = ScriptInt.AsCSharp(args[1].TryObject);
                end = ScriptInt.AsCSharp(args[2].TryObject);
                if (start > end && step > 0)
                    step = -1;

                _this.BuildInObject = new Range(start, end, step);
                return ScriptValue.Null;
            });

            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("start", BasicTypes.Int, null),
                new FnParameter("end", BasicTypes.Int, null),
                new FnParameter("step", BasicTypes.Int, null)
            }, args =>
            {
                long start = 0;
                long end = 0;
                long step = 1;
                ScriptObject _this = args[0].TryObject;
                start = ScriptInt.AsCSharp(args[1].TryObject);
                end = ScriptInt.AsCSharp(args[2].TryObject);
                step = ScriptInt.AsCSharp(args[3].TryObject);

                if (step == 0)
                    throw new ArgumentException("step cannot be zero.");

                _this.BuildInObject = new Range(start, end, step);
                return ScriptValue.Null;
            });

            AddVariable("Iter", args =>
            {
                var range = AsCSharp(args[0].TryObject);
                var result = BasicTypes.Iterable.Create(RangeIterable(range.Start, range.End, range.Step));
                return new ScriptValue(result);
            }, null, false, null);

            AddVariable("Length", args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(AsCSharp(_this).GetLength());
            }, null, false, null);

            this.AddFunction("__GetIndexer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("indexes", BasicTypes.List) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                Range range = AsCSharp(_this);
                List<ScriptValue> indexes = ScriptList.AsCSharp(args[1].TryObject);

                if (indexes.Count > 1)
                    throw new ArgumentException("Only 1 indexer is allowed for range");

                var indexObj = indexes[0].TryObject;

                if (indexObj?.ClassType == BasicTypes.Slice)
                {
                    var slice = ScriptSlice.AsCSharp(indexes[0].TryObject);

                    return new ScriptValue(BasicTypes.Range.Create(Slice(range, slice.Start, slice.End, slice.Step)));
                }

                if (indexObj?.ClassType != BasicTypes.Int)
                    throw new ArgumentException("Only int/slice is allowed for range indexer");

                long index = ScriptInt.AsCSharp(indexObj);

                return new ScriptValue(Index(range, index));
            });

            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var range = AsCSharp(_this);
                return new ScriptValue(BasicTypes.Str.Create($"range({range.Start}, {range.End}, {range.Step})"));
            });
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
            if (_other.IsTypeOrSubOf(BasicTypes.Range))
            {
                if (object.ReferenceEquals(_this, _other))
                    return true;

                var a = AsCSharp(_this);
                var b = AsCSharp(_other);
                return a.Start == b.Start && a.End == b.End && a.Step == b.Step;
            }
            return false;
        }

        public override ScriptObject Create() => new ScriptObject(this, new Range(0, 0, 0));

        public ScriptObject Create(Range range) => new ScriptObject(this, range);

        private static IEnumerable<ScriptValue> RangeIterable(long start, long end, long step)
        {
            long x = start;
            while (true)
            {
                if ((step < 0 && x <= end) || (step > 0 && x >= end))
                    break;
                yield return new ScriptValue(x);
                x += step;
            }
        }

        public static long GetModuloIndex(long index, long length)
        {
            if ((index >= length) || index < 0 && index < (length * -1))
                throw new IndexOutOfRangeException();

            return ScriptInt.Modulo(index, length);
        }

        private static long Index(Range range, long index)
        {
            return range.Start + range.Step * GetModuloIndex(index, range.GetLength());
        }

        private static Range Slice(Range range, long? start, long? end, long? step)
        {
            long length = range.GetLength();
            var adjusted = ScriptSlice.Slice.FillNullValues(length, start, end, step);

            long newStart = (range.Step == 1) ? (range.Start + adjusted.Start.Value) : (range.Start + (adjusted.Start.Value * range.Step));
            long newStop = (range.Step == 1) ? (range.Start + adjusted.End.Value) : (range.Start + (adjusted.End.Value * range.Step));
            long newStep = range.Step * adjusted.Step.Value;

            return new Range(newStart, newStop, newStep);
        }

        public static Range AsCSharp(ScriptObject _this)
        {
            return (Range)_this.BuildInObject;
        }

        public class Range
        {
            public long Start;
            public long End;
            public long Step;

            public Range(long start, long end, long step)
            {
                this.Start = start;
                this.End = end;
                this.Step = step;
            }

            public long GetLength()
            {
                if (this.Step > 0 && this.Start < this.End)
                    return 1 + (this.End - 1 - this.Start) / this.Step;
                else if (this.Step < 0 && this.Start > this.End)
                    return 1 + (this.Start - 1 - this.End) / (-this.Step);
                else
                    return 0;
            }
        }
    }
}
