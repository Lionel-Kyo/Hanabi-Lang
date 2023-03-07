using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Lexers
{

    internal class Token
    {
        public TokenType Type { get; private set; }
        public string Raw { get; private set; }
        public int Line { get; private set; }
        public Token(TokenType type, string raw, int line)
        {
            this.Type = type;
            this.Raw = raw;
            this.Line = line;
        }

        public override string ToString()
        {
            return $"{Type}({Line}): {Raw}";
        }
    }
}
