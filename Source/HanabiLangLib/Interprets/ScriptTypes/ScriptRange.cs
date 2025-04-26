using HanabiLang.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static HanabiLang.Interprets.ScriptTypes.ScriptRange;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptRange : ScriptClass
    {
        public ScriptRange() :
            base("Range", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
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

        private static long Index(Range range, long index)
        {
            return range.Start + range.Step * index;
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

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptRange)
            {
                var a = AsCSharp(_this);
                var b = AsCSharp(value);
                return BasicTypes.Bool.Create(a.Start == b.Start && a.End == b.End && a.Step == b.Step);
            }
            return BasicTypes.Bool.Create(false);
        }

        public override ScriptObject ToStr(ScriptObject _this)
        {
            var value = AsCSharp(_this);
            return BasicTypes.Str.Create($"range({value.Start}, {value.End}, {value.Step})");
        }

        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            var value = (Range)_this.BuildInObject;
            return $"[{value.Start}, {value.End}, {value.Step}]";
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
