using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class IndexersNode : AstNode, IExpressionNode
    {
        public AstNode Object { get; private set; }
        public AstNode Index { get; private set; }
        public bool IsNullConditional { get; private set; }

        public IndexersNode(AstNode obj, AstNode index, bool isNullConditional)
        {
            this.Object = obj;
            this.Index = index;
            this.IsNullConditional = isNullConditional;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Object.ToString());
            result.Append(this.IsNullConditional ? " ? ": " ");
            result.Append(Index.ToString());
            result.Append(')');
            //result.Append("  ");
            return result.ToString();
        }
    }
}
