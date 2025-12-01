using HanabiLangLib.Interprets;
using HanabiLangLib.Interprets.ScriptTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace HanabiLangLib.Parses.Nodes
{
    abstract class InterpretedNode : AstNode, IExpressionNode
    {
        public ScriptValue Value { get; private set; }

        public InterpretedNode(ScriptValue value, int pos, int line)
        {
            this.Value = value;
            this.Pos = pos;
            this.Line = line;
        }

        public abstract ScriptValue CloneValue();

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({Value})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
