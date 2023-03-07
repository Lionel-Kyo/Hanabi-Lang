using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Lexers
{
    internal enum TokenType
    {
        OPERATOR,
        OPEN_ROUND_BRACKET,
        CLOSE_ROUND_BRACKET,
        OPEN_CURLY_BRACKET,
        CLOSE_CURLY_BRACKET,
        OPEN_SQURE_BRACKET,
        CLOSE_SQURE_BRACKET,
        SINGLE_ARROW,
        DOUBLE_ARROW,
        DOT,
        COMMA,
        COLON,
        SEMI_COLON,
        QUESTION_MARK,

        KEYWORD,
        IDENTIFIER,
        STRING,
        INTERPOLATED_STRING,
        INT,
        FLOAT,
        NULL,
        TRUE,
        FALSE,
        EQUALS
    }
}
