using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    internal class TypeNode : AstNode, IExpressionNode
    {
        public List<AstNode> Types { get; private set; }

        public TypeNode(List<AstNode> types)
        {
            this.Types = types;
        }

        public override string ToString()
        {
            return $"{this.NodeName} ({string.Join("|", Types)})";
        }
    }
}
