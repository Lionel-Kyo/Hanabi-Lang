using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class NullCoalescingNode : AstNode, IExpressionNode
    {
        public AstNode Value { get; private set; }
        public AstNode Consequent { get; private set; }

        public NullCoalescingNode(AstNode condition, AstNode consequent)
        {
            this.Value = condition;
            this.Consequent = consequent;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Value.ToString());
            result.Append(' ');
            result.Append(Consequent.ToString());
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
