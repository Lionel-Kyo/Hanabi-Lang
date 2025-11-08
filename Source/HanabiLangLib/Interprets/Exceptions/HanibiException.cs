using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLangLib.Interprets.ScriptTypes;

namespace HanabiLangLib.Interprets.Exceptions
{
    class HanibiException : Exception
    {
        public ScriptObject ExceptionObject { get; private set; }
        public HanibiException(ScriptObject exceptionObject, string message) :
            base(message)
        {
            this.ExceptionObject = exceptionObject;
        }
        public HanibiException(ScriptObject exceptionObject) : this(exceptionObject, "")
        { }
    }
}
