﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    [Obsolete]
    class FnCallNode : AstNode
    {
        public string Name { get; private set; }
        public Dictionary<string, AstNode> Args { get; private set; }

        public FnCallNode(string name, Dictionary<string, AstNode> args)
        {
            this.Name = name;
            this.Args = args;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Name);
            result.Append(' ');
            foreach (var arg in Args)
            {
                result.Append($"[{arg.Key}:{arg.Value}]");
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
