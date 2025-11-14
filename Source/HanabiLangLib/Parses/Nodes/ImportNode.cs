using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Parses.Nodes
{
    class ImportNode : AstNode, IStatementNode
    {
        public string Path { get; private set; }
        /// <summary>
        /// null = import as a variable, Length 0 = import all, Length > 0 = import some.
        /// Item1: Member Name, Item2: Renamed Member Name (as)
        /// </summary>
        public List<Tuple<string, string>> Imports { get; private set; }
        public string AsName { get; private set; }

        public ImportNode(string path, List<Tuple<string, string>> imports, string asName, int pos, int line)
        {
            this.Path = path;
            this.Imports = imports;
            this.AsName = asName;
            this.Pos = pos;
            this.Line = line;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(this.Path);
            result.Append(' ');
            if (this.AsName != null)
            {
                result.Append("as ");
                result.Append(this.AsName);
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
