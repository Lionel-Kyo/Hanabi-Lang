using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class StringNode : AstNode
    {
        public string Value { get; private set; }

        public StringNode(string value)
        {
            this.Value = value;
        }

        public StringNode(StringBuilder value)
        {
            this.Value = value.ToString();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({Value})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
