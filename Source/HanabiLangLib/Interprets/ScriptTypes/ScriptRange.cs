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
    public class ScriptRange : ScriptClass
    {
        public ScriptRange() :
            base("Range", new List<ScriptClass> { BasicTypes.Iterator }, isStatic: false)
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

                _this.BuildInObject = Tuple.Create(start, end, step);
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

                _this.BuildInObject = Tuple.Create(start, end, step);
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

                _this.BuildInObject = Tuple.Create(start, end, step);
                return ScriptValue.Null;
            });

            AddVariable("Iter", args =>
            {
                var range = AsCSharp(args[0].TryObject);
                var result = BasicTypes.Iterator.Create(RangeIterator(range.Item1, range.Item2, range.Item3));
                return new ScriptValue(result);
            }, null, false, null);
        }

        public override ScriptObject Create() => new ScriptObject(this, Tuple.Create((long)0, (long)0, (long)0));
        public ScriptObject Create(long start, long stop, long step = 1) => 
            new ScriptObject(this, Tuple.Create(start, stop, step));

        private static IEnumerable<ScriptValue> RangeIterator(long start, long stop, long step)
        {
            long x = start;
            while (true)
            {
                if ((step < 0 && x <= stop) || (step > 0 && x >= stop))
                    break;
                yield return new ScriptValue(x);
                x += step;
            }
            /*do
            {
                yield return new ScriptValue(x);
                x += step;
                if (step < 0 && x <= stop || 0 < step && stop <= x)
                    break;
            }
            while (true);*/
        }

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptRange)
            {
                var a = (Tuple<long, long, long>)_this.BuildInObject;
                var b = (Tuple<long, long, long>)value.BuildInObject;
                return BasicTypes.Bool.Create(a.Item1 == b.Item1 && a.Item2 == b.Item2 && a.Item3 == b.Item3);
            }
            return BasicTypes.Bool.Create(false);
        }

        public override ScriptObject ToStr(ScriptObject _this)
        {
            var value = (Tuple<long, long, long>)_this.BuildInObject;
            return BasicTypes.Str.Create($"range({value.Item1}, {value.Item2}, {value.Item3})");
        }

        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            var value = (Tuple<long, long, long>)_this.BuildInObject;
            return $"[{value.Item1}, {value.Item2}, {value.Item3}]";
        }

        public static Tuple<long, long, long> AsCSharp(ScriptObject _this)
        {
            return (Tuple<long, long, long>)_this.BuildInObject;
        }
    }
}
