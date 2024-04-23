﻿using HanabiLang.Parses;
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
        public HashSet<ScriptClass> DataTypes { get; private set; }
        public ScriptValue Value { get; set; }

        public bool IsValid => Value != null;

        public FnTempParameter(FnParameter fnParameter)
        {
            this.IsMultiArgs = fnParameter.IsMultiArgs;
            this.DataTypes = fnParameter.DataTypes;
            this.Value = fnParameter.DefaultValue;
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
        public BuildInFns.ScriptFnType BuildInFn { get; private set; }
        public bool IsBuildIn => BuildInFn != null;
        public int MinArgs { get; private set; }
        public bool HasMultiArgs => Parameters.Count != 0 && Parameters[Parameters.Count - 1].IsMultiArgs;
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }

        private ScriptFn(List<FnParameter> parameters, List<AstNode> body, ScriptScope scope, BuildInFns.ScriptFnType fn, bool isStatic, AccessibilityLevel level)
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
        }

        internal ScriptFn(List<FnParameter> parameters, ScriptScope scope, BuildInFns.ScriptFnType fn, bool isStatic, AccessibilityLevel level) :
            this(parameters, null, scope, fn, isStatic, level)
        { }

        internal ScriptFn(List<FnParameter> parameters, List<AstNode> body, ScriptScope scope, bool isStatic, AccessibilityLevel level) :
            this(parameters, body, scope, null, isStatic, level)
        { }

    }

    public class ScriptFns : ScriptType
    {
        public string Name { get; private set; }

        public List<ScriptFn> Fns { get; private set; }
        public ScriptFns(string name, params ScriptFn[] fns)
        {
            this.Name = name;
            if (string.IsNullOrEmpty(this.Name))
                this.Name = "Lambda";
            this.Fns = new List<ScriptFn>();
            this.Fns.AddRange(fns);
        }

        public int IndexOfOverridableFn(ScriptFn fn) => this.Fns.FindIndex(x =>
        {
            if (x.Parameters.Count != fn.Parameters.Count)
                return false;
            for (int i = 0; i < x.Parameters.Count; i++)
            {
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
                this.Fns.RemoveAt(overrideIndex);
                this.Fns.Add(fn);
            }
        }

        public void AddFns(IEnumerable<ScriptFn> fns, bool addOverridable)
        {
            foreach (var fn in fns)
            {
                AddFn(fn, addOverridable);
            }
        }

        private static Tuple<List<ScriptValue>, Dictionary<string, ScriptValue>> InterpretArgs(ScriptScope currentScope, List<AstNode> args, Dictionary<string, AstNode> keyArgs)
        {
            List<ScriptValue> resultArgs = new List<ScriptValue>();
            Dictionary<string, ScriptValue> resultKeyArgs = new Dictionary<string, ScriptValue>();
            foreach (var arg in args)
            {
                ScriptValue value = Interpreter.InterpretExpression(currentScope, arg).Ref;
                if (value.Value is SingleUnzipList)
                {
                    SingleUnzipList singleUnzipList = (SingleUnzipList)value.Value;
                    resultArgs.AddRange(singleUnzipList.Value);
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
                            if ((((ScriptObject)kv.Value.Value).ClassType != BasicTypes.List) ||
                                (!((List<ScriptValue>)((ScriptObject)kv.Value.Value).BuildInObject).All(arg => tempParam.DataTypes.Contains(((ScriptObject)arg.Value).ClassType))))
                            {
                                isMatchFn = false;
                                break;
                            }
                        }
                        else
                        {
                            if (!tempParam.DataTypes.Contains(((ScriptObject)kv.Value.Value).ClassType))
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
                        if (_param.Value.DataTypes != null && !multiArgs.All(arg => _param.Value.DataTypes.Contains(((ScriptObject)arg.Value).ClassType)))
                        {
                            isMatchFn = false;
                            break;
                        }
                        _param.Value.Value = new ScriptValue(multiArgs);
                    }
                    else
                    {
                        if (_param.Value.DataTypes != null && !_param.Value.DataTypes.Contains(((ScriptObject)args[argCount].Value).ClassType))
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

        internal Tuple<ScriptFn, List<ScriptVariable>> FindCallableInfo(ScriptScope scope, List<AstNode> args, Dictionary<string, AstNode> keyArgs)
        {
            var kv = InterpretArgs(scope, args, keyArgs);
            return FindCallableFnParams(kv.Item1, kv.Item2);
        }

        internal Tuple<ScriptFn, List<ScriptVariable>> FindCallableInfo(List<ScriptValue> args, Dictionary<string, ScriptValue> keyArgs)
        {
            return FindCallableFnParams(args, keyArgs);
        }

        internal Tuple<ScriptFn, List<ScriptVariable>> FindCallableInfo(params ScriptValue[] values)
        {
            return FindCallableFnParams(values.ToList(), null);
        }

        internal ScriptValue Call(ScriptObject _this, Tuple<ScriptFn, List<ScriptVariable>> callableInfo)
        {
            return Call(callableInfo.Item1, _this, callableInfo.Item2);
        }

        public ScriptValue Call(ScriptObject _this, params ScriptValue[] value)
        {
            var fnInfo = FindCallableInfo(value);
            return Call(fnInfo.Item1, _this, fnInfo.Item2);
        }

        private ScriptValue Call(ScriptFn fn, ScriptObject _this, List<ScriptVariable> args)
        {
            if (fn.IsBuildIn)
            {
                List<ScriptValue> valueArgs = new List<ScriptValue>();

                if (!fn.IsStatic && _this != null)
                {
                    valueArgs.Add(new ScriptValue(_this));
                }

                foreach (var arg in args)
                {
                    valueArgs.Add(arg.Value);
                }
                return fn.BuildInFn(valueArgs);
            }

            // if it is a object call, set the scope to object
            // else if it is a static class call or it is a normal scope call, set the scope to the class scope / normal scope
            ScriptScope parentScope = _this == null ? fn.Scope : _this.Scope;
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
}
