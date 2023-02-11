using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    abstract class ScriptType
    {
        public virtual string ToJsonString(int basicIndent = 2, int currentIndent = 0) => "";
        /*public virtual bool Equals(ScriptType obj)
        {
            if (this is ScriptObject && obj is ScriptObject)
                return ((ScriptBool)((ScriptObject)(this)).Equals((ScriptObject)obj)).Value;
            return this.guid.Equals(obj.guid);
        }

        public override bool Equals(object obj)
        {
            if (obj is ScriptType)
                return this.Equals((ScriptType)obj);
            return base.Equals(obj);
        }*/

    }
}
