using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptEnumerator : ScriptClass
    {
        public ScriptEnumerator() :
            base("Enumerator", null, new ScriptScope(ScopeType.Class), false)
        {
            this.AddObjectFn(this.Name, new List<FnParameter>()
            {
                new FnParameter("obj"),
                new FnParameter("currentFn"),
                new FnParameter("moveNextFn"),
                new FnParameter("resetFn")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ScriptObject currentObject = (ScriptObject)args[1].Value;

                for (int i = 2; i < args.Count; i++)
                    if (!args[i].IsFunction)
                        throw new SystemException($"{args[i]} is not a function");
                var currentFn = args[2];
                var moveNextFn = args[3];
                var resetFn = args[4];

                _this.BuildInObject = new Enumerator(currentObject, currentFn, moveNextFn, resetFn);

                return ScriptValue.Null;
            });
        }

        class Enumerator : IEnumerator<ScriptValue>, IEnumerable<ScriptValue>
        {
            private ScriptObject currentObject;
            private ScriptFns currentFn;
            private ScriptFns moveNextFn;
            private ScriptFns resetFn;
            public Enumerator(ScriptObject currentObject, ScriptValue current, ScriptValue moveNext, ScriptValue reset)
            {
                if (!current.IsFunction)
                    throw new SystemException("Current must be function");
                if (!moveNext.IsFunction)
                    throw new SystemException("MoveNext must be function");
                if (!reset.IsFunction)
                    throw new SystemException("Reset must be function");
                this.currentObject = currentObject;
                this.currentFn = (ScriptFns)current.Value;
                this.moveNextFn = (ScriptFns)moveNext.Value;
                this.resetFn = (ScriptFns)reset.Value;
            }

            public object Current => currentFn.Call(currentObject.Scope, currentObject, new Dictionary<string, Parses.Nodes.AstNode>());

            ScriptValue IEnumerator<ScriptValue>.Current => currentFn.Call(currentObject.Scope, currentObject, new Dictionary<string, Parses.Nodes.AstNode>());

            public void Dispose() => resetFn.Call(currentObject.Scope, currentObject, new Dictionary<string, Parses.Nodes.AstNode>());

            public bool MoveNext()
            {
                var result = moveNextFn.Call(currentObject.Scope, currentObject, new Dictionary<string, Parses.Nodes.AstNode>());
                if (!result.IsObject)
                    throw new SystemException("MoveNext does not return a bool");
                var obj = (ScriptObject)result.Value;
                if (!(obj.ClassType is ScriptBool))
                    throw new SystemException("MoveNext does not return a bool");
                return (bool)obj.BuildInObject;
            }

            public void Reset() => resetFn.Call(currentObject.Scope, currentObject, new Dictionary<string, Parses.Nodes.AstNode>());

            public IEnumerator<ScriptValue> GetEnumerator() => this;
            IEnumerator IEnumerable.GetEnumerator() => this;
        }

    }
}
