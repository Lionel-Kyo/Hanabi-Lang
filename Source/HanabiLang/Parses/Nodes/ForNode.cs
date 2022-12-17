using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class ForNode : AstNode
    {
        public string Initializer { get; private set; }
        public AstNode Location { get; private set; }
        public List<AstNode> Body { get; private set; }

        public ForNode(string initializer, AstNode location, List<AstNode> body)
        {
            this.Initializer = initializer;
            this.Location = location;
            this.Body = body;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Location.ToString());
            result.Append(' ');
            foreach (var statement in Body)
            {
                result.Append(statement.ToString());
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
