using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class CatchNode
    {
        public string Name { get; private set; }
        public AstNode DataType { get; private set; }
        public List<AstNode> Body { get; private set; }

        public CatchNode(string name, AstNode dataType, List<AstNode> body)
        {
            this.Name = name;
            this.DataType = dataType;
            this.Body = body;
        }

        public override string ToString()
        {
            return $"{Name}: {DataType}, {string.Join(" ", Body)}";
        }
    }
}
