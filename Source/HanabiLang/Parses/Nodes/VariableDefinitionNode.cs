using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class VariableDefinitionNode : AstNode
    {
        public string Name { get; private set; }
        public AstNode Value { get; private set; }
        public AstNode DataType { get; private set; }
        public bool IsConstant { get; private set; }

        public VariableDefinitionNode(string name, AstNode value, AstNode dataType, bool isConstant)
        {
            this.Name = name;
            this.Value = value;
            this.DataType = dataType;
            this.IsConstant = isConstant;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({Name} {Value} Type: {DataType} const: {IsConstant})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
