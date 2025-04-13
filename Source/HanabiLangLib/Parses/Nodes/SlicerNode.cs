using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class SlicerNode : AstNode, IExpressionNode
    {
        public AstNode Object { get; private set; }
        public List<List<AstNode>> Slices { get; private set; }
        public bool IsNullConditional { get; private set; }

        public SlicerNode(AstNode obj, List<List<AstNode>> slices, bool isNullConditional)
        {
            this.Object = obj;
            this.Slices = slices;
            this.IsNullConditional = isNullConditional;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Object.ToString());
            result.Append(this.IsNullConditional ? " ? ": " ");
            result.Append(string.Join(", ", Slices.Select(i => "[" + string.Join(":", i) + "]")));
            result.Append(')');
            return result.ToString();
        }
    }
}
