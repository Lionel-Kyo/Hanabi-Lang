using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Lexers
{

    public class Token
    {
        public TokenType Type { get; set; }
        public string Raw { get; set; }
        public int Line { get; set; }
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
