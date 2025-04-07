using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptObjectClass : ScriptClass
    {
        public ScriptObjectClass():
            base("object", isStatic: false)
        {
            AddFunction("ToStr", new List<FnParameter>() { new FnParameter("this"), },
                args =>
                {
                    ScriptObject _this = (ScriptObject)args[0].Value;
                    return new ScriptValue(_this.ClassType.ToStr(_this));
                });
        }
    }
}
