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
    public class ScriptSlice : ScriptClass
    {
        public ScriptSlice() :
            base("Slice", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("start", new HashSet<ScriptClass>(){ BasicTypes.Int, BasicTypes.Null }, null),
                new FnParameter("end", new HashSet<ScriptClass>(){ BasicTypes.Int, BasicTypes.Null }, null),
                new FnParameter("step", new HashSet < ScriptClass >() { BasicTypes.Int, BasicTypes.Null }, null)
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                long? start = args[1].IsNull ? (long?)null : ScriptInt.AsCSharp(args[1].TryObject);
                long? end = args[2].IsNull ? (long?)null : ScriptInt.AsCSharp(args[2].TryObject);
                long? step = args[3].IsNull ? (long?)null : ScriptInt.AsCSharp(args[3].TryObject);

                if (step == 0)
                    throw new ArgumentException("step cannot be zero.");

                _this.BuildInObject = new Slice(start, end, step);
                return ScriptValue.Null;
            });

            this.AddFunction("FillNull", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("length", BasicTypes.Int, null),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                var slice = AsCSharp(_this);
                long length = ScriptInt.AsCSharp(args[1].TryObject);

                return new ScriptValue(Create(Slice.FillNullValues(length, slice.Start, slice.End, slice.Step)));
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, new Slice(null, null, null));

        public ScriptObject Create(Slice range) => new ScriptObject(this, range);

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptSlice)
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
            return BasicTypes.Str.Create($"slice({value.Start?.ToString() ?? "null"}, {value.End?.ToString() ?? "null"}, {value.Step?.ToString() ?? "null"})");
        }

        public static Slice AsCSharp(ScriptObject _this)
        {
            return (Slice)_this.BuildInObject;
        }

        public class Slice
        {
            public long? Start;
            public long? End;
            public long? Step;

            public Slice(long? start, long? end, long? step)
            {
                this.Start = start;
                this.End = end;
                this.Step = step;
            }

            public static Slice FillNullValues(long length, long? start, long? end, long? step)
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

                return new Slice(_start, _end, _step);
            }
        }
    }
}
