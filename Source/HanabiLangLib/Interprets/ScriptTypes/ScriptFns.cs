using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Parses.Nodes;

namespace HanabiLang.Interprets.ScriptTypes
{
    enum FunctionType
    {
        Normal,
        Get,
        Set
    }

    public class FnParameter
    {
        public string Name { get; private set; }
        public HashSet<ScriptClass> DataTypes { get; private set; }
        public ScriptValue DefaultValue { get; private set; }
        public bool IsMultiArgs { get; private set; }

        public FnParameter(string name, HashSet<ScriptClass> dataTypes = null, ScriptValue defaultValue = null, bool multipleArguments = false)
        {
            this.Name = name;
            this.DataTypes = dataTypes;
            this.DefaultValue = defaultValue;
            this.IsMultiArgs = multipleArguments;
        }

        public FnParameter(string name, ScriptClass dataType, ScriptValue defaultValue = null, bool multipleArguments = false) :
            this(name, dataType == null ? null : new HashSet<ScriptClass> { dataType }, defaultValue, multipleArguments)
        { 
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal class FnTempParameter
    {
        public bool IsMultiArgs { get; private set; }
        public ScriptClass[] DataTypes { get; private set; }
        public ScriptValue Value { get; set; }

        public bool IsValid => Value != null;

        public FnTempParameter(FnParameter fnParameter)
        {
            this.IsMultiArgs = fnParameter.IsMultiArgs;
            this.DataTypes = fnParameter.DataTypes?.ToArray();
            this.Value = fnParameter.DefaultValue;
        }

        public bool CheckType(ScriptValue value)
        {
            if (value.TryFunction != null && this.DataTypes.Contains(BasicTypes.FunctionClass))
                return true;
            ScriptObject obj = value.TryObject;
            if (obj != null && Array.FindIndex(DataTypes, type => obj.IsTypeOrSubOf(type)) >= 0)
                return true;
            return false;
        }
    }

    public class ScriptFn : ScriptType
    {
        public List<FnParameter> Parameters { get; private set; }
        //private Dictionary<string, int> ArgsMap { get; set; }
        internal List<AstNode> Body { get; private set; }
        /// <summary>
        /// The scope when the function is created 
        /// </summary>
        internal ScriptScope Scope { get; private set; }
        public BasicFns.ScriptFnType BuildInFn { get; private set; }
        public bool IsBuildIn => BuildInFn != null;
        public int MinArgs { get; private set; }
        public bool HasMultiArgs => Parameters.Count != 0 && Parameters[Parameters.Count - 1].IsMultiArgs;
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }

        private ScriptFn(List<FnParameter> parameters, List<AstNode> body, ScriptScope scope, BasicFns.ScriptFnType fn, bool isStatic, AccessibilityLevel level)
        {
            this.Parameters = parameters;
            //this.ArgsMap = new Dictionary<string, int>();
            this.Body = body;
            this.Scope = scope;
            this.BuildInFn = fn;
            this.IsStatic = isStatic;
            this.Level = level;
            // Number of arguments with no default value
            this.MinArgs = this.Parameters.Count(x => x.DefaultValue == null);
            if (parameters.Count > 1 && parameters.Skip(1).Any(p => p.Name == parameters[0].Name))
                throw new SystemException("Parameter name cannot be the same");
        }

        internal ScriptFn(List<FnParameter> parameters, ScriptScope scope, BasicFns.ScriptFnType fn, bool isStatic, AccessibilityLevel level) :
            this(parameters, null, scope, fn, isStatic, level)
        { }

        internal ScriptFn(List<FnParameter> parameters, List<AstNode> body, ScriptScope scope, bool isStatic, AccessibilityLevel level) :
            this(parameters, body, scope, null, isStatic, level)
        { }

    }

    public class ScriptFns : ScriptType
    {
        public static readonly string LambdaFnName = "Lambda";
        public string Name { get; private set; }
        public List<ScriptFn> Fns { get; private set; }

        public bool IsLambda => this.Name == LambdaFnName;

        public ScriptFns(string name, IEnumerable<ScriptFn> fns)
        {
            this.Name = name;
            if (string.IsNullOrEmpty(this.Name))
                this.Name = LambdaFnName;
            this.Fns = fns.ToList();
        }

