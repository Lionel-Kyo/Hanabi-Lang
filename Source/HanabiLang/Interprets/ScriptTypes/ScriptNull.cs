using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptNull : ScriptObject
    {
        public static ScriptClass CreateBuildInClass()
        {
            var newScrope = new ScriptScope(ScopeType.Class);
            var result  = new ScriptClass("null", null, new List<string>(),
                newScrope, false, () => new ScriptNull());
            return result;
        }
        public ScriptNull() : base(CreateBuildInClass()) { }

        public override ScriptObject Equals(ScriptObject value)
        {
            if (value is ScriptNull)
            {
                return new ScriptBool(true);
            }
            return new ScriptBool(false);
        }

        public override ScriptStr ToStr() => new ScriptStr(this.ToString());
        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0) => "null";
        public override string ToString() => "null";

        public override ScriptObject Copy()
        {
            return new ScriptNull();
        }

        /*public override int GetHashCode()
        {
            return "null".GetHashCode();
        }*/
    }
}
