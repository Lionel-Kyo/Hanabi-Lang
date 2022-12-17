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
    class BuildInFns
    {
        public delegate ScriptValue ScriptFnType(List<ScriptValue> parameters);
        public static Dictionary<string, ScriptFnType> Fns = new Dictionary<string, ScriptFnType>();
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
            Console.Write(GetStringByArgs(args));
            return ScriptValue.Null;
        }

        public static ScriptValue Println(List<ScriptValue> args)
        {
            Console.WriteLine(GetStringByArgs(args));
            return ScriptValue.Null;
        }

        public static ScriptValue Input(List<ScriptValue> args)
        {
            Print(args);
            return new ScriptValue(Console.ReadLine());
        }

        public static ScriptValue ParallelFor(List<ScriptValue> args)
        {
            var arg1 = args[0];
            var arg2 = args[1];
            if (!(arg1.Value is IEnumerable<ScriptValue>))
                throw new SystemException("ParallelFor should input enumerable and a function");
            if (!arg2.IsFunction)
                throw new SystemException("ParallelFor should input enumerable and a function");
            var enumerable = (IEnumerable<ScriptValue>)arg1.Value;
            ScriptFn fn = (ScriptFn)arg2.Value;
            Parallel.ForEach(enumerable, x =>
            {
                fn.Call(fn.Scope, x);
            });
            Print(args);
            return ScriptValue.Null;
        }

        public static void AddBasicFunctions()
        {
            Fns["print"] = Print;
            Fns["println"] = Println;
            Fns["input"] = Input;
            //Fns["ParallelFor"] = ParallelFor;
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
