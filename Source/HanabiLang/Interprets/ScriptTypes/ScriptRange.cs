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
    class ScriptRange : ScriptClass
    {
        public ScriptRange() :
            base("Range", isStatic: false)
        {
            this.AddObjectFn(this.Name, new List<FnParameter>()
            {
                new FnParameter("end", BasicTypes.Int, null),
            }, args =>
            {
                long start = 0;
                long end = 0;
                long step = 1;
                ScriptObject _this = (ScriptObject)args[0].Value;

                end = (long)((ScriptObject)args[1].Value).BuildInObject;
                if (end < 0 && step > 0)
                    step *= -1;

                _this.BuildInObject = Tuple.Create(start, end, step);
                return ScriptValue.Null;
            });

            this.AddObjectFn(this.Name, new List<FnParameter>()
            {
                new FnParameter("start", BasicTypes.Int, null),
                new FnParameter("end", BasicTypes.Int, null)
            }, args =>
            {
                long start = 0;
                long end = 0;
                long step = 1;
                ScriptObject _this = (ScriptObject)args[0].Value;

                start = (long)((ScriptObject)args[1].Value).BuildInObject;
                end = (long)((ScriptObject)args[2].Value).BuildInObject;
                if (end < 0 && step > 0)
                    step *= -1;

                if (step == 0)
                    throw new SystemException("step cannot be 0");
                _this.BuildInObject = Tuple.Create(start, end, step);
                return ScriptValue.Null;
            });

            this.AddObjectFn(this.Name, new List<FnParameter>()
            {
                new FnParameter("start", BasicTypes.Int, null),
                new FnParameter("end", BasicTypes.Int, null),
                new FnParameter("step", BasicTypes.Int, null)
            }, args =>
            {
                long start = 0;
                long end = 0;
                long step = 1;
                ScriptObject _this = (ScriptObject)args[0].Value;
                start = (long)((ScriptObject)args[1].Value).BuildInObject;
                end = (long)((ScriptObject)args[2].Value).BuildInObject;
                step = (long)((ScriptObject)args[3].Value).BuildInObject;

                if (step == 0)
                    throw new SystemException("step cannot be 0");
                _this.BuildInObject = Tuple.Create(start, end, step);
                return ScriptValue.Null;
            });

            this.AddObjectFn("GetEnumerator", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var result = BasicTypes.Enumerator.Create();
                var value = (Tuple<long, long, long>)_this.BuildInObject;
                result.BuildInObject = RangeIterator(value.Item1, value.Item2, value.Item3);
                return new ScriptValue(result);
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, Tuple.Create((long)0, (long)0, (long)0));
        public ScriptObject Create(long start, long stop, long step = 1) => 
            new ScriptObject(this, Tuple.Create(start, stop, step));

        private static IEnumerable<ScriptValue> RangeIterator(long start, long stop, long step)
        {
            long x = start;

            do
            {
                yield return new ScriptValue(x);
                x += step;
                if (step < 0 && x <= stop || 0 < step && stop <= x)
                    break;
            }
            while (true);
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
    }
}
