using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class InterpolatedStringNode : AstNode, IExpressionNode
    {
        public List<string> Texts { get; private set; }
        private Queue<AstNode> interpolatedNodes;

        public InterpolatedStringNode(List<string> values, Queue<AstNode> interpolatedNodes, int pos, int line)
        {
            this.Texts = values;
            this.interpolatedNodes = interpolatedNodes;
            this.Pos = pos;
            this.Line = line;
        }

        public Queue<AstNode> CloneInterpolatedNodes()
        {
            return new Queue<AstNode>(interpolatedNodes);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}(");
            foreach(var value in Texts)
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
