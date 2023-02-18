using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    internal class BasicTypes
    {
        public static ScriptObjectClass ObjectClass = new ScriptObjectClass();
        public static ScriptInt Int = new ScriptInt();
        public static ScriptFloat Float = new ScriptFloat();
        public static ScriptDecimal Decimal = new ScriptDecimal();
        public static ScriptBool Bool = new ScriptBool();
        public static ScriptStr Str = new ScriptStr();
        public static ScriptList List = new ScriptList();
        public static ScriptDict Dict = new ScriptDict();
        public static ScriptRange Range = new ScriptRange();
        public static ScriptNull Null = new ScriptNull();
        public static ScriptObject NullValue = Null.Create();
        public static ScriptEnumerator Enumerator = new ScriptEnumerator();
    }
}
