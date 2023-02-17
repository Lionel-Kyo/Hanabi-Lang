using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

namespace HanabiLang.Interprets
{
    static class ImportedItems
    {
        public static Dictionary<string, Interpreter> Files = new Dictionary<string, Interpreter>();
        public static Dictionary<Type, ScriptClass> Types = new Dictionary<Type, ScriptClass>();
    }
}
