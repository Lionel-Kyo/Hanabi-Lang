﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Lexers
{
    public enum TokenType
    {
        OPERATOR,
        OPEN_ROUND_BRACKET,
        QUESTION_OPEN_ROUND_BRACKET,
        CLOSE_ROUND_BRACKET,
        OPEN_CURLY_BRACKET,
        CLOSE_CURLY_BRACKET,
        OPEN_SQURE_BRACKET,
        QUESTION_OPEN_SQURE_BRACKET,
        CLOSE_SQURE_BRACKET,
        SINGLE_ARROW,
        DOUBLE_ARROW,
        DOT,
        QUESTION_DOT,
        COMMA,
        COLON,
        SEMI_COLON,
        QUESTION_MARK,
        DOUBLE_QUESTION_MARK,

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
