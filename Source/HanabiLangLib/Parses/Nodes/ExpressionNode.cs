using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class ExpressionNode : AstNode, IExpressionNode
    {
        public AstNode Left { get; private set; }
        public AstNode Right { get; private set; }
        public string Operator { get; private set; }

        public ExpressionNode(AstNode left, AstNode right, string _operator)
        {
            this.Left = left;
            this.Right = right;
            this.Operator = _operator;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName} ({Left} {Operator} {Right})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
