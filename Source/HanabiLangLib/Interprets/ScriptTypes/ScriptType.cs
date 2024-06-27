using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    public abstract class ScriptType
    {

    }

    public class BreakType: ScriptType { }
    public class ContinueType: ScriptType { }

    public class DefinedTypes: ScriptType 
    {
        public HashSet<ScriptClass> Value { get; private set; }
        public DefinedTypes(HashSet<ScriptClass> types) 
        { 
            this.Value = types;
        }
    }
}
