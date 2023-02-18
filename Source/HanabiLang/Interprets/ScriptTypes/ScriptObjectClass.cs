using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptObjectClass : ScriptClass
    {
        public ScriptObjectClass():
            base("object", null, null, null, false, AccessibilityLevel.Public)
        {
            AddObjectFn("ToStr", new List<FnParameter>(),
                args => new ScriptValue(ToStr((ScriptObject)args[0].Value)));
        }
    }
}
