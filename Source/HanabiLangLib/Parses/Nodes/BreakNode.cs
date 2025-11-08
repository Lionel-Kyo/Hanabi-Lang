using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class BreakNode : AstNode, IStatementNode
    {
        public BreakNode() { }

        public override string ToString()
        {
            return this.NodeName;
        }
    }
}
