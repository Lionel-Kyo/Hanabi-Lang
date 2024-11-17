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
        public string ConstructorName => $"{this.Name}::New";
        public string Name { get; private set; }

        /// <summary>
        /// With non static VariableDefinitionNode / ScriptVariable only
        /// </summary>
        internal List<object> Body { get; private set; }
        internal ScriptScope Scope { get; private set; }
        public List<ScriptClass> SuperClasses { get; protected set; }
        public ScriptClass SuperClass { get; protected set; }
        public ScriptFns BuildInConstructor { get; private set; }
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }
        public bool IsBuildIn { get; private set; }

        internal ScriptClass(string name, List<AstNode> body, ScriptScope currentScope, List<ScriptClass> definedSuperClasses,
            bool isStatic, AccessibilityLevel level, bool isImported = false)
        {
            this.Name = name;
            this.IsStatic = isStatic;
            this.IsBuildIn = body == null;

            if (isImported)
                this.Scope = currentScope;
            else
                this.Scope = new ScriptScope(this, currentScope);

            if (!this.IsStatic && !this.Name.Equals("object"))
            {
                if (definedSuperClasses == null || definedSuperClasses.Count <= 0)
                    definedSuperClasses = new List<ScriptClass>() { BasicTypes.ObjectClass };

                this.SuperClass = new ScriptClass($"super_{name}", true);
                this.SuperClass.Level = AccessibilityLevel.Private;
                this.SuperClass.Body = new List<object>();

                foreach (var _class in ((IEnumerable<ScriptClass>)definedSuperClasses).Reverse())
                { 
                    if (_class.BuildInConstructor.Fns.Count != 0)
                        throw new SystemException("Inherit from C# class is not supported");

                    if (_class.SuperClass != null)
                        CopyClassMember(_class.SuperClass, this.SuperClass, true);

                    CopyClassMember(_class, this.SuperClass, true);
                }
                this.SuperClasses = GetAllSuperClassesFromDefinedSuperClasses(definedSuperClasses).ToList();
            }

            this.BuildInConstructor = new ScriptFns(this.ConstructorName);
            this.Level = level;

            if (body != null && !isImported)
            {
                this.Body = new List<object>();
                foreach (var bodyNode in body)
                {
                    if (bodyNode is FnDefineNode || bodyNode is ClassDefineNode)
                    {
                        Interpreter.InterpretChild(this.Scope, bodyNode, false);
                    }
                    else if (bodyNode is VariableDefinitionNode)
                    {
                        if (((VariableDefinitionNode)bodyNode).IsStatic)
                        {
                            Interpreter.InterpretChild(this.Scope, bodyNode, false);
                        }
                        else
                        {
                            this.Body.Add(bodyNode);
                        }
                    }
                }
            }

            if (this.Body == null)
                this.Body = new List<object>();

            if (this.SuperClass != null)
                CopyClassMember(this.SuperClass, this, false);
        }

        private static void CopyClassMember(ScriptClass from, ScriptClass to, bool replaceMember)
        {
            foreach (var fns in from.Scope.Functions)
            {
                // Change constructor name
                string fnName = fns.Key.Equals(from.ConstructorName) ? to.ConstructorName : fns.Key;
                //string fnName = fns.Key;
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
                Func<object, string> getBodyName = x => (x is VariableDefinitionNode ? ((VariableDefinitionNode)x).Name : ((ScriptVariable)x).Name);
                foreach (object instanceVariable in from.Body)
                {
                    int varNameIndex = to.Body.FindIndex(x => getBodyName(x).Equals(getBodyName(instanceVariable)));
                    if (varNameIndex == -1)
                        to.Body.Add(instanceVariable);
                    else if (replaceMember)
                        to.Body[varNameIndex] = instanceVariable;
                }
            }
        }

        private static HashSet<ScriptClass> GetAllSuperClassesFromDefinedSuperClasses(List<ScriptClass> definedSuperClasses)
        {
            if (definedSuperClasses == null)
                return null;

            HashSet<ScriptClass> result = new HashSet<ScriptClass>();
            foreach (ScriptClass superClass in definedSuperClasses)
            {
                result.Add(superClass);
                var classes = GetAllSuperClassesFromDefinedSuperClasses(superClass.SuperClasses);
                if (classes != null)
                {
                    foreach (ScriptClass interClass in classes)
                    {
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

        public bool TryGetValue(string name, out ScriptType value)
        {
            if (this.Scope.TryGetValue(name, out value))
                return true;
            return false;
        }

        protected void AddFunction(string name, List<FnParameter> parameters, BasicFns.ScriptFnType fn,
            bool isStatic = false, AccessibilityLevel level = AccessibilityLevel.Public)
        {
            if (!this.Scope.Functions.TryGetValue(name, out ScriptFns scriptFns))
            {
                scriptFns = new ScriptFns(name);
                this.Scope.Functions[name] = scriptFns;
            }
            //scriptFns.Fns.Add(new ScriptFn(parameters, this.Scope, fn, isStatic, level));
            // override function
            scriptFns.AddFn(new ScriptFn(parameters, this.Scope, fn, isStatic, level), true);
        }

        protected void AddVariable(string name, bool isStatic, HashSet<ScriptClass> dataTypes)
        {
            var variable = new ScriptVariable(name, null, new ScriptValue(), false, isStatic, AccessibilityLevel.Public);

            if (isStatic)
            {
                // this.Scope.Variables.Add(name, variable);
                // replace variable
                this.Scope.Variables[name] =  variable;
            }
            else
            {
                if (this.Body == null)
                    this.Body = new List<object> { variable };
                else
                    this.Body.Add(variable);
            }
        }

        protected void AddVariable(string name, BasicFns.ScriptFnType getFn, BasicFns.ScriptFnType setFn, bool isStatic, HashSet<ScriptClass> dataTypes)
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
                setFns.Fns.Add(new ScriptFn(new List<FnParameter>() { new FnParameter("value", dataTypes) }, null, setFn, isStatic, AccessibilityLevel.Public));
            }

            var variable = new ScriptVariable(name, null, getFns, setFns, false, isStatic, AccessibilityLevel.Public);

            if (isStatic)
            {
                // this.Scope.Variables.Add(name, variable);
                // replace variable
                this.Scope.Variables[name] = variable;
            }
            else
            {
                if (this.Body == null)
                    this.Body = new List<object>{ variable }; 
                else
                    this.Body.Add(variable); 
            }
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

            if (this.Scope.Functions.TryGetValue(ConstructorName, out ScriptFns currentConstructor))
            {
                var fnInfo = currentConstructor.FindCallableInfo(currentScope, args, keyArgs);
                currentConstructor.Call(_object, fnInfo);
            }
            else
            {
                if (args.Count > 0 || keyArgs.Count > 0)
                    throw new NotImplementedException($"Match function call for {ConstructorName} does not exist\nAvaliable Function: ()");
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
