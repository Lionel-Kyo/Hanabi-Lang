using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class UnaryNode : AstNode, IExpressionNode
    {
        public AstNode Node { get; private set; }
        public string Operator { get; private set; }

        public UnaryNode(AstNode child, string _operator)
        {
            this.Node = child;
            this.Operator = _operator;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({Operator} {Node})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
