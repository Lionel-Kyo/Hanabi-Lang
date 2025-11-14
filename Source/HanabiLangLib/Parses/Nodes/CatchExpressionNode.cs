using HanabiLangLib.Parses.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace HanabiLangLib.Parses.Nodes
{
    class CatchExpressionNode : AstNode, IExpressionNode
    {
        public AstNode Expression { get; private set; }
        public AstNode DefaultValue { get; private set; }

        public CatchExpressionNode(AstNode expression, AstNode defaultValue, int pos, int line)
        {
            this.Expression = expression;
            this.DefaultValue = defaultValue;
            this.Pos = pos;
            this.Line = line;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Expression.ToString());
            if (this.DefaultValue != null)
            {
                result.Append(' ');
                result.Append(DefaultValue.ToString());
            }
            result.Append(')');
            return result.ToString();
        }
    }
}
