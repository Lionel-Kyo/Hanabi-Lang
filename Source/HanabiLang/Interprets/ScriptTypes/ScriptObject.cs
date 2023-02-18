using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptObject : ScriptType
    {
        public ScriptScope Scope { get; private set; }
        public ScriptClass ClassType { get; private set; }
        public object BuildInObject { get; set; }

        public ScriptObject(ScriptClass objectClass, object buildInObject = null)
        {
            this.ClassType = objectClass;
            this.Scope = new ScriptScope(ScopeType.Object, null, objectClass.Scope);
            this.Scope.Variables["this"] = new ScriptVariable("this", new ScriptValue(this), true, false, AccessibilityLevels.Private);
            this.BuildInObject = buildInObject;
        }

        public override string ToString()
        {
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
