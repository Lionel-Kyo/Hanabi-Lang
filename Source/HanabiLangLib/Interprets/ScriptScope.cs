using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

namespace HanabiLang.Interprets
{
    public class ScriptScope
    {
        public Dictionary<string, ScriptVariable> Variables { get; private set; }
        public ScriptScope Parent { get; set; }
        public ScriptType Type { get; private set; }
        public Interpreter ParentInterpreter { get; private set; }

        private ScriptScope(ScriptType type)
        {
            this.Type = type;
            this.Parent = null;
            this.ParentInterpreter = null;
            this.Variables = new Dictionary<string, ScriptVariable>();
        }

        public ScriptScope(ScriptType type, Interpreter parentInterpreter) : this(type)
        {
            this.Parent = null;
            this.ParentInterpreter = parentInterpreter;
        }

        public ScriptScope(ScriptType type, ScriptScope parentScope) : this(type)
        {
            this.Parent = parentScope;
            this.ParentInterpreter = parentScope == null ? null : parentScope.ParentInterpreter;
        }

        public bool TryGetValue(string name, out ScriptVariable value)
        {
            if (this.Type is ScriptObject)
            {
                ScriptObject scriptObject = (ScriptObject)this.Type;

                // Check if it exists in current class
                if (this.Variables.TryGetValue(name, out ScriptVariable interpretedVariable))
                {
                    value = interpretedVariable;
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
                if (this.Variables.TryGetValue(name, out ScriptVariable interpretedVariable))
                {
                    value = interpretedVariable;
                    return true;
                }
            }
            value = null;
            return false;
        }

        public ScriptScope GetParentScope(Func<ScriptScope, bool> condition)
        {
            if (condition == null)
                return this.Parent;

            for (ScriptScope scope = this.Parent; scope != null; scope = scope.Parent)
            {
                if (condition(scope))
                    return scope;
            }
            return null;
        }

        public ScriptScope GetParentScope()
        {
            return GetParentScope(null);
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
