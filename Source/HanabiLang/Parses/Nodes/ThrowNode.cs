using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class ThrowNode : AstNode
    {
        public AstNode Value { get; private set; }

        public ThrowNode(AstNode value)
        {
            this.Value = value;
        }

        public ThrowNode()
        {
            this.Value = null;
        }
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append(' ');
            result.Append(Value == null ? "null" : Value.ToString());
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
