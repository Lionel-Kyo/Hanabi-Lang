using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class ClassDefineNode : AstNode
    {
        public string Name { get; private set; }
        public List<AstNode> Body { get; private set; }
        public List<AstNode> SuperClasses { get; private set; }
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }

        // Constructor requires a name and body
        public ClassDefineNode(string name, List<AstNode> body, List<AstNode> superClasses,
            bool isStatic, AccessibilityLevel level)
        {
            this.Name = name;
            this.Body = body;
            this.SuperClasses = superClasses;
            this.IsStatic = isStatic;
            this.Level = level;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Name);
            result.Append(' ');
            foreach (var statement in Body)
            {
                result.Append(statement.ToString());
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
