using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

namespace HanabiLang.Interprets
{
    public static class BuildInImports
    {
        public static Dictionary<string, ScriptClass> Classes = new Dictionary<string, ScriptClass>();

        static BuildInImports()
        {
            Classes["Function"] = BasicTypes.FunctionClass;
            Classes["Type"] = BasicTypes.TypeClass;
            Classes["decimal"] = BasicTypes.Decimal;
            Classes["Iterable"] = BasicTypes.Iterable;
            Classes["FnEvent"] = BasicTypes.FnEvent;
            Classes["Json5"] = BasicTypes.Json5;
            Classes["DateTime"] = BasicTypes.DateTime;
        }
    }
}
