using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class FnReferenceCallNode : AstNode, IExpressionNode
    {
        /// <summary>
        /// VariableReferenceNode / FnReferenceCallNode
        /// </summary>
        public AstNode Reference { get; private set; }
        public List<AstNode> Args { get; private set; }
        public Dictionary<string, AstNode> KeyArgs { get; private set; }
        public bool IsNullConditional { get; private set; }

        public FnReferenceCallNode(AstNode reference, List<AstNode> args, Dictionary<string, AstNode> keyArgs, bool isNullConditional)
        {
            this.Reference = reference;
            this.Args = args;
            this.KeyArgs = keyArgs;
            this.IsNullConditional = isNullConditional;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Reference);
            result.Append(this.IsNullConditional ? " ? " : " ");
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
