using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

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
            // this.Scope.Variables["this"] = new ScriptVariable("this", new ScriptValue(this), true, false, AccessibilityLevel.Private);
            //this.Scope.Variables["super"] = new ScriptVariable("super", new ScriptValue(this.ClassType.SuperClass), true, false, AccessibilityLevel.Private);
            this.BuildInObject = buildInObject;
        }

        private ScriptObject() { }

        public bool TryGetValue(string name, out ScriptType value)
        {
            return this.Scope.TryGetValue(name, out value);
        }

        public override string ToString()
        {
            if (this.Scope.Parent.Functions.TryGetValue("ToStr", out ScriptFns fns))
            {
                return (string)((ScriptObject)fns.Call(this).Value).BuildInObject;
            }
            return (string)ClassType.ToStr(this).BuildInObject;
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
