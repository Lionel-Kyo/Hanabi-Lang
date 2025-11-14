using HanabiLangLib.Lexers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    public abstract class AstNode
    {
        private static readonly string NODE_NAME_POST_TEXT = "Node";

        public int Pos { get; protected set; }
        public int Line { get; protected set; }

        public string NodeName
        {
            get
            {
                string result = this.GetType().Name;
                if (result.EndsWith(NODE_NAME_POST_TEXT))
                    result = result.Remove(result.Length - NODE_NAME_POST_TEXT.Length, NODE_NAME_POST_TEXT.Length);
                return result;
            }
        }
    }

    interface IStatementNode { }
    interface IExpressionNode {}
}