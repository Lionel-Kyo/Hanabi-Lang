using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    abstract class ScriptType
    {
        protected Guid guid = Guid.NewGuid();

        public virtual string ToJsonString(int basicIndent = 2, int currentIndent = 0) => "";
    }
}
