using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class FloatNode : AstNode, IExpressionNode
    {
        public double Value { get; private set; }

        public FloatNode(double value)
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
