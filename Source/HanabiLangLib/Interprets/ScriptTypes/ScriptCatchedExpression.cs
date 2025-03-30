using HanabiLang.Interprets;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Parses.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptCatchedExpression : ScriptClass
    {
        public ScriptCatchedExpression() :
            base("CatchedExpression", isStatic: false)
        {
            AddVariable("Result", args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptValue value = AsCSharp(_this).Item1;
                return value == null ? new ScriptValue() : value;
            }, null, false, null);

            AddVariable("Error", args =>
            {
                ScriptObject _this = args[0].TryObject;
                Exception exception = AsCSharp(_this).Item2;
                return exception == null ? new ScriptValue() : new ScriptValue(BasicTypes.Exception.Create(exception));
            }, null, false, null);
        }

        public override ScriptObject Create() => new ScriptObject(this, new Tuple<ScriptValue, Exception>(null, null));
        public ScriptObject Create(ScriptValue result, Exception exception) => new ScriptObject(this, Tuple.Create(result, exception));

        public override ScriptObject ToStr(ScriptObject _this)
        {
            return BasicTypes.Str.Create($"<object: CatchedExpression>");
        }

        public static Tuple<ScriptValue, Exception> AsCSharp(ScriptObject _this)
        {
            return (Tuple<ScriptValue, Exception>)_this.BuildInObject;
        }
    }
}
