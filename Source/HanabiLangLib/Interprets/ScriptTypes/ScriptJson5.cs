using HanabiLang.Interprets;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Lexers;
using HanabiLang.Parses;
using HanabiLangLib.Interprets.Json5Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptJson5 : ScriptClass
    {
        public ScriptJson5() : base("Json5", true)
        {
            this.AddFunction("Deserialize", new List<FnParameter>()
            {
                new FnParameter("input", BasicTypes.Str),
                new FnParameter("type", new HashSet<ScriptClass>{ BasicTypes.TypeClass, BasicTypes.Null }, new ScriptValue())
            }, args =>
            {
                var input = ScriptStr.AsCSharp(args[0].TryObject);
                var targetType = args[1].IsNull ? null : (ScriptClass)args[1].TryObject.BuildInObject;
                var lexer = new Json5Lexer(input);

                var tokens = lexer.Tokenize();
                var parser = new Json5Parser(tokens, targetType);

                return parser.Parse();
            }, true, AccessibilityLevel.Public);

            this.AddFunction("Serialize", new List<FnParameter>()
            {
                new FnParameter("input"),
                new FnParameter("indent", new HashSet<ScriptClass>() { BasicTypes.Str, BasicTypes.Null }, new ScriptValue()),
                new FnParameter("checkCircularRef", BasicTypes.Bool, new ScriptValue(true)),
                new FnParameter("quoteAllKeys", BasicTypes.Bool, new ScriptValue(true)),
                new FnParameter("useSingleQuote", BasicTypes.Bool, new ScriptValue(false)),
                new FnParameter("nanAsNull", BasicTypes.Bool, new ScriptValue(false)),
                new FnParameter("infinityAsNull", BasicTypes.Bool, new ScriptValue(false)),
                new FnParameter("trailingComma", BasicTypes.Bool, new ScriptValue(false)),
                new FnParameter("allowScientific", BasicTypes.Bool, new ScriptValue(false)),
                new FnParameter("allowJson5EscapeSequences", BasicTypes.Bool, new ScriptValue(false)),
                new FnParameter("ensureAscii", BasicTypes.Bool, new ScriptValue(false)),
            }, args =>
            {
                var input = args[0];
                var indent = args[1].IsNull ? null : ScriptStr.AsCSharp(args[1].TryObject);
                var checkCircularRef = ScriptBool.AsCSharp(args[2].TryObject);
                var quoteAllKeys = ScriptBool.AsCSharp(args[3].TryObject);
                var useSingleQuote = ScriptBool.AsCSharp(args[4].TryObject);
                var nanAsNull = ScriptBool.AsCSharp(args[5].TryObject);
                var infinityAsNull = ScriptBool.AsCSharp(args[6].TryObject);
                var trailingComma = ScriptBool.AsCSharp(args[7].TryObject);
                var allowScientific = ScriptBool.AsCSharp(args[8].TryObject);
                var allowJson5EscapeSequences = ScriptBool.AsCSharp(args[9].TryObject);
                var ensureAscii = ScriptBool.AsCSharp(args[10].TryObject);

                var serializer = new Json5Serializer(
                    indent, 
                    checkCircularRef, 
                    quoteAllKeys, 
                    useSingleQuote, 
                    nanAsNull, 
                    infinityAsNull, 
                    trailingComma, 
                    allowScientific, 
                    allowJson5EscapeSequences, 
                    ensureAscii
                );
                return new ScriptValue(serializer.Serialize(input));

            }, true, AccessibilityLevel.Public);
        }
    }
}
