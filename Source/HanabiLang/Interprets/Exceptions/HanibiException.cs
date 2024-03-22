using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

namespace HanabiLang.Interprets.Exceptions
{
    class HanibiException : Exception
    {
        public string Name { get; private set; }
        public HanibiException(string name, string message) :
            base(message)
        {
            this.Name = name;
        }
        public HanibiException(string name) : this(name, "")
        { }
    }
}
