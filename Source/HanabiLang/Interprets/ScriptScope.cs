using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

namespace HanabiLang.Interprets
{
    public enum ScopeType
    {
        Normal,
        Conditon,
        Loop,
        Function,
        Class,
        Object
    }

    class ScriptScope
    {
        public Dictionary<string, ScriptVariable> Variables { get; private set; }
        public Dictionary<string, ScriptFns> Functions { get; private set; }
        public Dictionary<string, ScriptClass> Classes { get; private set; }
        public ScriptScope Parent { get; set; }
        //public ScopeType Type { get; private set; }
        public ScriptType Type { get; private set; }
        public ScriptScope(ScriptType type, ScriptScope parent = null)
        {
            this.Type = type;
            this.Parent = parent;
            this.Variables = new Dictionary<string, ScriptVariable>();
            this.Functions = new Dictionary<string, ScriptFns>();
            this.Classes = new Dictionary<string, ScriptClass>();
        }

        public bool TryGetValue(string name, out ScriptType value)
        {
            if (name.Equals("this"))
            {
                if (this.Type is ScriptObject)
                {
                    value = this.Type;
                    return true;
                }
                else if (this.Type is ScriptFn && this.Parent.Type is ScriptObject)
                {
                    value = this.Parent.Type;
                    return true;
                }
                else
                {
                    throw new SystemException("Cannot use this out of object");
                }
            }
            if (this.Type is ScriptObject)
            {
                ScriptObject scriptObject = (ScriptObject)this.Type;

                // Check if it exists in current class
                if (this.Variables.TryGetValue(name, out ScriptVariable interpretedVariable))
                {
                    value = interpretedVariable;
                    return true;
                }
                else if (scriptObject.ClassType.Scope.Functions.TryGetValue(name, out ScriptFns interpretedFunction))
                {
                    value = interpretedFunction;
                    return true;
                }
                else if (scriptObject.ClassType.Scope.Classes.TryGetValue(name, out ScriptClass interpretedClass))
                {
                    value = interpretedClass;
                    return true;
                }
                else if (scriptObject.ClassType.Scope.Variables.TryGetValue(name, out interpretedVariable))
                {
                    value = interpretedVariable;
                    return true;
                }

            }
            else
            {
                if (this.Classes.TryGetValue(name, out ScriptClass interpretedClass))
                {
                    value = interpretedClass;
                    return true;
                }
                else if (this.Functions.TryGetValue(name, out ScriptFns interpretedFunction))
                {
                    value = interpretedFunction;
                    return true;
                }
                else if (this.Variables.TryGetValue(name, out ScriptVariable interpretedVariable))
                {
                    value = interpretedVariable;
                    return true;
                }
            }
            value = null;
            return false;
        }

        public bool ContainsScope(ScriptScope scope)
        {
            for (ScriptScope item = this; item != null; item = item.Parent)
            {
                if (item == scope)
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return this.ToString(0);
        }

        public string ToString(int currentIndent = 0)
        {
            int basicIndent = 2;
            StringBuilder result = new StringBuilder();
            result.AppendLine(new string(' ', currentIndent) + $"\"Scope ({this.Type})\": {{");
            currentIndent += basicIndent;
            result.AppendLine(new string(' ', currentIndent) + "\"Classes\": [");
            currentIndent += basicIndent;
            foreach (var kv in this.Classes)
            {
                result.AppendLine(new string(' ', currentIndent) + $"\"{kv.Value}\", ");
            }
            if (this.Classes.Count > 0)
            {
                result.Remove(result.Length - 4, 4);
            }
            result.AppendLine();
            currentIndent -= basicIndent;
            result.AppendLine(new string(' ', currentIndent) + "], ");

            result.AppendLine(new string(' ', currentIndent) + "\"Functions\": [");
            currentIndent += basicIndent;
            foreach (var kv in this.Functions)
            {
                result.AppendLine(new string(' ', currentIndent) + $"\"{kv.Value}\", ");
            }
            if (this.Functions.Count > 0)
            {
                result.Remove(result.Length - 4, 4);
            }
            result.AppendLine();
            currentIndent -= basicIndent;
            result.AppendLine(new string(' ', currentIndent) + "], ");

            result.AppendLine(new string(' ', currentIndent) + "\"Variables\": [");
            currentIndent += basicIndent;
            foreach (var kv in this.Variables)
            {
                result.AppendLine(new string(' ', currentIndent) + $"\"{kv.Value}\", ");
            }
            if (this.Variables.Count > 0)
            {
                result.Remove(result.Length - 4, 4);
            }
            result.AppendLine();
            currentIndent -= basicIndent;
            result.Append(new string(' ', currentIndent) + "], ");

            if (this.Parent != null)
            {
                result.AppendLine();
                result.AppendLine(new string(' ', currentIndent) + "\"Parent\": {");
                result.AppendLine(Parent.ToString(currentIndent + 2));
                result.AppendLine(new string(' ', currentIndent) + "}");
            }
            else
            {
                result.Remove(result.Length - 2, 2);
                result.AppendLine();
            }
            currentIndent -= basicIndent;
            result.Append(new string(' ', currentIndent) + "}");
            return result.ToString();
        }
    }
}
