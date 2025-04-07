using System;
using System.Collections.Generic;
using System.Text;

namespace HanabiLang.Parses
{
    internal enum ParseScope
    {
        Class,
        Fn,
        Enum,
        Loop,
        Condition,
        TryCatchFinally,
    }
}
