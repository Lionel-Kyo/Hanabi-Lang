﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class ForNode : AstNode, IStatementNode
    {
        public List<string> Initializers { get; private set; }
        public AstNode Iterator { get; private set; }
        public List<AstNode> Body { get; private set; }

        public ForNode(List<string> initializers, AstNode iterator, List<AstNode> body)
        {
            this.Initializers = initializers;
            this.Iterator = iterator;
            this.Body = body;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(string.Join(",", Iterator));
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
