using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Parses.Nodes;

namespace HanabiLang.Interprets.ScriptTypes
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

            if (objectClass.Body != null)
            {
                foreach (object item in objectClass.Body)
                {
                    if (item is VariableDefinitionNode)
                        Interpreter.InterpretChild(this.Scope, (VariableDefinitionNode)item, false);
                    else if (item is ScriptVariable)
                        this.Scope.Variables[((ScriptVariable)item).Name] = (ScriptVariable)item;
                }
            }
        }

        private ScriptObject() { }

        public bool TryGetValue(string name, out ScriptType value)
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
            if (this.ClassType.TryGetValue("ToStr", out ScriptType fns) && fns is ScriptFns)
            {
                var _fns  = (ScriptFns)fns;
                return ScriptStr.AsCSharp((ScriptObject)_fns.Call(this).Value);
            }
            return ScriptStr.AsCSharp(ClassType.ToStr(this));
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
