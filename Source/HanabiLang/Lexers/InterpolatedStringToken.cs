using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Lexers
{

    public class InterpolatedStringToken: Token
    {
        public List<string> Texts { get; private set; }
        public List<List<Token>> InterpolatedTokens { get; private set; }
        public InterpolatedStringToken(TokenType type, string raw, int line, List<string> texts, List<List<Token>> interpolatedTokens)
            : base(type, raw, line)
        {
            this.Texts = texts;
            this.InterpolatedTokens = interpolatedTokens;
        }

        public override string ToString()
        {
            return $"{Type}({Line}): {Raw}";
        }
    }
}
