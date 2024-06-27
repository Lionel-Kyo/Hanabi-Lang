using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Parses.Nodes;

namespace HanabiLang.Parses
{
    public class AbstractSyntaxTree
    {
        public List<AstNode> Nodes { get; set; }
        public AbstractSyntaxTree()
        {
            this.Nodes = new List<AstNode>();
        }
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (var item in Nodes)
            {
                result.AppendLine(item.ToString());
            }
            return result.ToString();
        }
    }
}
