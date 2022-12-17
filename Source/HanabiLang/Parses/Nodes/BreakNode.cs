using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class BreakNode : AstNode
    {
        public BreakNode() { }

        public override string ToString()
        {
            return this.NodeName;
        }
    }
}
