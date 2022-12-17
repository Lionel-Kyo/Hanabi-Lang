using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptScript : ScriptObject
    {
        public static ScriptClass CreateBuildInClass(bool isMain, IEnumerable<string> args)
        {
            var newScrope = new ScriptScope(ScopeType.Class);
            var result = new ScriptClass("Script", null, new List<string>(),
                newScrope, true, () => new ScriptNull());
            newScrope.Variables["IsMain"] = new ScriptVariable("IsMain", new ScriptValue(isMain), true);
            List<ScriptValue> scriptArgs = new List<ScriptValue>();
            foreach (string arg in args) 
            {
                scriptArgs.Add(new ScriptValue(arg));
            }
            newScrope.Variables["Args"] = new ScriptVariable("Args", new ScriptValue(scriptArgs), true);
            return result;
        }
        public ScriptScript() : base(CreateBuildInClass(false, new string[0])) { }

        public override ScriptObject Equals(ScriptObject value)
        {
            if (value is ScriptNull)
            {
                return new ScriptBool(true);
            }
            return new ScriptBool(false);
        }

        public override ScriptStr ToStr() => new ScriptStr(this.ToString());

        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0) => "\"Script\"";

        public override string ToString() => "Script";

        public override ScriptObject Copy()
        {
            return new ScriptNull();
        }
    }
}
