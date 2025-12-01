using HanabiLangLib.Interprets;
using HanabiLangLib.Interprets.ScriptTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HanabiLangLib.Parses.Nodes
{
    class InterpretedDictNode : InterpretedNode
    {

        public InterpretedDictNode(ScriptValue value, int pos, int line) : base(value, pos, line) 
        {
        }

        public override ScriptValue CloneValue()
        {
            return new ScriptValue(ScriptDict.AsCSharp(this.Value.TryObject).ToDictionary(kv => kv.Key, kv => kv.Value));
        }
    }
}
