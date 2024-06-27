using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class ContinueNode : AstNode, IStatementNode
    {
        public ContinueNode() { }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
