using HanabiLang.Lexers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses
{
    internal class ParseException : SystemException
    {
        public ParseException(string message, Token token) : base($"{message}\nLine: {token.Line}")
        { }
    }

    internal class ParseFormatNotCompleteException : ParseException
    {
        public ParseFormatNotCompleteException(string message, Token token) : base(message, token)
        { }
    }
}
