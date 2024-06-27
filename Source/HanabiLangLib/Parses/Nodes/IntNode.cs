using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class IntNode : AstNode, IExpressionNode
    {
        public long Value { get; private set; }

        public IntNode(long value)
        {
            this.Value = value;
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
