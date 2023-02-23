using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class InterpolatedString : AstNode
    {
        public List<string> Values { get; private set; }
        private Queue<AstNode> interpolatedNodes;
        public Queue<AstNode> InterpolatedNodes => new Queue<AstNode>(interpolatedNodes);

        public InterpolatedString(List<string> values, Queue<AstNode> interpolatedNodes)
        {
            this.Values = values;
            this.interpolatedNodes = interpolatedNodes;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}(");
            foreach(var value in Values)
            {
                if (value == null)
                    result.Append("{}");
                else
                    result.Append($"{value} ");
            }
            result.Append(")");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
