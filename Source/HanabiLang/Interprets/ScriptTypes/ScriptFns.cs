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

    class FnParameter
    {
        public string Name { get; private set; }
        public ScriptClass DataType { get; private set; }
        public ScriptValue DefaultValue { get; private set; }
        public bool IsMultiArgs { get; private set; }

        public FnParameter(string name, ScriptClass dataType = null, ScriptValue defaultValue = null, bool multipleArguments = false)
        {
            this.Name = name;
            this.DataType = dataType;
            this.DefaultValue = defaultValue;
            this.IsMultiArgs = multipleArguments;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class ScriptFn
    {
        public List<FnParameter> Parameters { get; private set; }
        private Dictionary<string, int> ArgsMap { get; set; }
        public List<AstNode> Body { get; private set; }
        public ScriptScope Scope { get; private set; }
        public BuildInFns.ScriptFnType BuildInFn { get; private set; }
        public bool IsBuildIn => BuildInFn != null;
        public int MinArgs { get; private set; }
        public bool HasMultiArgs => Parameters.Count != 0 && Parameters[Parameters.Count - 1].IsMultiArgs;
        public bool IsStatic { get; private set; }
        public AccessibilityLevel Level { get; private set; }

        private ScriptFn(List<FnParameter> parameters, List<AstNode> body, ScriptScope scope, BuildInFns.ScriptFnType fn, bool isStatic, AccessibilityLevel level)
        {
            this.Parameters = parameters;
            this.ArgsMap = new Dictionary<string, int>();
            this.Body = body;
            this.Scope = scope;
            this.BuildInFn = fn;
            this.IsStatic = isStatic;
            this.Level = level;

            int count = 0;
            foreach (var param in this.Parameters)
            {
                ArgsMap[param.Name] = count;
                if (param.DefaultValue == null)
                {
                    MinArgs++;
                }
                count++;
            }
        }

        public ScriptFn(List<FnParameter> parameters, ScriptScope scope, BuildInFns.ScriptFnType fn, bool isStatic, AccessibilityLevel level) :
            this(parameters, null, scope, fn, isStatic, level)
        { }

        public ScriptFn(List<FnParameter> parameters, List<AstNode> body, ScriptScope scope, bool isStatic, AccessibilityLevel level) :
            this(parameters, body, scope, null, isStatic, level)
        { }

    }

    class ScriptFns : ScriptType
    {
        public string Name { get; private set; }

        public List<ScriptFn> Fns { get; private set; }
        public ScriptFns(string name)
        {
            this.Name = name;
            if (string.IsNullOrEmpty(this.Name))
                this.Name = "Lambda";
            this.Fns = new List<ScriptFn>();
        }

        /*public ScriptValue Call(ScriptScope currentScope, params ScriptValue[] values)
        {
            ScriptFn fn = this.Fns[0]; 
            if (fn.IsBuildIn)
            {
                return fn.BuildInFn(values.ToList());
            }

            var fnScope = new ScriptScope(ScopeType.Function, fn.Scope);

            var index = 0;
            foreach (var parameter in fn.Parameters)
            {
                var variable = new ScriptVariable(parameter.Name, values[index], false);
                if (parameter.DefaultValue != null)
                    variable.Value = parameter.DefaultValue;
                fnScope.Variables[parameter.Name] = variable;

                if (index >= values.Length && parameter.DefaultValue != null)
                {
                    fnScope.Variables[parameter.Name] = new ScriptVariable(parameter.Name,
                                                            parameter.DefaultValue, false);
                }
                else
                {
                    fnScope.Variables[parameter.Name] = new ScriptVariable(parameter.Name, values[index], false);
                }
                index++;
            }


            foreach (var bodyNode in fn.Body)
            {
                if (bodyNode is ReturnNode)
                {
                    var returnNode = (ReturnNode)bodyNode;

                    if (returnNode.Value != null)
                    {
                        var value = Interpreter.InterpretExpression(currentScope, returnNode.Value);

                        // Returning the value
                        return value.Ref;
                    }

                    // Returning the value
                    return new ScriptValue();
                }
                if (bodyNode is IfNode || bodyNode is SwitchCaseNode || 
                    bodyNode is ForNode || bodyNode is WhileNode)
                {
                    var value = Interpreter.InterpretExpression(currentScope, bodyNode);
                    if (!value.IsEmpty)
                    {
                        return value.Ref;
                    }
                }
                else
                {
                    Interpreter.InterpretChild(currentScope, bodyNode);
                }
            }
            return new ScriptValue();
        }*/

        private static Dictionary<string, ScriptValue> GetArgs(ScriptScope currentScope, Dictionary<string, AstNode> callArgs)
        {
            Dictionary<string, ScriptValue> result = new Dictionary<string, ScriptValue>();
            foreach (var arg in callArgs)
            {
                result[arg.Key] = Interpreter.InterpretExpression(currentScope, arg.Value).Ref;
            }
            return result;
        }

        private static Tuple<ScriptFn, List<ScriptVariable>, int> MinScriptFn(List<Tuple<ScriptFn, List<ScriptVariable>, int>> list)
        {
            int min = list[0].Item3;
            int minIndex = 0;

            for (int i = 1; i < list.Count; ++i)
            {
                if (list[i].Item3 < min)
                {
                    min = list[i].Item3;
                    minIndex = i;
                }
            }

            return list[minIndex];
        }

        private Tuple<ScriptFn, List<ScriptVariable>> FindFnInfo(Dictionary<string, ScriptValue> args)
        {
            var fns = new List<Tuple<ScriptFn, List<ScriptVariable>, int>>();

            foreach (var fn in this.Fns)
            {
                if ((args.Count < fn.MinArgs || args.Count > fn.Parameters.Count) && !fn.HasMultiArgs)
                    continue;

                int index = 0;
                int anyTypeCount = 0;
                var variables = new List<ScriptVariable>();
                foreach (var parameter in fn.Parameters)
                {
                    if (parameter.IsMultiArgs)
                    {
                        var multipleArguments = new List<ScriptValue>();

                        while (args.TryGetValue(index.ToString(), out ScriptValue value))
                        {
                            multipleArguments.Add(value);
                            index++;
                        }
                        variables.Add(new ScriptVariable(parameter.Name, new ScriptValue(multipleArguments), false, false, AccessibilityLevel.Private));
                    }
                    else if (index >= args.Count)
                    {
                        if (parameter.DefaultValue == null)
                            break;
                        variables.Add(new ScriptVariable(parameter.Name, parameter.DefaultValue, false, false, AccessibilityLevel.Private));
                    }
                    else
                    {
                        if (!args.TryGetValue(parameter.Name, out ScriptValue value))
                            value = args[index.ToString()];

                        if (parameter.DataType != null && 
                            (value.IsNull || ((ScriptObject)value.Value).ClassType != parameter.DataType))
                            break;
                        if (parameter.DataType == null)
                            anyTypeCount ++;
                        variables.Add(new ScriptVariable(parameter.Name, value, false, false, AccessibilityLevel.Private));
                    }
                    index++;
                }

                if (index == fn.Parameters.Count || fn.HasMultiArgs)
                    fns.Add(Tuple.Create(fn, variables, anyTypeCount));
            }

            if (fns.Count <= 0)
                throw new SystemException($"Match function call for {this.Name} does not exists");

            var scriptfn = MinScriptFn(fns);
            return Tuple.Create(scriptfn.Item1, scriptfn.Item2);
        }

        public Tuple<ScriptFn, List<ScriptVariable>> GetFnInfo(ScriptScope scope, Dictionary<string, AstNode> nodeArgs)
        {
            return FindFnInfo(GetArgs(scope, nodeArgs));
        }

        public Tuple<ScriptFn, List<ScriptVariable>> GetFnInfo(Dictionary<string, ScriptValue> args)
        {
            return FindFnInfo(args);
        }

        public Tuple<ScriptFn, List<ScriptVariable>> GetFnInfo(params ScriptValue[] values)
        {
            Dictionary<string, ScriptValue> args = new Dictionary<string, ScriptValue>();
            for (int i = 0; i < values.Length; i++)
            {
                args[i.ToString()] = values[i];
            }
            return FindFnInfo(args); ;
        }

        public ScriptValue Call(ScriptObject _this, Tuple<ScriptFn, List<ScriptVariable>> fnInfo)
        {
            return Call(fnInfo.Item1, _this, fnInfo.Item2);
        }

        public ScriptValue Call(ScriptObject _this, params ScriptValue[] value)
        {
            var fnInfo = GetFnInfo(value);
            return Call(fnInfo.Item1, _this, fnInfo.Item2);
        }

        private ScriptValue Call(ScriptFn fn, ScriptObject _this, List<ScriptVariable> args)
        {
            if (fn.IsBuildIn)
            {
                List<ScriptValue> valueArgs = new List<ScriptValue>();

                foreach (var arg in args)
                {
                    valueArgs.Add(arg.Value);
                }
                if (_this != null)
                {
                    valueArgs.Insert(0, new ScriptValue(_this));
                }
                return fn.BuildInFn(valueArgs);
            }

            ScriptScope parentScope = _this == null ? fn.Scope : _this.Scope;
            /*ScriptScope parentScope = fn.Scope;
            if (_this != null)
            {
                parentScope = _this.Scope.Copy();
                parentScope.Parent = _this.Scope.ClassScope;
            }*/

            var fnScope = new ScriptScope(ScopeType.Function, parentScope);

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

                        // Returning the value
                        return value.Ref;
                    }

                    // Returning the value
                    return new ScriptValue();
                }
                if (node is IfNode || node is SwitchCaseNode || 
                    node is ForNode || node is WhileNode)
                {
                    var value = Interpreter.InterpretExpression(fnScope, node);
                    if (!value.IsEmpty) 
                    {
                        return value.Ref;
                    }
                }
                else
                {
                    Interpreter.InterpretChild(fnScope, node);
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
