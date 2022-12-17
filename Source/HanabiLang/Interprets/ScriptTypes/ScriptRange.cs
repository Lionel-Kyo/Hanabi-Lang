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
    class ScriptRange : ScriptObject, IEnumerable<ScriptValue>
    {
        public static ScriptClass CreateBuildInClass()
        {
            var newScrope = new ScriptScope(ScopeType.Class);
            return new ScriptClass("range", null, new List<string>(),
                newScrope, false, () => new ScriptRange());
        }
        public long Start { get; private set; }
        public long End { get; private set; }
        public long Step { get; private set; }
        public ScriptRange() :
            base(CreateBuildInClass())
        {
            this.Start = 0;
            this.End = 0;
            this.Step = 1;
            this.AddObjectFn(this.ObjectClass.Name, args =>
            {
                if (args.Count == 1)
                {
                    this.End = ((ScriptInt)args[0].Value).Value;
                    if (this.End < 0 && this.Step > 0)
                        this.Step *= -1;
                }
                else if (args.Count == 2)
                {
                    this.Start = ((ScriptInt)args[0].Value).Value;
                    this.End = ((ScriptInt)args[1].Value).Value;
                    if (this.End < 0 && this.Step > 0)
                        this.Step *= -1;
                }
                else if (args.Count == 3)
                {
                    this.Start = ((ScriptInt)args[0].Value).Value;
                    this.End = ((ScriptInt)args[1].Value).Value;
                    this.Step = ((ScriptInt)args[2].Value).Value;
                }

                if (this.Step == 0)
                    throw new SystemException("step cannot be 0");
                return ScriptValue.Null;
            });
        }
        public ScriptRange(long start, long stop, long step = 1) : this()
        {
            this.Start = start;
            this.End = stop;
            this.Step = step;
        }

        public override ScriptObject Equals(ScriptObject value)
        {
            if (value is ScriptRange)
            {
                ScriptRange obj = (ScriptRange)value;
                return new ScriptBool(this.Start == obj.Start && this.End == obj.End && this.Step == obj.Step);
            }
            return new ScriptBool(false);
        }

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

        public IEnumerator<ScriptValue> GetEnumerator()
        {
            return RangeIterator(this.Start, this.End, this.Step).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return RangeIterator(this.Start, this.End, this.Step).GetEnumerator();
        }

        public override ScriptStr ToStr() => new ScriptStr(this.ToString());
        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0)
        {
            return this.ToString();
        }
        public override string ToString() => $"range({this.Start}, {this.End}, {this.Step})";

        /*public override int GetHashCode()
        {
            return $"{this.Start}{this.End}{this.Step}".GetHashCode();
        }*/
    }
}
