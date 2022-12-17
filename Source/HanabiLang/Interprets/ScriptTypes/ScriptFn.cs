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

    class ScriptFn : ScriptType
    {
        public string Name { get; private set; }
        public List<FnParameter> Parameters { get; private set; }
        public List<AstNode> Body { get; private set; }
        public ScriptScope Scope { get; private set; }
        public BuildInFns.ScriptFnType BuildInFn { get; private set; }
        public bool IsBuildIn => BuildInFn != null;
        public int MinArgs { get; private set; }
        public ScriptFn(string name, List<FnParameter> parameters, List<AstNode> body, ScriptScope scope, BuildInFns.ScriptFnType fn = null)
        {
            this.Name = name;
            if (string.IsNullOrEmpty(this.Name))
                this.Name = "Lambda";
            this.Parameters = parameters;
            this.Body = body;
            this.Scope = scope;
            this.BuildInFn = fn;
            foreach (var param in this.Parameters)
            {
                if (param.DefaultValue == null)
                {
                    MinArgs++;
                }
            }
        }


        public ScriptValue Call(ScriptScope currentScope, params ScriptValue[] values)
        {
            var interpreter = new Interpreter(currentScope);
            if (this.IsBuildIn)
            {
                return this.BuildInFn(values.ToList());
            }


            var parentScope = interpreter.currentScope;

            var fnScope = new ScriptScope(ScopeType.Function, this.Scope);

            var index = 0;
            foreach (var parameter in this.Parameters)
            {
                var variable = new ScriptVariable(parameter.Name, values[index], false);
                if (parameter.DefaultValue != null)
                    variable.Value = interpreter.InterpretExpression(parameter.DefaultValue).Ref;
                fnScope.Variables[parameter.Name] = variable;

                if (index >= values.Length && parameter.DefaultValue != null)
                {
                    fnScope.Variables[parameter.Name] = new ScriptVariable(parameter.Name,
                                                            interpreter.InterpretExpression(
                                                                parameter.DefaultValue).Ref, false);
                }
                else
                {
                    fnScope.Variables[parameter.Name] = new ScriptVariable(parameter.Name, values[index], false);
                }
                index++;
            }

            interpreter.currentScope = fnScope;

            foreach (var bodyNode in this.Body)
            {

                if (bodyNode is ReturnNode)
                {
                    var returnNode = (ReturnNode)bodyNode;

                    if (returnNode.Value != null)
                    {
                        var value = interpreter.InterpretExpression(returnNode.Value);

                        // Returning the value
                        interpreter.currentScope = parentScope;
                        return value.Ref;
                    }

                    // Returning the value
                    interpreter.currentScope = parentScope;
                    return new ScriptValue();
                }
                if (bodyNode is IfNode || bodyNode is SwitchCaseNode || 
                    bodyNode is ForNode || bodyNode is WhileNode)
                {
                    var value = interpreter.InterpretExpression(bodyNode);
                    if (value != null)
                    {
                        interpreter.currentScope = parentScope;
                        return value.Ref;
                    }
                }
                else
                {
                    interpreter.InterpretChild(bodyNode);
                }
            }
            interpreter.currentScope = parentScope;
            return new ScriptValue();
        }
        public ScriptValue Call(ScriptScope currentScope, FnCallNode callNode)
        {
            var interpreter = new Interpreter(currentScope);
            if (this.IsBuildIn)
            {
                List<ScriptValue> args = new List<ScriptValue>();

                foreach (var arg in callNode.Args)
                {
                    args.Add(interpreter.InterpretExpression(arg).Ref);
                }

                return this.BuildInFn(args);
            }

            if (callNode.Args.Count < this.MinArgs)
                throw new SystemException("Wrong number of arguments");

            var parentScope = interpreter.currentScope;
            var fnScope = new ScriptScope(ScopeType.Function, this.Scope);

            var index = 0;
            foreach (var parameter in this.Parameters)
            {
                if (index >= callNode.Args.Count && parameter.DefaultValue != null)
                {
                    fnScope.Variables[parameter.Name] = new ScriptVariable(parameter.Name,
                                                            interpreter.InterpretExpression(
                                                                parameter.DefaultValue).Ref, false);
                }
                else
                {
                    fnScope.Variables[parameter.Name] = new ScriptVariable(parameter.Name,
                                  interpreter.InterpretExpression(
                                          callNode.Args[index]).Ref, false);
                }
                index++;
            }


            interpreter.currentScope = fnScope;

            foreach (var node in this.Body)
            {
                if (node is ReturnNode)
                {
                    var returnNode = (ReturnNode)node;

                    if (returnNode.Value != null)
                    {
                        var value = interpreter.InterpretExpression(returnNode.Value);

                        // Returning the value
                        interpreter.currentScope = parentScope;
                        return value.Ref;
                    }

                    // Returning the value
                    interpreter.currentScope = parentScope;
                    return new ScriptValue();
                }
                if (node is IfNode || node is SwitchCaseNode || 
                    node is ForNode || node is WhileNode)
                {
                    var value = interpreter.InterpretExpression(node);
                    if (value != null) 
                    {
                        interpreter.currentScope = parentScope;
                        return value.Ref;
                    }
                }
                else
                {
                    interpreter.InterpretChild(node);
                }
            }
            interpreter.currentScope = parentScope;
            return new ScriptValue();
        }
        public override string ToJsonString(int basicIndent = 2, int currentIndent = 0)
        {
            return $"\"Function\": \"{this.Name}\"";
        }
        public override string ToString()
        {
            return $"Function: {this.Name}";
        }
    }
}
