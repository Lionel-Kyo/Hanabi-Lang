﻿using System;
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

        public FnReferenceCallNode(AstNode reference, List<AstNode> args)
        {
            this.Reference = reference;
            this.Args = args;
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
                result.Append(arg.ToString());
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
