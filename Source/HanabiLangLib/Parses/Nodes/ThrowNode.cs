using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class ThrowNode : AstNode, IStatementNode
    {
        public AstNode Value { get; private set; }

        public ThrowNode(AstNode value, int pos, int line)
        {
            this.Value = value;
            this.Pos = pos;
            this.Line = line;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append(' ');
            result.Append(Value == null ? "null" : Value.ToString());
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
