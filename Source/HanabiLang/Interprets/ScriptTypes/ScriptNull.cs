using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptNull : ScriptClass
    {
        public ScriptNull() :
            base("bool", null, null, BasicTypes.ObjectClass, false, AccessibilityLevel.Public)
        {

        }

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptNull)
            {
                return ScriptBool.True;
            }
            return ScriptBool.False;
        }

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create("null");
        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0) => "null";
    }
}
