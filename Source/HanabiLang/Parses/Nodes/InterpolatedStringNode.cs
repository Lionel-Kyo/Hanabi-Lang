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
        public Queue<AstNode> InterpolatedNodes { get; private set; }

        public InterpolatedString(List<string> values, Queue<AstNode> interpolatedNodes)
        {
            this.Values = values;
            this.InterpolatedNodes = interpolatedNodes;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({Values})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
