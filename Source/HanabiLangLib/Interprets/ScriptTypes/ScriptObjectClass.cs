using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptObjectClass : ScriptClass
    {
        public ScriptObjectClass():
            base("object", isStatic: false)
        {
            this.AddFunction(OPEARTOR_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                return new ScriptValue(object.ReferenceEquals(args[0], args[1]));
            });
            AddFunction("ToStr", new List<FnParameter>() { new FnParameter("this"), },
            args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue($"<object: {_this.ClassType.Name}>");
            });
        }
    }
}
