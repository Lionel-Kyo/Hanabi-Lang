using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Interprets.ScriptTypes;

namespace HanabiLang.Interprets.Exceptions
{
    class ScriptException : ScriptClass
    {
        public ScriptException() :
            base("Exception", isStatic: false)
        {
            this.AddObjectFn(this.Name, new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                _this.BuildInObject = new HanibiException(_this.ClassType.Name);

                return ScriptValue.Null;
            });

            this.AddObjectFn(this.Name, new List<FnParameter>()
            {
                new FnParameter("message", BasicTypes.Str)
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptObject message = (ScriptObject)args[1].Value;
                _this.BuildInObject = new HanibiException(_this.ClassType.Name, (string)message.BuildInObject);

                return ScriptValue.Null;
            });

            AddVariable("Name", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((HanibiException)((ScriptObject)args[0].Value).BuildInObject).Name);
            }, null, false, null);

            AddVariable("Message", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((HanibiException)((ScriptObject)args[0].Value).BuildInObject).Message);
            }, null, false, null);
        }
        public override ScriptObject Create() => new ScriptObject(this, new HanibiException(this.Name));
        public ScriptObject Create(string message) => new ScriptObject(this, new HanibiException(this.Name, message));

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create($"{((HanibiException)_this.BuildInObject).Name}: {((HanibiException)_this.BuildInObject).Message}");
        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            return (string)this.ToStr(_this).BuildInObject;
        }
    }
}
