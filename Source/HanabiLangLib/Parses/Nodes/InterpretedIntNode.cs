using HanabiLangLib.Interprets;
using HanabiLangLib.Interprets.ScriptTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HanabiLangLib.Parses.Nodes
{
    class InterpretedIntNode : InterpretedNode
    {

        public InterpretedIntNode(ScriptValue value, int pos, int line) : base(value, pos, line) 
        {
        }

        public override ScriptValue CloneValue()
        {
            return new ScriptValue(ScriptInt.AsCSharp(this.Value.TryObject));
        }
    }
}
