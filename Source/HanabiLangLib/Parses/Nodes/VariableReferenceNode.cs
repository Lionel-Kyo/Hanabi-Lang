using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class VariableReferenceNode : AstNode, IExpressionNode
    {
        public string Name { get; private set; }

        public VariableReferenceNode(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}");
            result.Append('(');
            result.Append($"{Name}");
            /*foreach (var name in Name)
            {
                result.Append($"{name}.");
            }
            if (Names.Count != 0)
            {
                result.Remove(result.Length - 1, 1);
            }*/
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
