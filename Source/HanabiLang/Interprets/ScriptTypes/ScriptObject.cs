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
        public delegate ScriptObject CreateBuildInObject();
        public ScriptScope Scope { get; private set; }
        public ScriptClass ObjectClass { get; private set; }

        public ScriptObject(ScriptClass objectClass, Interpreter interpreter = null)
        {
            this.Scope = new ScriptScope(ScopeType.Object);
            //this.interpreter = interpreter ?? new Interpreter(this.Scope);
            this.Scope.Variables["this"] = new ScriptVariable("this", new ScriptValue(this), true);
            this.ObjectClass = objectClass;
            this.AddBasicFns();
        }

        protected void AddObjectFn(string name, BuildInFns.ScriptFnType fn)
        {
            this.Scope.Functions[name] = 
                new ScriptFn(name, BuildInFns.GetBuildInFnParams(fn), null, this.Scope, fn);
        }

        protected void AddObjectFnIsNotExists(string name, BuildInFns.ScriptFnType fn)
        {
            if (!this.Scope.Functions.ContainsKey(name))
            {
                this.Scope.Functions[name] =
                    new ScriptFn(name, BuildInFns.GetBuildInFnParams(fn), null, this.Scope, fn);
            }
        }

        private void AddBasicFns()
        {
            AddObjectFn("ToStr", args => new ScriptValue(ToStr()));
        }

        public virtual ScriptObject Not()
        {
            if (this.Scope.Functions.TryGetValue("!", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope).Value;
            throw new SystemException("Operator ! is not implemented");
        }
        public virtual ScriptObject Positive()
        {
            if (this.Scope.Functions.TryGetValue("^+", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope).Value;
            throw new SystemException("Operator ! is not implemented");
        }
        public virtual ScriptObject Negative()
        {
            if (this.Scope.Functions.TryGetValue("^-", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope).Value;
            throw new SystemException("Operator ! is not implemented");
        }
        public virtual ScriptObject Add(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("+", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator + is not implemented");
        }
        public virtual ScriptObject Minus(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("-", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator - is not implemented");
        }
        public virtual ScriptObject Multiply(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("*", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator * is not implemented");
        }
        public virtual ScriptObject Divide(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("/", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator / is not implemented");
        }
        public virtual ScriptObject Modulo(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("%", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator ! is not implemented");
        }
        public virtual ScriptObject Larger(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue(">", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator > is not implemented");
        }
        public virtual ScriptObject LargerEquals(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue(">=", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator >= is not implemented");
        }
        public virtual ScriptObject Less(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("<", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator < is not implemented");
        }
        public virtual ScriptObject LessEquals(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("<=", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator <= is not implemented");
        }
        public virtual ScriptObject And(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("&&", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator && is not implemented");
        }
        public virtual ScriptObject Or(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("||", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator || is not implemented");
        }
        public virtual ScriptObject Equals(ScriptObject value)
        {
            if (this.Scope.Functions.TryGetValue("==", out ScriptFn fn))
                return (ScriptObject)fn.Call(Scope, new ScriptValue(value)).Value;
            throw new SystemException("Operator ! is not implemented");
        }

        public virtual ScriptStr ToStr()
        {
            return new ScriptStr(this.ToString());
        }

        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0)
        {
            StringBuilder result = new StringBuilder();
            //result.Append(' ', currentIndent);
            result.Append('{');
            if (basicIndent != 0)
            {
                result.AppendLine();
                currentIndent += 2;
            }
            int count = 0;
            foreach (var item in Scope.Variables)
            {
                if (item.Key.Equals("this") || item.Key.Equals("super"))
                {
                    count++;
                    continue;
                }
                // if value is ScriptStr, stack over flow will happen
                result.Append(' ', currentIndent);
                /*if (item.Value.Value.Obj is ScriptStr)
                    result.Append($"\"{item.Key}\": {((ScriptStr)item.Value.Value.Obj).Value}");
                else*/
                result.Append($"\"{item.Key}\": {item.Value.Value.Value.ToJsonString(basicIndent, currentIndent)}");
                if (count < Scope.Variables.Count - 1)
                {
                    result.Append(", ");
                    if (basicIndent != 0)
                        result.AppendLine();
                }
                count++;
            }
            if (basicIndent != 0)
            {
                currentIndent -= 2;
                result.Append(' ', currentIndent);
                result.AppendLine();
            }
            result.Append(' ', currentIndent);
            result.Append('}');
            return result.ToString();
        }

        public override string ToString()
        {
            if (this.Scope.Functions.TryGetValue("ToStr", out ScriptFn fn))
            {
                if (!fn.IsBuildIn)
                {
                    var obj = (ScriptObject)fn.Call(Scope).Value;
                    if (obj is ScriptStr)
                        return ((ScriptStr)obj).Value;
                }
            }
            return ToJsonString(2);
        }

        public virtual ScriptObject Copy()
        {
            return new ScriptObject(ObjectClass);
        }
    }
}
