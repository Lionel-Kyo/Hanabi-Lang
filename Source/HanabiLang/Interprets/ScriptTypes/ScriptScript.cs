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
            base("Script", isStatic: true)
        {
            this.Scope.Variables["IsMain"] = new ScriptVariable("IsMain", new ScriptValue(BasicTypes.Bool.Create(isMain)), true, true, AccessibilityLevel.Public);
            List<ScriptValue> scriptArgs = args.Select(x => new ScriptValue(x)).ToList();
            this.Scope.Variables["Args"] = new ScriptVariable("Args", new ScriptValue(scriptArgs), true, true, AccessibilityLevel.Public);
        }
    }
}
