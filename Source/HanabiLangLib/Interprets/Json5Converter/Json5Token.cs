using System;
using System.Collections.Generic;
using System.Text;

namespace HanabiLangLib.Interprets.Json5Converter
{
    public enum Json5TokenType
    {
        Identifier,
        String,
        Integer,
        Float,
        Boolean,
        Null,
        LeftBrace,
        RightBrace,
        LeftBracket,
        RightBracket,
        Colon,
        Comma,
        EOF
    }

    public class Json5Token
    {
        public Json5TokenType Type;
        public string Text;
        public int Pos;
        public int Line;

        public Json5Token(Json5TokenType type, string text, int pos, int line)
        {
            Type = type;
            Text = text;
            Pos = pos;
            Line = line;
        }

        public override string ToString() => $"Token({Type}, '{Text}')";
    }
}
