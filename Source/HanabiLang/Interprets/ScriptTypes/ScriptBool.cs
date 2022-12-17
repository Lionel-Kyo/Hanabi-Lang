using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptBool : ScriptObject
    {
        public static ScriptClass CreateBuildInClass()
        {
            var newScrope = new ScriptScope(ScopeType.Class);
            return new ScriptClass("bool", null, new List<string>(),
                newScrope, false, () => new ScriptBool());
        }
        public bool Value { get; private set; }
        public ScriptBool() :
            base(CreateBuildInClass())
        {
            this.Value = false;
        }
        public ScriptBool(bool value) : this()
        {
            this.Value = value;
        }

        public static ScriptBool True => new ScriptBool(true);
        public static ScriptBool False => new ScriptBool(false);

        public override ScriptObject Not()
        {
            return new ScriptBool(!this.Value);
        }

        public override ScriptObject And(ScriptObject value)
        {
            if (value is ScriptBool)
            {
                ScriptBool obj = (ScriptBool)value;
                return new ScriptBool(this.Value && obj.Value);
            }
            return base.And(value);
        }
        public override ScriptObject Or(ScriptObject value)
        {
            if (value is ScriptBool)
            {
                ScriptBool obj = (ScriptBool)value;
                return new ScriptBool(this.Value || obj.Value);
            }
            return base.Or(value);
        }

        public override ScriptObject Equals(ScriptObject value)
        {
            if (value is ScriptBool)
            {
                ScriptBool obj = (ScriptBool)value;
                return new ScriptBool(this.Value == obj.Value);
            }
            return ScriptBool.False;
        }

        public override ScriptStr ToStr() => new ScriptStr(this.ToString());

        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0)
        {
            return this.Value ? "true" : "false";
        }

        public override string ToString() => this.Value ? "true" : "false";

        public override ScriptObject Copy()
        {
            return new ScriptBool(this.Value);
        }

        /*public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }*/
    }
}
