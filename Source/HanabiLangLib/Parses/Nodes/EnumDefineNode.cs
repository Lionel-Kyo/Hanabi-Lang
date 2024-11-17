using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class EnumDefineNode : AstNode, IStatementNode
    {
        public string Name { get; private set; }
        public Dictionary<string, AstNode> Members { get; private set; }
        public AccessibilityLevel Level { get; private set; }

        public EnumDefineNode(string name, Dictionary<string, AstNode> members, AccessibilityLevel level)
        {
            this.Name = name;
            this.Members = members;
            this.Level = level;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Name);
            result.Append(' ');
            foreach (var member in Members)
            {
                result.Append($"{member.Key}: {member.Value}");
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
