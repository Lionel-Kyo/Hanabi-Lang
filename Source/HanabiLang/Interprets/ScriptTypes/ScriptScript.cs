using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptScript : ScriptClass
    {
        public ScriptScript(bool isMain, IEnumerable<string> args) : 
            base("Script", null, new ScriptScope(ScopeType.Class), true, AccessibilityLevels.Public)
        {
            this.Scope.Variables["IsMain"] = new ScriptVariable("IsMain", new ScriptValue(isMain), true, true, AccessibilityLevels.Public);
            List<ScriptValue> scriptArgs = new List<ScriptValue>();
            foreach (string arg in args)
            {
                scriptArgs.Add(new ScriptValue(arg));
            }
            this.Scope.Variables["Args"] = new ScriptVariable("Args", new ScriptValue(scriptArgs), true, true, AccessibilityLevels.Public);
        }
    }
}
