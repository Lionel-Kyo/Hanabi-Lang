using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class SwitchCaseNode : AstNode
    {
        public List<AstNode> Conditions { get; private set; }
        public List<AstNode> Body { get; private set; }

        public SwitchCaseNode(List<AstNode> conditions, List<AstNode> body)
        {
            this.Conditions = conditions;
            this.Body = body;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Conditions == null ? "default" : Conditions.ToString());
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
