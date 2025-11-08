using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HanabiLangLib.Interprets.ScriptTypes;

namespace HanabiLangLib.Interprets
{
    public static class ImportedItems
    {
        public static Dictionary<string, Tuple<DateTime, Interpreter>> Files = new Dictionary<string, Tuple<DateTime, Interpreter>>();
        public static Dictionary<Type, ScriptClass> Types = new Dictionary<Type, ScriptClass>();
        public static Dictionary<Assembly, ScriptClass> Assemblies = new Dictionary<Assembly, ScriptClass>();
    }
}
