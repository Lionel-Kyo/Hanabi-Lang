using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    internal class BasicTypes
    {
        public static readonly ScriptObjectClass ObjectClass = new ScriptObjectClass();
        public static readonly ScriptInt Int = new ScriptInt();
        public static readonly ScriptFloat Float = new ScriptFloat();
        public static readonly ScriptDecimal Decimal = new ScriptDecimal();
        public static readonly ScriptBool Bool = new ScriptBool();
        public static readonly ScriptStr Str = new ScriptStr();
        public static readonly ScriptList List = new ScriptList();
        public static readonly ScriptDict Dict = new ScriptDict();
        public static readonly ScriptRange Range = new ScriptRange();
        public static readonly ScriptNull Null = new ScriptNull();
        public static readonly ScriptObject NullValue = Null.Create();
        public static readonly ScriptEnumerator Enumerator = new ScriptEnumerator();
    }
}
