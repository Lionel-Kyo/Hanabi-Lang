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
    public class ScriptClass : ScriptType
    {
        public string Name { get; private set; }
        // With non static variable definition node only
        internal List<AstNode> Body { get; private set; }
        internal ScriptScope Scope { get; private set; }
        public List<ScriptClass> SuperClasses { get; protected set; }
        public ScriptClass SuperClass { get; protected set; }
        public ScriptFns BuildInConstructor { get; private set; }
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }
        public bool IsBuildIn => this.Body == null;

        internal ScriptClass(string name, List<AstNode> body, ScriptScope currentScope, List<ScriptClass> superClasses,
            bool isStatic, AccessibilityLevel level, bool isImported = false)
        {
            this.Name = name;
            this.IsStatic = isStatic;
            if (isImported)
                this.Scope = currentScope;
            else
                this.Scope = new ScriptScope(this, currentScope);

            if (!this.IsStatic && !this.Name.Equals("object"))
            {
                this.SuperClasses = superClasses;

                if (this.SuperClasses == null || this.SuperClasses.Count <= 0)
                    this.SuperClasses = new List<ScriptClass>() { BasicTypes.ObjectClass };

                this.SuperClass = new ScriptClass($"super_{name}", true);
                this.SuperClass.Level = AccessibilityLevel.Private;
                this.SuperClass.Body = new List<AstNode>();

                foreach (var _class in this.SuperClasses)
                { 
                    if (_class.BuildInConstructor.Fns.Count != 0)
                        throw new SystemException("Inherit from C# class is not supported");

                    if (_class.SuperClass != null)
                        AddClassMember(_class.SuperClass, this.SuperClass, true);

                    AddClassMember(_class, this.SuperClass, true);
                }

            }

            this.BuildInConstructor = new ScriptFns(this.Name);
            this.Level = level;

            if (body != null && !isImported)
            {
                this.Body = new List<AstNode>();
                foreach (var bodyNode in body)
                {
                    if (bodyNode is FnDefineNode || bodyNode is ClassDefineNode)
                    {
                        Interpreter.InterpretChild(this.Scope, bodyNode);
                    }
                    else if (bodyNode is VariableDefinitionNode)
                    {
                        if (((VariableDefinitionNode)bodyNode).IsStatic)
                        {
                            Interpreter.InterpretChild(this.Scope, bodyNode);
                        }
                        else
                        {
                            this.Body.Add(bodyNode);
                        }
                    }
                }
            }

            if (this.SuperClass != null)
                AddClassMember(this.SuperClass, this, false);
        }

        private static void AddClassMember(ScriptClass from, ScriptClass to, bool replaceMember)
        {
            foreach (var fns in from.Scope.Functions)
            {
                // Console.WriteLine($"{from.Name} -> {to.Name} ({fns.Key})");
                string fnName = fns.Key.Equals(from.Name) ? to.Name : fns.Key;
                if (!to.Scope.Functions.TryGetValue(fnName, out ScriptFns scriptFns))
                {
                    scriptFns = new ScriptFns(fnName);
                    to.Scope.Functions[fnName] = scriptFns;
                }
                scriptFns.AddFns(fns.Value.Fns, replaceMember);
            }

            foreach (var variable in from.Scope.Variables)
            {
                if (to.Scope.Variables.ContainsKey(variable.Key))
                {
                    if (replaceMember)
                        to.Scope.Variables[variable.Key] = variable.Value;
                }
                else
                {
                    to.Scope.Variables[variable.Key] = variable.Value;
                }
            }

            if (from.Body != null)
            {
                foreach (VariableDefinitionNode variableDefine in from.Body)
                {
                    int varNameIndex = to.Body.FindIndex
                        (x => ((VariableDefinitionNode)x).Name.Equals(variableDefine.Name));
                    if (varNameIndex == -1)
                        to.Body.Add(variableDefine);
                    else if (replaceMember)
                        to.Body[varNameIndex] = variableDefine;
                }
            }
        }

        private static List<ScriptClass> GetSuperClasses(ScriptClass _class)
        {
            if (_class.SuperClasses == null)
                return null;
            List<ScriptClass> result = new List<ScriptClass>();
            foreach (ScriptClass superClass in _class.SuperClasses)
            {
                result.Add(superClass);
                var classes = GetSuperClasses(superClass);
                if (classes != null)
                {
                    foreach (ScriptClass interClass in classes)
                    {
                        if (!result.Contains(interClass))
                            result.Add(interClass);
                    }
                }
                    
            }
            return result;
        }

        public ScriptClass(string name, List<ScriptClass> superClasses, bool isStatic, AccessibilityLevel level) :
            this(name, null, null, superClasses, isStatic, level, false)
        { }
        public ScriptClass(string name, List<ScriptClass> superClasses, bool isStatic) :
            this(name, null, null, superClasses, isStatic, AccessibilityLevel.Public, false)
        { }
        public ScriptClass(string name, bool isStatic) :
            this(name, null, null, null, isStatic, AccessibilityLevel.Public, false)
        { }
        public ScriptClass(string name, bool isStatic, AccessibilityLevel level) :
            this(name, null, null, null, isStatic, level, false)
        { }

        protected void AddObjectFn(string name, List<FnParameter> parameters, BuildInFns.ScriptFnType fn,
            bool isStatic = false, AccessibilityLevel level = AccessibilityLevel.Public)
        {
            if (!this.Scope.Functions.TryGetValue(name, out ScriptFns scriptFns))
            {
                scriptFns = new ScriptFns(name);
                this.Scope.Functions[name] = scriptFns;
            }
            scriptFns.Fns.Add(new ScriptFn(parameters, this.Scope, fn, isStatic, level));
        }

        protected void AddVariable(string name, BuildInFns.ScriptFnType getFn, BuildInFns.ScriptFnType setFn, bool isStatic, ScriptClass dataType)
        {
            ScriptFns getFns = null;
            ScriptFns setFns = null;

            if (getFn != null)
            {
                getFns = new ScriptFns($"get_{name}");
                getFns.Fns.Add(new ScriptFn(new List<FnParameter>(), null, getFn, isStatic, AccessibilityLevel.Public));
            }

            if (setFn != null)
            {
                setFns = new ScriptFns($"set_{name}");
                setFns.Fns.Add(new ScriptFn(new List<FnParameter>() { new FnParameter("value", dataType) }, null, getFn, isStatic, AccessibilityLevel.Public));
            }

            this.Scope.Variables.Add(name, new ScriptVariable(name, getFns, setFns, false, isStatic, AccessibilityLevel.Public));
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
                return (ScriptObject)fn.Call(left, fn.FindCallableInfo()).Value;
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
            return BasicTypes.Str.Create($"<object: {_this.ClassType.Name}>");
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

        internal ScriptValue Call(ScriptScope currentScope, List<AstNode> args, Dictionary<string, AstNode> keyArgs)
        {
            if (this.IsStatic)
                throw new SystemException($"Static class({this.Name}) cannot create an object");

            // C# class
            if (this.BuildInConstructor.Fns.Count != 0)
            {
                var fnInfo = this.BuildInConstructor.FindCallableInfo(currentScope, args, keyArgs);
                return this.BuildInConstructor.Call(null, fnInfo);
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
                    Interpreter.InterpretChild(_object.Scope, bodyNode);
                }
            }

            // A Function with same name as class is constructor
            ScriptFns currentConstructor;
            
            if (this.Scope.Functions.TryGetValue(this.Name, out currentConstructor))
            {
                var fnInfo = currentConstructor.FindCallableInfo(currentScope, args, keyArgs);
                currentConstructor.Call(_object, fnInfo);
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
