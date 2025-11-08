using HanabiLangLib.Lexers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses
{
    public class ParseException : Exception
    {
        public ParseException(string message, Token token) : base($"{message}\nLine: {token.Line}")
        { }
    }

    public class NotLambdaParseException : Exception
    {
        public NotLambdaParseException(string message, Token token) : base($"{message}\nLine: {token.Line}")
        { }
    }

    public class ParseFormatNotCompleteException : ParseException
    {
        public ParseFormatNotCompleteException(string message, Token token) : base(message, token)
        { }
    }
}
