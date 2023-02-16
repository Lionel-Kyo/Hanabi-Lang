using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptVariable : ScriptType
    {
        public string Name { get; private set; }
        public ScriptValue Value { get; set; }
        public bool IsConstant { get; private set; }
        
        public ScriptVariable(string name, ScriptValue value, bool isConstant)
        {
            this.Name = name;
            this.Value = value;
            this.IsConstant = isConstant;
        }

        public override string ToString()
        {
            return $"<variable: {this.Name}>";
        }
    }
}
