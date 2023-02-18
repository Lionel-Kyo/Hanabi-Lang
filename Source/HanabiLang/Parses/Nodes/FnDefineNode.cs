using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Parses.Nodes
{
    class FnDefineParameter
    {
        public string Name { get; private set; }
        public AstNode DataType { get; private set; }
        public AstNode DefaultValue { get; private set; }

        public FnDefineParameter(string name, AstNode dataType = null, AstNode defaultValue = null)
        {
            this.Name = name;
            this.DataType = dataType;
            this.DefaultValue = defaultValue;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class FnDefineNode : AstNode
    {
        public string Name { get; private set; }
        public List<FnDefineParameter> Parameters { get; private set; }
        public AstNode ReturnType { get; private set; }
        public List<AstNode> Body { get; private set; }
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }

        public FnDefineNode(string name, List<FnDefineParameter> parameters, AstNode returnType, List<AstNode> body, bool isStatic, AccessibilityLevel level)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.ReturnType = returnType;
            this.Body = body;
            this.IsStatic = isStatic;
            this.Level = level;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NodeName);
            result.Append('(');
            result.Append(Name);
            result.Append(' ');
            result.Append("Return Type: ");
            if (ReturnType == null)
                result.Append("Not Defined");
            else
                result.Append(ReturnType);
            result.Append(' ');
            foreach (var parm in Parameters)
            {
                result.Append(parm.ToString());
                result.Append(',');
            }
            result.Append(' ');
            foreach (var statement in Body)
            {
                result.Append(statement.ToString());
            }
            result.Append(')');
            //result.AppendLine("  ");
            return result.ToString();
        }
    }
}
