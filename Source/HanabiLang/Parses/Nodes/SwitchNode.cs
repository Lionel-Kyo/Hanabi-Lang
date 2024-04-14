using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class SwitchNode : AstNode, IStatementNode
    {
        public AstNode Condition { get; private set; }
        public List<SwitchCaseNode> Cases { get; private set; }
        public SwitchCaseNode DefaultCase { get; private set; }

        public SwitchNode(AstNode condition, List<SwitchCaseNode> cases, SwitchCaseNode defaultCase)
        {
            this.Condition = condition;
            this.Cases = cases;
            this.DefaultCase = defaultCase;

        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Condition.ToString());
            result.Append(' ');
            foreach (var statement in Cases)
            {
                result.Append(statement.ToString());
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
