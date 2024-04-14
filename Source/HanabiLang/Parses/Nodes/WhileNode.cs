using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class WhileNode : AstNode, IStatementNode
    {
        public AstNode Condition { get; private set; }
        public List<AstNode> Body { get; private set; }

        public WhileNode(AstNode condition, List<AstNode> body)
        {
            this.Condition = condition;
            this.Body = body;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Condition.ToString());
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
