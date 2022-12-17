using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class DictNode : AstNode
    {
        public List<Tuple<AstNode, AstNode>> KeyValues { get; private set; }

        public DictNode(List<Tuple<AstNode, AstNode>> keyValues)
        {
            this.KeyValues = keyValues;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('{');
            foreach (var element in KeyValues)
            {
                result.Append(element.Item1);
                result.Append(": ");
                result.Append(element.Item2);
                result.Append(", ");
            }
            if (KeyValues.Count != 0)
            {
                result.Remove(result.Length - 2, 2);
            }
            result.Append(')');
            //result.Append("  ");
            return result.ToString();
        }
    }
}
