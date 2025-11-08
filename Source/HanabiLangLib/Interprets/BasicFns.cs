using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLangLib.Interprets.ScriptTypes;
using HanabiLangLib.Parses;
using HanabiLangLib.Parses.Nodes;

namespace HanabiLangLib.Interprets
{
    public class BasicFns
    {
        public delegate ScriptValue ScriptFnType(List<ScriptValue> parameters);
        //public static Dictionary<string, ScriptFnType> Fns = new Dictionary<string, ScriptFnType>();
        private static string GetStringByArgs(List<ScriptValue> args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in args)
            {
                stringBuilder.Append(item.Value.ToString());
                stringBuilder.Append(' ');
            }
            if (stringBuilder.Length > 1)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }
        public static ScriptValue Print(List<ScriptValue> args)
        {
            args = (List<ScriptValue>)((ScriptObject)args[0].Value).BuildInObject;
            Console.Write(GetStringByArgs(args));
            return ScriptValue.Null;
        }

        public static ScriptValue Println(List<ScriptValue> args)
        {
            args = (List<ScriptValue>)((ScriptObject)args[0].Value).BuildInObject;
            Console.WriteLine(GetStringByArgs(args));
            return ScriptValue.Null;
        }

        public static ScriptValue Input(List<ScriptValue> args)
        {
            Print(args);
            return new ScriptValue(Console.ReadLine());
        }

        internal static void AddBasicFunctions(ScriptScope scope)
        {
            var print = new ScriptFns("print");
            print.Fns.Add(new ScriptFn(
                new List<FnParameter>() { new FnParameter("args", multipleArguments: true) }, null, Print, true, AccessibilityLevel.Public));
            scope.Variables["print"] = new ScriptVariable("print", print, AccessibilityLevel.Public);

            var println = new ScriptFns("println");
            println.Fns.Add(new ScriptFn(
                new List<FnParameter>() { new FnParameter("args", multipleArguments: true) }, null, Println, true, AccessibilityLevel.Public));
            scope.Variables["println"] = new ScriptVariable("println", println, AccessibilityLevel.Public);

            var input = new ScriptFns("input");
            input.Fns.Add(new ScriptFn(
                new List<FnParameter>() { new FnParameter("args", multipleArguments: true) }, null, Input, true, AccessibilityLevel.Public));
            scope.Variables["input"] = new ScriptVariable("input", input, AccessibilityLevel.Public);
        }

        public static List<FnParameter> GetBuildInFnParams(ScriptFnType fn)
        {
            var result = new List<FnParameter>();
            foreach (var parm in fn.Method.GetParameters())
            {
                result.Add(new FnParameter(parm.Name));
            }
            return result;
        }

    }
}
