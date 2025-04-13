using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class IndexerNode : AstNode, IExpressionNode
    {
        public AstNode Object { get; private set; }
        public List<AstNode> Indexes { get; private set; }
        public bool IsNullConditional { get; private set; }

        public IndexerNode(AstNode obj, List<AstNode> indexes, bool isNullConditional)
        {
            this.Object = obj;
            this.Indexes = indexes;
            this.IsNullConditional = isNullConditional;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Object.ToString());
            result.Append(this.IsNullConditional ? " ? ": " ");
            result.Append(string.Join(", ", Indexes.Select(i => "[" + string.Join(":", i) + "]")));
            result.Append(')');
            return result.ToString();
        }
    }
}
