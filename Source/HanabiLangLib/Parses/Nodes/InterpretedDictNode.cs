using HanabiLangLib.Interprets;
using HanabiLangLib.Interprets.ScriptTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HanabiLangLib.Parses.Nodes
{
    class InterpretedDictNode : AstNode, IExpressionNode
    {
        private ScriptValue value;

        public InterpretedDictNode(ScriptValue value, int pos, int line)
        {
            this.value = value;
            this.Pos = pos;
            this.Line = line;
        }

        public ScriptValue CloneValue()
        {
            return new ScriptValue(ScriptDict.AsCSharp(value.TryObject).ToDictionary(kv => kv.Key, kv => kv.Value));
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{this.NodeName}({value})");
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
