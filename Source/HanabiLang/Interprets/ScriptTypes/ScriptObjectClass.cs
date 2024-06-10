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
            base("object", isStatic: false)
        {
            AddObjectFn("ToStr", new List<FnParameter>(),
                args =>
                {
                    ScriptObject _this = (ScriptObject)args[0].Value;
                    return new ScriptValue(_this.ClassType.ToStr(_this));
                });

            //AddVariable("Type", args =>
            //{
            //    ScriptObject _this = (ScriptObject)args[0].Value;
            //    return new ScriptValue(_this.ClassType);
            //}, null, false, null);
        }
    }
}
