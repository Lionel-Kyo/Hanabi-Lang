using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class TernaryNode : AstNode, IExpressionNode
    {
        public AstNode Condition { get; private set; }
        public AstNode Consequent { get; private set; }
        public AstNode Alternative { get; private set; }

        public TernaryNode(AstNode condition, AstNode consequent, AstNode alternative)
        {
            this.Condition = condition;
            this.Consequent = consequent;
            this.Alternative = alternative;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Condition.ToString());
            result.Append(' ');
            result.Append(Consequent.ToString());
            result.Append(' ');
            result.Append(Alternative.ToString());
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