        public ScriptFns(string name, params ScriptFn[] fns)
            : this(name, (IEnumerable<ScriptFn>)fns)
        {
        }

        public int IndexOfOverridableFn(ScriptFn fn) => this.Fns.FindIndex(x =>
        {
            if (x.Parameters.Count != fn.Parameters.Count)
                return false;
            for (int i = 0; i < x.Parameters.Count; i++)
            {
                if (x.Parameters[i].DataTypes == null && fn.Parameters[i].DataTypes == null)
                    continue;
                else if ((x.Parameters[i].DataTypes == null && fn.Parameters[i].DataTypes != null) ||
                    x.Parameters[i].DataTypes != null && fn.Parameters[i].DataTypes == null)
                    return false;
                if (!x.Parameters[i].DataTypes.SetEquals(fn.Parameters[i].DataTypes))
                    return false;
            }
            return true;
        });

        public void AddFn(ScriptFn fn, bool addOverridable)
        {
            int overrideIndex = IndexOfOverridableFn(fn);
            if (overrideIndex == -1)
            {
                this.Fns.Add(fn);
            }
            else if (addOverridable)
            {
                //this.Fns.RemoveAt(overrideIndex);
                //this.Fns.Add(fn);
                this.Fns[overrideIndex] = fn;
            }
        }

        public void AddFns(IEnumerable<ScriptFn> fns, AccessibilityLevel minLevel, bool addOverridable)
        {
            foreach (var fn in fns)
            {
                if (minLevel >= fn.Level)
                    AddFn(fn, addOverridable);
            }
        }

        private static Tuple<List<ScriptValue>, Dictionary<string, ScriptValue>> InterpretArgs(ScriptScope currentScope, ScriptObject _this, List<AstNode> args, Dictionary<string, AstNode> keyArgs)
        {
            List<ScriptValue> resultArgs = new List<ScriptValue>();
            Dictionary<string, ScriptValue> resultKeyArgs = new Dictionary<string, ScriptValue>();
            if (_this != null)
                resultArgs.Add(new ScriptValue(_this));
            foreach (var arg in args)
            {
                ScriptValue value = Interpreter.InterpretExpression(currentScope, arg).Ref;
                if (value.IsUnzipable)
                {
                    resultArgs.AddRange(value.TryUnzipable);
                }
                else
                {
                    resultArgs.Add(value);
                }
            }

            foreach (var kv in keyArgs)
            {
                ScriptValue value = Interpreter.InterpretExpression(currentScope, kv.Value).Ref;
                resultKeyArgs[kv.Key] = value;
            }
            return Tuple.Create(resultArgs, resultKeyArgs);
        }

        private static Tuple<ScriptFn, Dictionary<string, FnTempParameter>, int> FindMinAnyTypeFn(List<Tuple<ScriptFn, Dictionary<string, FnTempParameter>, int>> infos)
        {
            int min = infos[0].Item3;
            int minIndex = 0;

            for (int i = 1; i < infos.Count; ++i)
            {
                if (infos[i].Item3 < min)
                {
                    min = infos[i].Item3;
                    minIndex = i;
                }
            }

            return infos[minIndex];
        }

