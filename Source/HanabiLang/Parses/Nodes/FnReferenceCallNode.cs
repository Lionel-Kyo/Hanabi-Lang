using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class FnReferenceCallNode : AstNode
    {
        public AstNode Reference { get; private set; }
        public List<AstNode> Args { get; private set; }
        public Dictionary<string, AstNode> KeyArgs { get; private set; }

        public FnReferenceCallNode(AstNode reference, List<AstNode> args, Dictionary<string, AstNode> keyArgs)
        {
            this.Reference = reference;
            this.Args = args;
            this.KeyArgs = keyArgs;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Reference);
            result.Append(' ');
            foreach (var arg in Args)
            {
                result.Append($"[{arg}]");
            }
            foreach (var arg in KeyArgs)
            {
                result.Append($"[{arg.Key}:{arg.Value}]");
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
