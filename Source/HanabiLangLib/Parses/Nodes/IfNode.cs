using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class IfNode : AstNode, IStatementNode
    {
        public AstNode Condition { get; private set; }
        public List<AstNode> ThenBranch { get; private set; }
        public List<AstNode> ElseBranch { get; private set; }

        public IfNode(AstNode condition, List<AstNode> thenBranch, List<AstNode> elseBranch)
        {
            this.Condition = condition;
            this.ThenBranch = thenBranch;
            this.ElseBranch = elseBranch;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Condition.ToString());
            result.Append(' ');
            foreach (var branch in ThenBranch)
            {
                result.Append(branch.ToString());
            }
            result.Append(' ');
            foreach (var branch in ElseBranch)
            {
                result.Append(branch.ToString());
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
