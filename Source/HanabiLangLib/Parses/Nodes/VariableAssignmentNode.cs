using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class VariableAssignmentNode : AstNode, IExpressionNode
    {
        public AstNode Name { get; private set; }
        public AstNode Value { get; private set; }

        public VariableAssignmentNode(AstNode name, AstNode value)
        {
            this.Name = name;
            this.Value = value;
        }

        public VariableAssignmentNode(string name, AstNode value)
        {
            this.Name = new VariableReferenceNode(name);
            this.Value = value;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({Name} {Value})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
