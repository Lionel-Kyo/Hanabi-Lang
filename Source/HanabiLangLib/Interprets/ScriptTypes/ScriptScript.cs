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
            this.Scope.Variables["IsMain"] = new ScriptVariable("IsMain", null, new ScriptValue(BasicTypes.Bool.Create(isMain)), true, true, AccessibilityLevel.Public);
            List<ScriptValue> scriptArgs = args.Select(x => new ScriptValue(x)).ToList();
            this.Scope.Variables["Args"] = new ScriptVariable("Args", null, new ScriptValue(scriptArgs), true, true, AccessibilityLevel.Public);
            this.Scope.Variables["ExitCode"] = new ScriptVariable("ExitCode", null, 
                new ScriptFns("get_ExitCode", new ScriptFn(new List<FnParameter>(), null, _args =>
                {
                    return new ScriptValue(Environment.ExitCode);
                }, true, AccessibilityLevel.Public)),
                new ScriptFns("set_ExitCode", new ScriptFn(new List<FnParameter>{ new FnParameter("value", BasicTypes.Int) }, null, _args =>
                {
                    long value = ScriptInt.AsCSharp(_args[0].TryObject);
                    Environment.ExitCode = ScriptInt.ValidateToInt32(value);
                    return new ScriptValue();
                }, true, AccessibilityLevel.Public))
                , false, true, AccessibilityLevel.Public);
            this.Scope.Variables["Exit"] = new ScriptVariable("Exit", new ScriptFns("Exit", new ScriptFn(new List<FnParameter> { new FnParameter("exitCode", BasicTypes.Int, new ScriptValue(Environment.ExitCode)) }, null, 
                _args =>
                {
                    long exitCode = ScriptInt.AsCSharp(_args[0].TryObject);
                    Environment.Exit(ScriptInt.ValidateToInt32(exitCode));
                    return ScriptValue.Null;
                }, true, AccessibilityLevel.Public)), AccessibilityLevel.Public);
        }
    }
}
