using HanabiLang.Lexers;
using HanabiLang.Parses.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.Exceptions
{
    internal class InterpretException : SystemException
    {
        public InterpretException(string message, AstNode token) : base($"{message}\nLine: {token.Line}")
        { }
    }
}
