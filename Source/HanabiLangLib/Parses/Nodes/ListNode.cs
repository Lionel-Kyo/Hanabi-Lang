using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class ListNode : AstNode, IExpressionNode
    {
        public List<AstNode> Elements { get; private set; }

        public ListNode(List<AstNode> elements)
        {
            this.Elements = elements;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            foreach (var element in Elements)
            {
                result.Append(element.ToString());
                result.Append(", ");
            }
            if (Elements.Count != 0)
            {
                result.Remove(result.Length - 2, 2);
            }
            result.Append(')');
            //result.Append("  ");
            return result.ToString();
        }
    }
}
