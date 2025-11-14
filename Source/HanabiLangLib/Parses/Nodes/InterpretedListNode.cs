using HanabiLangLib.Interprets;
using HanabiLangLib.Interprets.ScriptTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HanabiLangLib.Parses.Nodes
{
    class InterpretedListNode : AstNode, IExpressionNode
    {
        private ScriptValue value;

        public InterpretedListNode(ScriptValue value, int pos, int line)
        {
            this.value = value;
            this.Pos = pos;
            this.Line = line;
        }

        public ScriptValue CloneValue()
        {
            return new ScriptValue(ScriptList.AsCSharp(value.TryObject).ToList());
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({value})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