        private Tuple<ScriptFn, List<ScriptVariable>> FindCallableFnParams(List<ScriptValue> args, Dictionary<string, ScriptValue> keyArgs)
        {
            args = args ?? new List<ScriptValue>();
            keyArgs = keyArgs ?? new Dictionary<string, ScriptValue>();
            int totalArgsCount = args.Count + keyArgs.Count;
            var fns = new List<Tuple<ScriptFn, Dictionary<string, FnTempParameter>, int>>();
            foreach (var fn in this.Fns)
            {
                if ((totalArgsCount < fn.MinArgs || totalArgsCount > fn.Parameters.Count) && !fn.HasMultiArgs)
                    continue;

                var paramsMatch = fn.Parameters.ToDictionary(_param => _param.Name, _param => new FnTempParameter(_param));
                int anyTypeCount = paramsMatch.Values.Sum(x => x.DataTypes == null ? 1 : 0);
                bool isMatchFn = true;

                foreach (var kv in keyArgs)
                {
                    if (!paramsMatch.TryGetValue(kv.Key, out FnTempParameter tempParam))
                    {
                        isMatchFn = false;
                        break;
                    }
                    if (tempParam.DataTypes != null)
                    {
                        if (tempParam.IsMultiArgs)
                        {
                            if ((kv.Value.TryObject?.ClassType != BasicTypes.List) ||
                                (!ScriptList.AsCSharp(kv.Value.TryObject).All(arg => tempParam.CheckType(arg))))
                            {
                                isMatchFn = false;
                                break;
                            }
                        }
                        else
                        {
                            if (!tempParam.CheckType(kv.Value))
                            {
                                isMatchFn = false;
                                break;
                            }
                        }
                    }
                    tempParam.Value = kv.Value;
                }

                if (!isMatchFn)
                    continue;

                int argCount = 0;
                foreach (var _param in paramsMatch)
                {
                    if (argCount >= args.Count)
                        break;

                    if (_param.Value.IsMultiArgs)
                    {
                        var multiArgs = args.Skip(argCount).ToList();
                        if (_param.Value.DataTypes != null && !multiArgs.All(arg => _param.Value.CheckType(arg)))
                        {
                            isMatchFn = false;
                            break;
                        }
                        _param.Value.Value = new ScriptValue(multiArgs);
                    }
                    else
                    {
                        if (_param.Value.DataTypes != null && !_param.Value.CheckType(args[argCount]))
                        {
                            isMatchFn = false;
                            break;
                        }
                        _param.Value.Value = args[argCount];
                    }
                    argCount++;
                }

                if (!isMatchFn)
                    continue;

                FnTempParameter lastParam = paramsMatch.Count > 0 ? paramsMatch.Last().Value : null;

                if (lastParam != null && lastParam.IsMultiArgs)
                {
                    if (lastParam.Value == null)
                        lastParam.Value = new ScriptValue(BasicTypes.List.Create());
                    if (((ScriptObject)lastParam.Value.Value).ClassType != BasicTypes.List)
                    {
                        isMatchFn = false;
                        continue;
                    }
                }

                if (paramsMatch.Values.All(x => x.IsValid))
                {
                    fns.Add(Tuple.Create(fn, paramsMatch, anyTypeCount));
                }
            }

            if (fns.Count <= 0)
            {
                throw new NotImplementedException($"Match function call for {this.Name} does not exist\n" +
                    $"Avaliable Functions: {string.Join(", ", this.Fns.Select(_fn => '(' + string.Join(", ", _fn.Parameters.Select(_params => _params.DataTypes == null ? "any" : string.Join(" | ", _params.DataTypes.Select(_type => _type.Name)))) + ')'))}");
            }

            var scriptfn = FindMinAnyTypeFn(fns);
            return Tuple.Create(scriptfn.Item1, scriptfn.Item2.Select(x => new ScriptVariable(x.Key, null, x.Value.Value, false, false, AccessibilityLevel.Private)).ToList());
        }

        internal virtual Tuple<ScriptFn, List<ScriptVariable>> FindCallableInfo(ScriptScope scope, ScriptObject _this, List<AstNode> args, Dictionary<string, AstNode> keyArgs)
        {
            var kv = InterpretArgs(scope, _this, args, keyArgs);
            return FindCallableFnParams(kv.Item1, kv.Item2);
        }

        internal virtual Tuple<ScriptFn, List<ScriptVariable>> FindCallableInfo(ScriptObject _this, List<ScriptValue> args, Dictionary<string, ScriptValue> keyArgs)
        {
            List<ScriptValue> resultArgs = new List<ScriptValue>();
            if (_this != null)
                resultArgs.Add(new ScriptValue(_this));
            foreach (var value in args)
            {
                if (value.IsUnzipable)
                {
                    resultArgs.AddRange(value.TryUnzipable);
                }
                else
                {
                    resultArgs.Add(value);
                }
            }
            return FindCallableFnParams(resultArgs, keyArgs);
        }

