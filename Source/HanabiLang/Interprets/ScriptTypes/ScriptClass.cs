using HanabiLang.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Parses.Nodes;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptClass : ScriptType
    {
        public string Name { get; private set; }
        public List<AstNode> Body { get; private set; }
        public ScriptScope Scope { get; private set; }
        public ScriptFns BuildInConstructor { get; private set; }
        public bool IsStatic { get; private set; }
        public bool IsBuildIn => this.Body == null;

        public ScriptClass(string name, List<AstNode> body,
            ScriptScope scope, bool isStatic, bool ignoreInitialize = false)
        {
            this.Name = name;
            this.Body = body;
            this.Scope = scope;
            this.BuildInConstructor = new ScriptFns(this.Name);
            this.IsStatic = isStatic;
            this.AddBasicFns();

            if (this.Body != null && !ignoreInitialize)
            {
                foreach (var bodyNode in this.Body)
                {
                    if (bodyNode is FnDefineNode || bodyNode is ClassDefineNode)
                    {
                        Interpreter.InterpretChild(scope, bodyNode);
                    }
                    else if (bodyNode is VariableDefinitionNode)
                    {
                        if (((VariableDefinitionNode)bodyNode).IsConstant)
                        {
                            Interpreter.InterpretChild(scope, bodyNode);
                        }
                    }
                }
            }
        }

        protected void AddObjectFn(string name, List<FnParameter> parameters, BuildInFns.ScriptFnType fn)
        {
            if (!this.Scope.Functions.TryGetValue(name, out ScriptFns scriptFns))
            {
                scriptFns = new ScriptFns(name);
                this.Scope.Functions[name] = scriptFns;
            }
            scriptFns.Fns.Add(new ScriptFn(parameters, null, this.Scope, fn));
        }

        private void AddBasicFns()
        {
            AddObjectFn("ToStr", new List<FnParameter>(), 
                args => new ScriptValue(ToStr((ScriptObject)args[0].Value)));
        }

        public virtual ScriptObject Not(ScriptObject left)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("!", out ScriptFns fn))
                return (ScriptObject)fn.Call(left).Value;
            throw new SystemException("Operator ! is not implemented");
        }
        public virtual ScriptObject Positive(ScriptObject left)
        {
            if (left.Scope.Functions.TryGetValue("^+", out ScriptFns fn))
                return (ScriptObject)fn.Call(left).Value;
            throw new SystemException("Operator ! is not implemented");
        }
        public virtual ScriptObject Negative(ScriptObject left)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("^-", out ScriptFns fn))
                return (ScriptObject)fn.Call(left).Value;
            throw new SystemException("Operator ! is not implemented");
        }
        public virtual ScriptObject Add(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("+", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator + is not implemented");
        }
        public virtual ScriptObject Minus(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("-", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator - is not implemented");
        }
        public virtual ScriptObject Multiply(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("*", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator * is not implemented");
        }
        public virtual ScriptObject Divide(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("/", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator / is not implemented");
        }
        public virtual ScriptObject Modulo(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("%", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator ! is not implemented");
        }
        public virtual ScriptObject Larger(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue(">", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator > is not implemented");
        }
        public virtual ScriptObject LargerEquals(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue(">=", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator >= is not implemented");
        }
        public virtual ScriptObject Less(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("<", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator < is not implemented");
        }
        public virtual ScriptObject LessEquals(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("<=", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator <= is not implemented");
        }
        public virtual ScriptObject And(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("&&", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator && is not implemented");
        }
        public virtual ScriptObject Or(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("||", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            throw new SystemException("Operator || is not implemented");
        }
        public virtual ScriptObject Equals(ScriptObject left, ScriptObject right)
        {
            if (left.ClassType.Scope.Functions.TryGetValue("==", out ScriptFns fn))
                return (ScriptObject)fn.Call(left, new ScriptValue(right)).Value;
            return BasicTypes.Bool.Create(base.Equals(right));
        }

        public virtual ScriptObject ToStr(ScriptObject _this)
        {
            if (_this.BuildInObject != null)
                return BasicTypes.Str.Create(_this.BuildInObject.ToString());
            return BasicTypes.Str.Create($"<object: {this.Name}>");
        }

        public virtual string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
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

                ScriptType scriptType = item.Value.Value.Value;
                if (!(scriptType is ScriptObject))
                    throw new SystemException($"{scriptType} is not a object");
                ScriptObject scriptObject = (ScriptObject)scriptType;
                result.Append(' ', currentIndent);

                result.Append($"\"{item.Key}\": {scriptObject.ClassType.ToJsonString(scriptObject, basicIndent, currentIndent)}");
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

        /*public override string ToString()
        {
            if (this.Scope.Functions.TryGetValue("ToStr", out ScriptFn fn))
            {
                if (!fn.IsBuildIn)
                {
                    var obj = (ScriptObject)fn.Call(Scope).Value;
                    if (obj.ClassType is ScriptStr)
                        return (string)obj.BuildInObject;
                }
            }
            return this.ToJsonString(0);
        }*/

        public override string ToString()
        {
            return $"<class: {this.Name}>";
        }

        public virtual ScriptObject Create() => new ScriptObject(this);

        public ScriptValue Call(ScriptScope currentScope, Dictionary<string, AstNode> callArgs)
        {
            if (this.IsStatic)
                throw new SystemException($"Static class({this.Name}) cannot create an object");

            if (this.BuildInConstructor.Fns.Count != 0)
            {
                return this.BuildInConstructor.Call(currentScope, null, callArgs);
            }

            ScriptObject _object = Create();
            

            // Data Class
            /*var index = 0;
            foreach (var parameter in this.Constructor)
            {
                classScope.Variables[parameter] = new InterpretedVariable(parameter,
                                                  interpreter.InterpretExpression(
                                                          callNode.Args[index]).Ref, false);
                index++;
            }*/


            if (this.Body != null)
            { 
                foreach (var bodyNode in this.Body)
                {
                    if (bodyNode is VariableDefinitionNode)
                    {
                        Interpreter.InterpretChild(_object.Scope, bodyNode);
                    }
                    // interpreter.InterpretChild(bodyNode);
                }
            }

            // A Function with same name as class is constructor
            ScriptFns currentConstructor;
            
            if (this.Scope.Functions.TryGetValue(this.Name, out currentConstructor))
            {
                currentConstructor.Call(currentScope, _object, callArgs);
            }

            return new ScriptValue(_object);
        }

        /*public override string ToJsonString(int basicIndent = 2, int currentIndent = 0)
        {
            return new string(' ', currentIndent) + $"\"Class\": \"{Name}\"";
        }

        public override string ToString()
        {
            return $"Class: {Name}";
        }*/
    }
}
