using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class VariableAssignmentNode : AstNode, IExpressionNode
    {
        public List<AstNode> References { get; private set; }
        public AstNode Value { get; private set; }

        public VariableAssignmentNode(List<AstNode> refs, AstNode value)
        {
            this.References = refs;
            this.Value = value;
        }

        public VariableAssignmentNode(AstNode _ref, AstNode value)
        {
            this.References = new List<AstNode>() { _ref };
            this.Value = value;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({string.Join(", ", References)} {Value})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
