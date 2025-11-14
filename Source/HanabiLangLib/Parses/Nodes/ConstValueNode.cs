using HanabiLangLib.Interprets;
using System;
using System.Collections.Generic;
using System.Text;

namespace HanabiLangLib.Parses.Nodes
{
    class ConstValueNode : AstNode, IExpressionNode
    {
        public ScriptValue Value { get; private set; }

        public ConstValueNode(ScriptValue value, int pos, int line)
        {
            this.Value = value;
            this.Pos = pos;
            this.Line = line;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({Value})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
