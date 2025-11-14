using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HanabiLangLib.Interprets.ScriptTypes;
using HanabiLangLib.Parses.Nodes;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptObject : ScriptType
    {
        internal ScriptScope Scope { get; private set; }
        public ScriptClass ClassType { get; private set; }
        public object BuildInObject { get; set; }

        public ScriptObject(ScriptClass objectClass, object buildInObject = null)
        {
            this.ClassType = objectClass;
            this.Scope = new ScriptScope(this, objectClass.Scope);
            this.BuildInObject = buildInObject;

            if (objectClass.Body != null && objectClass.Body.Count > 0)
            {
                ScriptScope initalizeVariablesScope = new ScriptScope(new ScriptFn(new List<FnParameter>() { new FnParameter("this") }, new List<AstNode>(), this.Scope, true, AccessibilityLevel.Private), this.Scope);
                initalizeVariablesScope.Variables["this"] = new ScriptVariable("this", null, new ScriptValue(this), true, true, AccessibilityLevel.Private);
                foreach (object item in objectClass.Body)
                {
                    if (item is VariableDefinitionNode)
                        Interpreter.VariableDefinition((VariableDefinitionNode)item, this.Scope, initalizeVariablesScope);
                    else if (item is ScriptVariable)
                        this.Scope.Variables[((ScriptVariable)item).Name] = (ScriptVariable)item;
                }
            }
        }

        private ScriptObject() { }

        public bool TryGetValue(string name, out ScriptVariable value)
        {
            if (this.Scope.TryGetValue(name, out value))
                return true;
            return false;
        }

        public bool IsTypeOrSubOf(ScriptClass type)
        {
            return this.ClassType == type || (this.ClassType?.SuperClasses?.Contains(type) ?? false);
        }

        public override string ToString()
        {
            if (this.ClassType.TryGetValue(ScriptClass.TO_STR, out ScriptVariable fns) && fns.Value.IsFunction)
            {
                var _fns  = fns.Value.TryFunction;
                return ScriptStr.AsCSharp(_fns.Call(this).TryObject);
            }
            return $"<object: {this.ClassType.Name}>";
        }

        public override int GetHashCode()
        {
            if (ClassType.IsBuildIn)
            {
                return BuildInObject.GetHashCode();
            }
            return base.GetHashCode();
        }
    }
}
