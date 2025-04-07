using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets
{
    public class ValueReference
    {
        private Func<ScriptValue> getter;
        private Action<ScriptValue> setter;
        private static ValueReference empty = new ValueReference((Func<ScriptValue>)null, (Action<ScriptValue>)null);
        public static ValueReference Empty => empty;

        public ValueReference(Func<ScriptValue> getter, Action<ScriptValue> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }
        public ValueReference(ScriptValue value)
        {
            this.getter = () => value;
            this.setter = x => value = x;
        }

        public bool IsEmpty => getter == null && setter == null;

        public ScriptValue Ref { get => getter(); set => setter(value); }
    }
}