        internal virtual Tuple<ScriptFn, List<ScriptVariable>> FindCallableInfo(ScriptObject _this, params ScriptValue[] values)
        {
            List<ScriptValue> resultArgs = new List<ScriptValue>();
            if (_this != null)
                resultArgs.Add(new ScriptValue(_this));
            foreach (var value in values)
            {
                if (value.IsUnzipable)
                {
                    resultArgs.AddRange(value.TryUnzipable);
                }
                else
                {
                    resultArgs.Add(value);
                }
            }
            return FindCallableFnParams(resultArgs, null);
        }

        internal ScriptValue Call(Tuple<ScriptFn, List<ScriptVariable>> callableInfo)
        {
            return Call(callableInfo.Item1, callableInfo.Item2);
        }

        public virtual ScriptValue Call(ScriptObject _this, params ScriptValue[] value)
        {
            var fnInfo = FindCallableInfo(_this, value);
            return Call(fnInfo.Item1, fnInfo.Item2);
        }

        private ScriptValue Call(ScriptFn fn, List<ScriptVariable> args)
        {
            if (fn.IsBuildIn)
            {
                List<ScriptValue> valueArgs = args.Select(arg => arg.Value).ToList();
                return fn.BuildInFn(valueArgs);
            }

            // if it is a object call, set the scope to object
            // else if it is a static class call or it is a normal scope call, set the scope to the class scope / normal scope
            // ScriptScope parentScope = _this == null ? fn.Scope : _this.Scope;
            ScriptScope parentScope = fn.Scope;
            /*ScriptScope parentScope = fn.Scope;
            if (_this != null)
            {
                parentScope = _this.Scope.Copy();
                parentScope.Parent = _this.Scope.ClassScope;
            }*/

            var fnScope = new ScriptScope(fn, parentScope);

            foreach (var variable in args)
            {
                fnScope.Variables[variable.Name] = variable;
            }


            foreach (var node in fn.Body)
            {
                if (node is ReturnNode)
                {
                    var returnNode = (ReturnNode)node;

                    if (returnNode.Value != null)
                    {
                        var value = Interpreter.InterpretExpression(fnScope, returnNode.Value);
                        return value.Ref;
                    }
                    return ScriptValue.Null;
                }
                if (Interpreter.IsStatementNode(node))
                {
                    var value = Interpreter.InterpretStatement(fnScope, node);
                    if (!value.IsEmpty)
                    {
                        return value.Ref;
                    }
                }
                else
                {
                    Interpreter.InterpretChild(fnScope, node, false);
                }
            }
            return new ScriptValue();
        }

        public override string ToString()
        {
            return $"<function: {this.Name}>";
        }
    }

    public class ScriptBoundFns : ScriptFns
    {
        public ScriptObject BoundObject { get; private set; }
        public AccessibilityLevel AccessLevel { get; private set; }

        public ScriptBoundFns(ScriptFns scriptFns, ScriptObject scriptObject, AccessibilityLevel accessLevel) : base(scriptFns.Name, scriptFns.Fns.Where(fn => accessLevel >= fn.Level))
        {
            this.BoundObject = scriptObject;
            this.AccessLevel = accessLevel;
            if (this.Fns.Count <= 0)
                throw new SystemException($"No suitable function for {AccessLevel} access");
        }

        internal override Tuple<ScriptFn, List<ScriptVariable>> FindCallableInfo(ScriptScope scope, ScriptObject _this, List<AstNode> args, Dictionary<string, AstNode> keyArgs)
        {
            return base.FindCallableInfo(scope, this.BoundObject, args, keyArgs);
        }

        internal override Tuple<ScriptFn, List<ScriptVariable>> FindCallableInfo(ScriptObject _this, List<ScriptValue> args, Dictionary<string, ScriptValue> keyArgs)
        {
            return base.FindCallableInfo(this.BoundObject, args, keyArgs);
        }

        internal override Tuple<ScriptFn, List<ScriptVariable>> FindCallableInfo(ScriptObject _this, params ScriptValue[] values)
        {
            return base.FindCallableInfo(this.BoundObject, values);
        }

        public override ScriptValue Call(ScriptObject _this, params ScriptValue[] value)
        {
            return base.Call(this.BoundObject, value);
        }

        public override string ToString()
        {
            return $"<function: {this.Name} (object: {(BoundObject?.ClassType?.Name ?? "null")})>";
        }
    }
}
