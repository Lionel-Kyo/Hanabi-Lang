using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Parses;
using HanabiLang.Parses.Nodes;

namespace HanabiLang.Interprets
{
    public class BuildInFns
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
            scope.Functions["print"] = new ScriptFns("print");
            scope.Functions["print"].Fns.Add(new ScriptFn(
                new List<FnParameter>() { new FnParameter("args", multipleArguments: true) }, null, Print, true, AccessibilityLevel.Public));

            scope.Functions["println"] = new ScriptFns("println");
            scope.Functions["println"].Fns.Add(new ScriptFn(
                new List<FnParameter>() { new FnParameter("args", multipleArguments: true) }, null, Println, true, AccessibilityLevel.Public));

            scope.Functions["input"] = new ScriptFns("input");
            scope.Functions["input"].Fns.Add(new ScriptFn(
                new List<FnParameter>() { new FnParameter("args", multipleArguments: true) }, null, Input, true, AccessibilityLevel.Public));
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
