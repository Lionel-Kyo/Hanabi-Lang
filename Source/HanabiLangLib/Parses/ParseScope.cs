using System;
using System.Collections.Generic;
using System.Text;

namespace HanabiLangLib.Parses
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
