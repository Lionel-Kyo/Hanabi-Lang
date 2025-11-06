using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.Exceptions;
using HanabiLangLib.Interprets.ScriptTypes;

namespace HanabiLang.Interprets.ScriptTypes
{
    public static class BasicTypes
    {
        public static readonly ScriptObjectClass ObjectClass = new ScriptObjectClass();
        public static readonly ScriptNull Null = new ScriptNull();
        public static readonly ScriptObject NullValue = Null.Create();
        public static readonly ScriptTypeClass TypeClass = new ScriptTypeClass();
        public static readonly SciptFunctionClass FunctionClass = new SciptFunctionClass();
        public static readonly ScriptIterable Iterable = new ScriptIterable();
        public static readonly ScriptUnzipable Unzipable = new ScriptUnzipable();
        public static readonly ScriptInt Int = new ScriptInt();
        public static readonly ScriptFloat Float = new ScriptFloat();
        public static readonly ScriptDecimal Decimal = new ScriptDecimal();
        public static readonly ScriptBool Bool = new ScriptBool();
        public static readonly ScriptStr Str = new ScriptStr();
        public static readonly ScriptList List = new ScriptList();
        public static readonly ScriptDict Dict = new ScriptDict();
        public static readonly ScriptKeyValuePair KeyValuePair = new ScriptKeyValuePair();
        public static readonly ScriptRange Range = new ScriptRange();
        public static readonly ScriptSlice Slice = new ScriptSlice();
        public static readonly ScriptException Exception = new ScriptException();
        public static readonly ScriptCatchedExpression CatchedExpression = new ScriptCatchedExpression();
        public static readonly ScriptEnum Enum = new ScriptEnum();
        public static readonly ScriptFnEvent FnEvent = new ScriptFnEvent();
        public static readonly ScriptJson Json = new ScriptJson();
        public static readonly ScriptDateTime DateTime = new ScriptDateTime();
    }
}
