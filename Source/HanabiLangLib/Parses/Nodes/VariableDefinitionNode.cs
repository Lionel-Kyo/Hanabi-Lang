using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class VariableDefinitionNode : AstNode, IStatementNode
    {
        public List<string> Names { get; private set; }
        public AstNode Value { get; private set; }
        public AstNode DataType { get; private set; }
        public FnDefineNode GetFn { get; private set; }
        public FnDefineNode SetFn { get; private set; }
        public bool IsConstant { get; private set; }
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }

        public VariableDefinitionNode(List<string> name, AstNode value, AstNode dataType,
            FnDefineNode getFn, FnDefineNode setFn, bool isConstant, bool isStatic, AccessibilityLevel level)
        {
            this.Names = name;
            this.Value = value;
            this.GetFn = getFn;
            this.SetFn = setFn;
            this.DataType = dataType;
            this.IsConstant = isConstant;
            this.IsStatic = isConstant ? true : isStatic;
            this.Level = level;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({string.Join(", ", Names)} {Value} Type: {DataType} const: {IsConstant})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
