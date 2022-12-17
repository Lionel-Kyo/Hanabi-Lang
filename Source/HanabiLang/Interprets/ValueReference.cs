using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets
{
    class ValueReference
    {
        private Func<ScriptValue> Getter;
        private Action<ScriptValue> Setter;
        public ValueReference(Func<ScriptValue> getter, Action<ScriptValue> setter)
        {
            this.Getter = getter;
            this.Setter = setter;
        }
        public ValueReference(ScriptValue value)
        {
            this.Getter = () => value;
            this.Setter = x => value = x;
        }

        public ScriptValue Ref { get => Getter(); set => Setter(value); }
    }
}
