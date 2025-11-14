using HanabiLangLib.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLangLib.Parses.Nodes;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptClass : ScriptType
    {
        public static readonly string OPEARTOR_BIT_NOT = "__OP_BIT_NOT__";
        public static readonly string OPEARTOR_BIT_AND = "__OP_BIT_AND__";
        public static readonly string OPEARTOR_BIT_OR = "__OP_BIT_OR__";
        public static readonly string OPEARTOR_BIT_XOR = "__OP_BIT_XOR__";
        public static readonly string OPEARTOR_BIT_LEFT_SHIFT = "__OP_BIT_LEFT_SHIFT__";
        public static readonly string OPEARTOR_BIT_RIGHT_SHIFT = "__OP_BIT_RIGHT_SHIFT__";
        public static readonly string OPEARTOR_NOT = "__OP_NOT__";
        public static readonly string OPEARTOR_AND = "__OP_AND__";
        public static readonly string OPEARTOR_OR = "__OP_OR__";
        public static readonly string OPEARTOR_POSITIVE = "__OP_POSITIVE__";
        public static readonly string OPEARTOR_NEGATIVE = "__OP_NEGATIVE__";
        public static readonly string OPEARTOR_ADD = "__OP_ADD__";
        public static readonly string OPEARTOR_MINUS = "__OP_MINUS__";
        public static readonly string OPEARTOR_MULTIPLY = "__OP_MULTIPLY__";
        public static readonly string OPEARTOR_DIVIDE = "__OP_DIVIDE__";
        public static readonly string OPEARTOR_MUDULO = "__OP_MUDULO__";
        public static readonly string OPEARTOR_LARGER = "__OP_LARGER__";
        public static readonly string OPEARTOR_LARGER_EQUALS = "__OP_LARGER_EQUALS__";
        public static readonly string OPEARTOR_LESS = "__OP_LESS__";
        public static readonly string OPEARTOR_LESS_EQUALS = "__OP_LESS_EQUALS__";
        public static readonly string OPEARTOR_EQUALS = "__OP_EQUALS__";
        public static readonly string OPEARTOR_NOT_EQUALS = "__OP_NOT_EQUALS__";

        public static readonly string TO_STR = "ToStr";
        public static readonly string GET_INDEXER = "__GET_INDEXER__";
        public static readonly string SET_INDEXER = "__SET_INDEXER__";
        public static readonly string GET_ITERATOR = "__ITER__";

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

        /// <summary>
        /// For subclass copy only 
        /// </summary>
        protected ScriptClass(string name, List<object> body, ScriptScope scope, List<ScriptClass> superClasses, ScriptClass SuperClass,
            ScriptFns buildInConstructor, bool isStatic, AccessibilityLevel level, bool IsBuildIn)
        {
            this.Name = name;
            this.Body = body;
            this.Scope = scope;
            this.SuperClasses = superClasses;
            this.SuperClass = SuperClass;
            this.BuildInConstructor = buildInConstructor;
            this.IsStatic = isStatic;
            this.Level = level;
            this.IsBuildIn = IsBuildIn;
        }

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

            if (!this.IsStatic && !(this.Name.Equals("object") || this.Name.Equals("null")))
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
                        CopyClassMember(_class.SuperClass, this.SuperClass, AccessibilityLevel.Protected, true, false);

                    CopyClassMember(_class, this.SuperClass, AccessibilityLevel.Protected, true, false);
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
                CopyClassMember(this.SuperClass, this, AccessibilityLevel.Protected, false, true);
        }

        private static void CopyClassMember(ScriptClass from, ScriptClass to, AccessibilityLevel minLevel, bool replaceMember, bool skipConstructor)
        {
            foreach (var variable in from.Scope.Variables)
            {
                if (variable.Value.Value.IsFunction)
                {
                    var fns = variable.Value.Value.TryFunction;
                    bool isConstructor = variable.Key.Equals(from.ConstructorName);

                    if (isConstructor && skipConstructor)
                        continue;

                    // Change constructor name
                    string fnName = isConstructor ? to.ConstructorName : variable.Key;

                    ScriptFns scriptFns = null;
                    if (to.Scope.Variables.TryGetValue(fnName, out ScriptVariable scriptVar))
                    {
                        scriptFns = scriptVar.Value.TryFunction;
                    }
                    if (scriptFns == null)
                    {
                        scriptFns = new ScriptFns(fnName);
                        scriptVar = new ScriptVariable(fnName, scriptFns, AccessibilityLevel.Public);
                        to.Scope.Variables[fnName] = scriptVar;
                    }
                    scriptFns.AddFns(fns.Fns, minLevel, replaceMember);
                }
                else if (variable.Value.Value.IsClass)
                {
                    // skip
                }
                else
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
            }

            if (from.Body != null)
            {
                Func<object, string> getBodyName = x => (x is VariableDefinitionNode ? ((VariableDefinitionNode)x).Names[0] : ((ScriptVariable)x).Name);
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

        public bool TryGetValue(string name, out ScriptVariable value)
        {
            if (this.Scope.TryGetValue(name, out value))
                return true;
            return false;
        }

        /// <summary>
        /// Internal add function only, script interpretation won't use
        /// </summary>
        protected void AddFunction(string name, List<FnParameter> parameters, BasicFns.ScriptFnType fn,
            bool isStatic = false, AccessibilityLevel level = AccessibilityLevel.Public)
        {
            ScriptFns scriptFns = null;
            if (!isStatic && (parameters.Count <= 0 || parameters[0].Name != "this"))
                throw new SystemException($"Non static fn require this as first parameter");

            if (isStatic && parameters.FindIndex(p => p.Name == "this") >= 0)
                throw new SystemException($"static fn cannot define this as parameter");

            if (this.Scope.Variables.TryGetValue(name, out ScriptVariable scriptVar))
            {
                if (scriptVar.Value.IsFunction)
                {
                    scriptFns = scriptVar.Value.TryFunction;
                }
                else
                {
                    if (scriptVar.IsConstant)
                        throw new SystemException($"Cannot define fn {name} to constant variable.");
                }
            }
            if (scriptFns == null)
            {
                scriptFns = new ScriptFns(name);
                scriptVar = new ScriptVariable(name, scriptFns, AccessibilityLevel.Public);
                this.Scope.Variables[name] = scriptVar;
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
                var parameters = new List<FnParameter>();
                if (!isStatic)
                    parameters.Add(new FnParameter("this"));
                getFns.Fns.Add(new ScriptFn(parameters, null, getFn, isStatic, AccessibilityLevel.Public));
            }

            if (setFn != null)
            {
                setFns = new ScriptFns($"set_{name}");
                var parameters = new List<FnParameter>() { new FnParameter("value", dataTypes) };
                if (!isStatic)
                    parameters.Insert(0, new FnParameter("this"));
                setFns.Fns.Add(new ScriptFn(parameters, null, setFn, isStatic, AccessibilityLevel.Public));
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
                var fnInfo = this.BuildInConstructor.FindCallableInfo(currentScope, null, args, keyArgs);
                return this.BuildInConstructor.Call(fnInfo);
            }

            ScriptObject _object = Create();

            if (this.Scope.Variables.TryGetValue(ConstructorName, out ScriptVariable currentConstructor) && currentConstructor.Value.IsFunction)
            {
                var fnInfo = currentConstructor.Value.TryFunction.FindCallableInfo(currentScope, _object, args, keyArgs);
                currentConstructor.Value.TryFunction.Call(fnInfo);
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

    public class ScriptBoundSuperClass : ScriptClass
    {
        public ScriptObject BoundObject { get; private set; }
        public ScriptBoundSuperClass(ScriptClass scriptClass, ScriptObject scriptObject) : 
            base(scriptClass.Name, scriptClass.Body, scriptClass.Scope, scriptClass.SuperClasses, scriptClass.SuperClass,
                scriptClass.BuildInConstructor, scriptClass.IsStatic, scriptClass.Level, scriptClass.IsBuildIn)
        {
            this.BoundObject = scriptObject;
        }

        public override string ToString()
        {
            return $"<class: {this.Name} (object: {BoundObject.ClassType.Name})>";
        }
    }
}
