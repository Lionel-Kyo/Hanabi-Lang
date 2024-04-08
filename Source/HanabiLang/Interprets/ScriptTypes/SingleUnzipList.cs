using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class SingleUnzipList : ScriptType
    {
        public List<ScriptValue> Value { get; private set; }
        public SingleUnzipList(IEnumerable<ScriptValue> value) 
        { 
            this.Value = value.ToList();
        }
    }
}
