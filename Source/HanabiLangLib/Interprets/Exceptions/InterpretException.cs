using HanabiLangLib.Lexers;
using HanabiLangLib.Parses.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.Exceptions
{
    internal class InterpretException : SystemException
    {
        public InterpretException(string message, AstNode token) : base($"{message}\nLine: {token.Line}")
        { }
    }
}
