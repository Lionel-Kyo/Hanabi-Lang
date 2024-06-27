using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class BooleanNode : AstNode, IExpressionNode
    {
        public bool Value { get; private set; }

        public BooleanNode(bool value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return $"{this.NodeName}({Value})";
        }
    }
}
