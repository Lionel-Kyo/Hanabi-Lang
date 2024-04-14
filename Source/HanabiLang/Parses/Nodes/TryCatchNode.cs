using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class TryCatchNode : AstNode, IStatementNode
    {
        public List<AstNode> TryBranch { get; private set; }
        public List<AstNode> CatchBranch { get; private set; }
        public List<AstNode> FinallyBranch { get; private set; }

        public TryCatchNode(List<AstNode> tryBranch, List<AstNode> catchBranch, List<AstNode> finallyBranch)
        {
            this.TryBranch = tryBranch;
            this.CatchBranch = catchBranch;
            this.FinallyBranch = finallyBranch;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            foreach (var branch in TryBranch)
            {
                result.Append(branch.ToString());
            }
            result.Append(' ');
            if (CatchBranch != null)
            {
                foreach (var branch in CatchBranch)
                {
                    result.Append(branch.ToString());
                }
                result.Append(' ');
            }
            if (FinallyBranch != null)
            {
                foreach (var branch in FinallyBranch)
                {
                    result.Append(branch.ToString());
                }
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
