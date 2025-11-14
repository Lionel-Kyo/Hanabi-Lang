using HanabiLangLib.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptNull : ScriptClass
    {
        public ScriptNull() :
            base("null", isStatic: false)
        {
            this.AddFunction(OPEARTOR_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                if (_other.IsTypeOrSubOf(BasicTypes.Null))
                    return new ScriptValue(true);
                return new ScriptValue(false);
            });
            this.AddFunction(OPEARTOR_NOT_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject _other = args[1].TryObject;
                if (_other.IsTypeOrSubOf(BasicTypes.Null))
                    return new ScriptValue(false);
                return new ScriptValue(true);
            });

            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this"),
            }, args =>
            {
                return new ScriptValue(BasicTypes.Str.Create("null"));
            });
        }
    }
}
