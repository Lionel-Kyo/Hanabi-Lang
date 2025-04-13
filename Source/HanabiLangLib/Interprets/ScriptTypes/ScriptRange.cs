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

            this.AddFunction("__GetSlicer__", new List<FnParameter> { new FnParameter("this"), new FnParameter("slicer", BasicTypes.List) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                Range range = AsCSharp(_this);
                List<ScriptValue> slicer = ScriptList.AsCSharp(args[1].TryObject);

                if (slicer.Count > 1)
                    throw new ArgumentException("Only 1 slicer is allowed for range");

                List<ScriptValue> slicerValues = ScriptList.AsCSharp(slicer[0].TryObject);

                long? start = slicerValues[0].IsNull ? (long?)null : ScriptInt.AsCSharp(slicerValues[0].TryObject);
                long? end = slicerValues[1].IsNull ? (long?)null : ScriptInt.AsCSharp(slicerValues[1].TryObject);
                long? step = slicerValues[2].IsNull ? (long?)null : ScriptInt.AsCSharp(slicerValues[2].TryObject);


                return new ScriptValue(BasicTypes.Range.Create(Slicer(range, start, end, step)));
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
            /*do
            {
                yield return new ScriptValue(x);
                x += step;
                if (step < 0 && x <= end || 0 < step && end <= x)
                    break;
            }
            while (true);*/
        }

        private static long CalculateLength(Range range)
        {
            if (range.Step > 0 && range.Start < range.End)
                return 1 + (range.End - 1 - range.Start) / range.Step;
            else if (range.Step < 0 && range.Start > range.End)
                return 1 + (range.Start - 1 - range.End) / (0 - range.Step);
            else
                return 0;
        }

        private static Range Slicer(Range range, long? start, long? end, long? step)
        {
            long length = range.GetLength();
            //long _step = step ?? 1;

            //if (start.HasValue)
            //{
            //    if (start < 0)
            //        start += length;
            //    if (start < 0)
            //        start = 0;
            //    if (start >= length)
            //        start = length - 1;
            //}
            //if (end.HasValue)
            //{
            //    if (end < 0)
            //        end += length;
            //    if (end < 0)
            //        end = -1;
            //    if (end > length)
            //        end = length;
            //}
            //long _start = start ?? (_step > 0 ? 0 : length - 1);
            //long _end = end ?? (_step > 0 ? length : -1);
            Range adjusted = Range.CreateAdjusted(length, start, end, step);

            long newStart = (range.Step == 1) ? (range.Start + adjusted.Start) : (range.Start + (adjusted.Start * range.Step));
            long newStop = (range.Step == 1) ? (range.Start + adjusted.End) : (range.Start + (adjusted.End * range.Step));
            long newStep = range.Step * adjusted.Step;

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

            public static Range CreateAdjusted(long length, long? start, long? end, long? step)
            {
                long _step = step.HasValue ? step.Value : 1;
                if (_step == 0)
                    throw new ArgumentException("step cannot be zero.");
                long _start;
                if (start.HasValue)
                {
                    _start = start.Value;
                    if (_start < 0)
                        _start += length;
                    if (_start < 0)
                        _start = _step < 0 ? -1 : 0;
                    if (_start >= length)
                        _start = _step < 0 ? length - 1 : length;
                }
                else
                {
                    _start = _step < 0 ? length - 1 : 0;
                }

                long _end;
                if (end.HasValue)
                {
                    _end = end.Value;
                    if (_end < 0)
                        _end += length;
                    if (_end < 0)
                        _end = _step < 0 ? -1 : 0;
                    if (_end >= length)
                        _end = _step < 0 ? length - 1 : length;
                }
                else
                {
                    _end = _step < 0 ? -1 : length;
                }

                return new Range(_start, _end, _step);
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
