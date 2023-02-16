using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HanabiLang.Parses;
using HanabiLang.Lexers;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Parses.Nodes;

namespace HanabiLang.Interprets
{
    class Interpreter
    {
        public ScriptScope currentScope { get; set; }
        private AbstractSyntaxTree ast { get; set; }
        public string Path { get; private set; }
        public static IEnumerable<string> Arguments { get; set; }

        /// <summary>
        /// Script Start or Import Script
        /// </summary>
        /// <param name="ast"></param>
        public Interpreter(AbstractSyntaxTree ast, string path, bool isMain)
        {
            this.ast = ast;
            this.currentScope = new ScriptScope(ScopeType.Normal);
            this.Path = path;
            this.currentScope.Classes["Script"] = new ScriptScript(isMain, Arguments);
            this.currentScope.Classes.Add("str", BasicTypes.Str);
            this.currentScope.Classes.Add("int", BasicTypes.Int);
            this.currentScope.Classes.Add("float", BasicTypes.Float);
            this.currentScope.Classes.Add("decimal", BasicTypes.Decimal);
            this.currentScope.Classes.Add("bool", BasicTypes.Bool);
            this.currentScope.Classes.Add("range", BasicTypes.Range);
            this.currentScope.Classes.Add("List", BasicTypes.List);
            this.currentScope.Classes.Add("Dict", BasicTypes.Dict);
            this.currentScope.Classes.Add("Enumerator", BasicTypes.Enumerator);
            BuildInFns.AddBasicFunctions(this.currentScope);
        }

        public void Interpret()
        {
            foreach (var child in this.ast.Nodes)
            {
                InterpretChild(this.currentScope, child);
            }
        }

        public static void ImportFile(ScriptScope interpretScope, AstNode node)
        {
            var realNode = (ImportNode)node;
            try
            {
                Type type = Type.GetType(realNode.Path);
                ScriptScope scriptScope =  BuildInClasses.FromStaticClass(type);
                if (realNode.Imports == null)
                {
                    if (string.IsNullOrEmpty(realNode.AsName))
                        interpretScope.Classes[type.Name] = new
                            ScriptClass(type.Name, null,
                            scriptScope, true);
                    else
                        interpretScope.Classes[realNode.AsName] = new
                            ScriptClass(realNode.AsName, null,
                             scriptScope, true);
                }
                else
                {
                    foreach (string item in realNode.Imports)
                    {
                        if (scriptScope.TryGetValue(item, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptFns)
                                interpretScope.Functions[((ScriptFns)scriptType).Name] = (ScriptFns)scriptType;
                            else if (scriptType is ScriptClass)
                                interpretScope.Classes[((ScriptClass)scriptType).Name] = (ScriptClass)scriptType;
                            else if (scriptType is ScriptVariable)
                                interpretScope.Variables[((ScriptVariable)scriptType).Name] = (ScriptVariable)scriptType;
                            else
                                throw new SystemException($"Unexpected script type");
                        }
                        else
                            throw new SystemException($"{item} is not defined in realNode.Path");
                    }
                }
                return;
            }
            catch { }

            string fullPath = System.IO.Path.GetFullPath(realNode.Path);
            if (!System.IO.File.Exists(fullPath))
                throw new SystemException($"File {realNode.Path} not found");
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullPath);
            string extension = System.IO.Path.GetExtension(fullPath).ToLower();
            if (extension.Equals(".json"))
            {
                List<string> lines = new List<string>();
                lines.Add("const jsonData = ");
                lines.AddRange(System.IO.File.ReadAllLines(fullPath));
                var tokens = Lexer.Tokenize(lines);
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                //Interpreter interpreter = new Interpreter(ast, fullPath, false, this.Arguments);
                Interpreter interpreter = new Interpreter(ast, fullPath, false);
                interpreter.Interpret();
                var jsonData = interpreter.currentScope.Variables["jsonData"].Value;
                if (string.IsNullOrEmpty(realNode.AsName))
                    interpretScope.Variables[fileNameWithoutExtension] = new ScriptVariable(fileNameWithoutExtension, jsonData, true);
                else
                    interpretScope.Variables[realNode.AsName] = new ScriptVariable(realNode.AsName, jsonData, true);
            }
            else
            {
                string[] lines = System.IO.File.ReadAllLines(fullPath);
                var tokens = Lexer.Tokenize(lines);
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                //Interpreter interpreter = new Interpreter(ast, fullPath, false, this.Arguments);
                Interpreter interpreter = new Interpreter(ast, fullPath, false);
                interpreter.Interpret();

                if (realNode.Imports == null)
                {
                    if (string.IsNullOrEmpty(realNode.AsName))
                        interpretScope.Classes[fileNameWithoutExtension] = new
                            ScriptClass(fileNameWithoutExtension, interpreter.ast.Nodes,
                            interpreter.currentScope, true, true);
                    else
                        interpretScope.Classes[realNode.AsName] = new
                            ScriptClass(realNode.AsName, interpreter.ast.Nodes,
                            interpreter.currentScope, true, true);
                }
                else
                {
                    foreach (string item in realNode.Imports)
                    {
                        if (interpretScope.TryGetValue(item, out _))
                            throw new SystemException($"Import failed, value {item} exists");

                        if (interpreter.currentScope.TryGetValue(item, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptFns)
                                interpretScope.Functions[((ScriptFns)scriptType).Name] = (ScriptFns)scriptType;
                            else if (scriptType is ScriptClass)
                                interpretScope.Classes[((ScriptClass)scriptType).Name] = (ScriptClass)scriptType;
                            else if (scriptType is ScriptVariable)
                                interpretScope.Variables[((ScriptVariable)scriptType).Name] = (ScriptVariable)scriptType;
                            else
                                throw new SystemException($"Unexpected script type");
                        }
                        else
                            throw new SystemException($"{item} is not defined in realNode.Path");
                    }
                }
            }
        }

        public static void InterpretChild(ScriptScope interpretScope, AstNode node)
        {
            // Interpret the child node
            if (node is ExpressionNode || node is IntNode ||
                node is FloatNode || node is InterpolatedString ||
                node is UnaryNode || node is StringNode ||
                node is VariableReferenceNode || node is FnCallNode || node is FnReferenceCallNode ||
                node is ForNode || node is WhileNode ||
                node is SwitchNode || node is SwitchCaseNode || node is IfNode)
            {
                InterpretExpression(interpretScope, node);
            }
            else if (node is ImportNode)
            {
                ImportFile(interpretScope, node);
            }
            else if (node is ThrowNode)
            {
               /* var realNode = (ThrowNode)node;
                var result = this.InterpretExpression(realNode.Value);
                if (!result.Ref.IsObject)
                    throw new SystemException($"{result.Ref} is not exception type");
                ScriptClass objClass = ((ScriptObject)result.Ref.Value).ObjectClass;
                if (!objClass.IsBuildIn)
                    throw new SystemException($"{result.Ref} is not exception type");
                throw (ScriptValue)result.Ref.Value;*/
            }
            else if (node is VariableDefinitionNode)
            {
                var realNode = (VariableDefinitionNode)node;

                if (interpretScope.Variables.ContainsKey(realNode.Name))
                    throw new SystemException($"Cannot reassigned variable {realNode.Name}");

                interpretScope.Variables[realNode.Name] = new ScriptVariable(realNode.Name,
                                                            InterpretExpression(interpretScope, realNode.Value).Ref,
                                                            realNode.IsConstant);
            }
            else if (node is VariableAssignmentNode)
            {
                var realNode = (VariableAssignmentNode)node;

                ValueReference left = InterpretExpression(interpretScope, realNode.Name);
                var assignValue = InterpretExpression(interpretScope, realNode.Value);
                left.Ref = assignValue.Ref;
                return;
                throw new SystemException($"Variable: {realNode.Name} is not defined");
            }
            else if (node is FnDefineNode)
            {
                var realNode = (FnDefineNode)node;

                if (!interpretScope.Functions.TryGetValue(realNode.Name, out ScriptFns scriptFns))
                {
                    scriptFns = new ScriptFns(realNode.Name);
                    interpretScope.Functions[realNode.Name] = scriptFns;
                }

                List<FnParameter> fnParameters = new List<FnParameter>();
                foreach (var param in realNode.Parameters)
                {
                    ScriptClass dataType = null;
                    ScriptValue defaultValue = null;
                    if (param.DataType != null)
                    {
                        ScriptValue type = InterpretExpression(interpretScope, param.DataType).Ref;
                        if (!type.IsClass)
                            throw new SystemException($"Only class is accepted in {realNode.Name}.{param.Name}");
                        dataType = (ScriptClass)type.Value;
                    }
                    if (param.DefaultValue != null)
                        defaultValue = InterpretExpression(interpretScope, param.DefaultValue).Ref;
                    fnParameters.Add(new FnParameter(param.Name, dataType, defaultValue));
                }

                scriptFns.Fns.Add(new ScriptFn(fnParameters,
                    realNode.Body, interpretScope, null));
            }
            else if (node is ClassDefineNode)
            {
                var realNode = (ClassDefineNode)node;

                foreach (var item in realNode.Body)
                {
                    if (!(item is FnDefineNode || item is VariableDefinitionNode || item is ClassDefineNode))
                    {
                        throw new SystemException("Class can only contains definition but not implement");
                    }
                }

                var scope = new ScriptScope(ScopeType.Class, interpretScope);
                interpretScope.Classes[realNode.Name] = new ScriptClass(realNode.Name, realNode.Body,
                    scope, false);
            }
        }

        public static ValueReference InterpretExpression(ScriptScope interpretScope, AstNode node)
        {
            if (node is ExpressionNode)
            {
                var realNode = (ExpressionNode)node;

                var _operater = realNode.Operator;
                var left = InterpretExpression(interpretScope, realNode.Left).Ref;
                if (_operater == ".")
                {
                    /*var originalScope = this.currentScope;
                    this.currentScope = null;
                    if (left.Obj is ScriptClass)
                        this.currentScope = ((ScriptClass)left.Obj).Scope;
                    else if (left.Obj is ScriptFn)
                        this.currentScope = ((ScriptFn)left.Obj).Scope;
                    else
                        this.currentScope = ((ScriptObject)left.Obj).Scope;
                    var rightValue = this.InterpretExpression(realNode.Right);
                    this.currentScope = originalScope;
                    return rightValue;*/
                    ScriptScope leftScope = null;
                    if (left.Value is ScriptClass)
                        leftScope = ((ScriptClass)left.Value).Scope;
                    else if (left.Value is ScriptFns)
                        throw new SystemException("Function cannot use operator '.'");
                    else
                        leftScope = ((ScriptObject)left.Value).Scope;
                    /*if (realNode.Right is FnCallNode)
                    {

                        var fnCall = (FnCallNode)realNode.Right;
                        if (leftScope.TryGetValue(fnCall.Name, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptFn)
                                return new ValueReference(((ScriptFn)scriptType).Call(this.currentScope, fnCall.Args));
                            else if (scriptType is ScriptClass)
                                return new ValueReference(((ScriptClass)scriptType).Call(this.currentScope, fnCall.Args));
                            else if (scriptType is ScriptVariable &&
                                ((ScriptVariable)(scriptType)).Value.IsFunction)
                                return new ValueReference(((ScriptFn)((ScriptVariable)(scriptType)).Value.Value).Call(this.currentScope, fnCall.Args));
                            else
                                throw new SystemException($"{fnCall.Name} is not callable");
                        }
                    }*/
                    if (realNode.Right is FnReferenceCallNode)
                    {

                        var fnCall = (FnReferenceCallNode)realNode.Right;
                        if (!(fnCall.Reference is VariableReferenceNode))
                            throw new SystemException($"Unexpected function call");

                        if (leftScope.TryGetValue(((VariableReferenceNode)fnCall.Reference).Name, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptFns)
                            {
                                return new ValueReference(((ScriptFns)scriptType).
                                    Call(interpretScope, leftScope.Type == ScopeType.Object ? (ScriptObject)left.Value : null,
                                    fnCall.Args));
                            }
                            else if (scriptType is ScriptClass)
                                return new ValueReference(((ScriptClass)scriptType).Call(interpretScope, fnCall.Args));
                            else if (scriptType is ScriptVariable &&
                                ((ScriptVariable)(scriptType)).Value.IsFunction)
                                return new ValueReference(((ScriptFns)((ScriptVariable)(scriptType)).Value.Value).
                                    Call(interpretScope, leftScope.Type == ScopeType.Object ? (ScriptObject)left.Value : null,
                                    fnCall.Args));
                            else
                                throw new SystemException($"{((VariableReferenceNode)fnCall.Reference).Name} is not callable");
                        }
                    }
                    else if (realNode.Right is VariableReferenceNode)
                    {
                        var varRef = (VariableReferenceNode)realNode.Right;
                        if (leftScope.TryGetValue(varRef.Name, out ScriptType scriptType))
                            if (scriptType is ScriptFns)
                                return new ValueReference(new ScriptValue((ScriptFns)scriptType));
                            else if (scriptType is ScriptClass)
                                return new ValueReference(new ScriptValue((ScriptClass)scriptType));
                            else if (scriptType is ScriptVariable)
                            {
                                var variable = (ScriptVariable)scriptType;
                                return new ValueReference(() => variable.Value, x => variable.Value = x);
                            }
                            else
                                throw new SystemException($"Unexcepted reference to variable");
                    }
                    throw new SystemException($"Unexcepted operation '.'");
                }
                var right = InterpretExpression(interpretScope, realNode.Right).Ref;
                if (_operater == "+") return new ValueReference(left + right);
                else if (_operater == "-") return new ValueReference(left - right);
                else if (_operater == "*") return new ValueReference(left * right);
                else if (_operater == "/") return new ValueReference(left % right);
                else if (_operater == "%") return new ValueReference(left % right);
                else if (_operater == "==") return new ValueReference(new ScriptValue(left.Equals(right)));
                else if (_operater == "!=") return new ValueReference(new ScriptValue(!left.Equals(right)));
                else if (_operater == "<") return new ValueReference(left < right);
                else if (_operater == ">") return new ValueReference(left > right);
                else if (_operater == "<=") return new ValueReference(left <= right);
                else if (_operater == ">=") return new ValueReference(left >= right);
                else if (_operater == "&&") return new ValueReference(ScriptValue.And(left, right));
                else if (_operater == "||") return new ValueReference(ScriptValue.Or(left, right));
                else throw new SystemException("Unknown operator " + _operater);
            }
            else if (node is IntNode)
            {
                var realNode = (IntNode)node;
                return new ValueReference(new ScriptValue(realNode.Value));
            }
            else if (node is FloatNode)
            {
                var realNode = (FloatNode)node;
                return new ValueReference(new ScriptValue(realNode.Value));
            }
            else if (node is StringNode)
            {
                var realNode = (StringNode)node;
                return new ValueReference(new ScriptValue(realNode.Value));
            }
            else if (node is InterpolatedString)
            {
                var realNode = (InterpolatedString)node;
                StringBuilder text = new StringBuilder();
                var interpolatedNodes = realNode.InterpolatedNodes;
                foreach (string str in realNode.Values)
                {
                    if (str == null)
                    {
                        text.Append(InterpretExpression(interpretScope, interpolatedNodes.Dequeue()).Ref.ToString());
                    }
                    else
                    {
                        text.Append(str);
                    }
                }
                return new ValueReference(new ScriptValue(text.ToString()));
            }
            else if (node is UnaryNode)
            {
                var realNode = (UnaryNode)node;
                ScriptValue value = InterpretExpression(interpretScope, realNode.Node).Ref;

                if (realNode.Operator == "+")
                    return new ValueReference(+value);
                else if (realNode.Operator == "!")
                    return new ValueReference(!value);
                return new ValueReference(-value);
            }
            else if (node is VariableReferenceNode)
            {
                var realNode = (VariableReferenceNode)node;

                // Checking if the variable is defined in any scope above the current one
                for (var scope = interpretScope; scope != null; scope = scope.Parent)
                {
                    if (scope.TryGetValue(realNode.Name, out ScriptType scriptType))
                    {
                        if (scriptType is ScriptVariable)
                        {
                            var variable = (ScriptVariable)scriptType;
                            return new ValueReference(() => variable.Value, x => variable.Value = x);
                        }
                        else if (scriptType is ScriptFns)
                        {
                            var fn = (ScriptFns)scriptType;
                            return new ValueReference(new ScriptValue(fn));
                        }
                        else if (scriptType is ScriptClass)
                        {
                            var _class = (ScriptClass)scriptType;
                            return new ValueReference(new ScriptValue(_class));
                        }
                    }
                }

                throw new SystemException($"{realNode.Name} is not defined");
            }
            /*
            else if (node is FnCallNode)
            {
                var realNode = (FnCallNode)node;
                for (var scope = this.currentScope; scope != null; scope = scope.Parent)
                {
                    if (scope.TryGetValue(realNode.Name, out ScriptType scriptType))
                    {
                        if (scriptType is ScriptVariable &&
                            ((ScriptVariable)scriptType).Value.IsFunction)
                        {
                            ScriptFn scriptFn = (ScriptFn)((ScriptVariable)scriptType).Value.Value;
                            return new ValueReference(scriptFn.Call(this.currentScope, realNode.Args));
                        }
                        else if (scriptType is ScriptFn)
                        {
                            ScriptFn scriptFn = (ScriptFn)scriptType;
                            return new ValueReference(scriptFn.Call(this.currentScope, realNode.Args));
                        }
                        else if (scriptType is ScriptClass) 
                        {
                            ScriptClass scriptClass = (ScriptClass)scriptType;
                            return new ValueReference(scriptClass.Call(this.currentScope, realNode.Args));
                        }
                        else
                        {
                            throw new SystemException($"{realNode.Name} is not a class or a function");
                        }
                    }
                }

                throw new SystemException($"{realNode.Name} is not defined");
            }*/
            else if (node is FnReferenceCallNode)
            {
                var realNode = (FnReferenceCallNode)node;
                var fnRef = InterpretExpression(interpretScope, realNode.Reference);
                if (fnRef.Ref.IsFunction)
                {
                    var fn = (ScriptFns)fnRef.Ref.Value;
                    return new ValueReference(fn.Call(interpretScope, null, realNode.Args));
                }
                else if (fnRef.Ref.IsClass)
                {
                    var _class = (ScriptClass)fnRef.Ref.Value;
                    return new ValueReference(_class.Call(interpretScope, realNode.Args));
                }

                throw new SystemException($"{realNode.Reference.NodeName} is not a class or a function");
            }
            else if (node is IfNode)
            {
                var realNode = (IfNode)node;

                var scope = new ScriptScope(ScopeType.Conditon, interpretScope);

                var compareResult = InterpretExpression(scope, realNode.Condition).Ref;

                if (!(compareResult.Value is ScriptObject))
                    throw new SystemException($"Cannot compare {compareResult}");

                if (!(((ScriptObject)compareResult.Value).ClassType is ScriptBool))
                    throw new SystemException($"Cannot compare {compareResult}");

                List<AstNode> currentBranch = null;
                if ((bool)((ScriptObject)compareResult.Value).BuildInObject)
                    currentBranch = realNode.ThenBranch;
                else
                    currentBranch = realNode.ElseBranch;

                foreach (var item in currentBranch)
                {
                    if (item is ReturnNode)
                    {
                        var returnNode = (ReturnNode)item;

                        if (returnNode.Value != null)
                        {
                            var value = InterpretExpression(scope, returnNode.Value);

                            return value;
                        }
                        return new ValueReference(ScriptValue.Null);
                    }
                    else
                    {
                        InterpretChild(scope, item);
                    }
                }

                return ValueReference.Empty;
            }
            else if (node is SwitchNode)
            {
                var realNode = (SwitchNode)node;
                var switchValue = InterpretExpression(interpretScope, realNode.Condition);
                bool hasMatchCase = false;

                foreach (var caseNode in realNode.Cases)
                {
                    foreach (var condition in caseNode.Conditions)
                    {
                        var caseExpression = InterpretExpression(interpretScope, condition);
                        if (caseExpression.Ref.Equals(switchValue.Ref))
                        {
                            hasMatchCase = true;
                            foreach (var item in caseNode.Body)
                            {
                                var scope = new ScriptScope(ScopeType.Conditon, interpretScope);
                                if (item is ReturnNode)
                                {
                                    var returnNode = (ReturnNode)item;

                                    if (returnNode.Value != null)
                                    {
                                        var value = InterpretExpression(scope, returnNode.Value);

                                        return value;
                                    }

                                    return new ValueReference(ScriptValue.Null);
                                }
                                else if (item is BreakNode)
                                {
                                    return ValueReference.Empty;
                                }
                                else
                                {
                                    InterpretChild(scope, item);
                                }
                            }
                        }
                    }
                }

                if (realNode.DefaultCase == null || hasMatchCase)
                    return ValueReference.Empty;
                var defaultScope = new ScriptScope(ScopeType.Conditon, interpretScope);
                foreach (var item in realNode.DefaultCase.Body)
                {
                    if (item is ReturnNode)
                    {
                        var returnNode = (ReturnNode)item;

                        if (returnNode.Value != null)
                        {
                            var value = InterpretExpression(defaultScope, returnNode.Value);

                            return value;
                        }

                        return new ValueReference(ScriptValue.Null);
                    }
                    else
                    {
                        InterpretChild(defaultScope, item);
                    }
                }
                return ValueReference.Empty;
            }
            else if (node is WhileNode)
            {
                var realNode = (WhileNode)node;
                var condition = realNode.Condition;

                while ((bool)((ScriptObject)(InterpretExpression(interpretScope, condition).Ref.Value)).BuildInObject)
                {
                    var scope = new ScriptScope(ScopeType.Loop, interpretScope);
                    var hasBreak = false;

                    foreach (var bodyNode in realNode.Body)
                    {
                        var hasContinue = false;
                        if (bodyNode is ReturnNode)
                        {
                            var returnNode = (ReturnNode)bodyNode;

                            if (returnNode.Value != null)
                            {
                                var value = InterpretExpression(scope, returnNode.Value);

                                return value;
                            }
                            return new ValueReference(ScriptValue.Null);
                        }

                        if (bodyNode is BreakNode)
                            hasBreak = true;

                        if (bodyNode is ContinueNode)
                            hasContinue = true;

                        if (!hasBreak && !hasContinue)
                            InterpretChild(scope, bodyNode);

                        /*if (((ScriptBool)(this.InterpretExpression(condition).Ref.Value)).Value)
                            break;*/
                    }

                    if (hasBreak)
                        break;
                }

                return ValueReference.Empty;
            }
            else if (node is ForNode)
            {
                var realNode = (ForNode)node;
                var location = InterpretExpression(interpretScope, realNode.Location).Ref;


                if (!location.IsObject)
                    throw new SystemException("For loop running failed, variable is not enumerable");

                ScriptObject scriptObject = (ScriptObject)location.Value;

                if (!scriptObject.Scope.TryGetValue("GetEnumerator", out ScriptType getEnumerator))
                    throw new SystemException("For loop running failed, variable is not enumerable");

                if (!(getEnumerator is ScriptFns))
                    throw new SystemException("For loop running failed, variable is not enumerable");
                var enumerator = ((ScriptFns)getEnumerator).Call(interpretScope, scriptObject, new Dictionary<string, AstNode>());

                if (!(((ScriptObject)enumerator.Value).BuildInObject is IEnumerable<ScriptValue>))
                    throw new SystemException("For loop running failed, variable is not enumerable");

                var list = (IEnumerable<ScriptValue>)(((ScriptObject)enumerator.Value).BuildInObject);

                /*this.currentScope = new ScriptScope(ScopeType.Loop, this.currentScope);

                this.currentScope.Variables[realNode.Initializer] =
                    new ScriptVariable(realNode.Initializer, new ScriptValue(0), false);*/

                var hasBreak = false;

                foreach (var item in list)
                {
                    var scope = new ScriptScope(ScopeType.Loop, interpretScope);

                    scope.Variables[realNode.Initializer] =
                        new ScriptVariable(realNode.Initializer, item, false);

                    var hasContinue = false;

                    foreach (var bodyNode in realNode.Body)
                    {
                        if (bodyNode is ReturnNode)
                        {
                            var returnNode = (ReturnNode)bodyNode;

                            if (returnNode.Value != null)
                            {
                                var value = InterpretExpression(scope, returnNode.Value);

                                return value;
                            }

                            return new ValueReference(ScriptValue.Null);
                        }

                        if (bodyNode is BreakNode)
                            hasBreak = true;

                        if (bodyNode is ContinueNode)
                            hasContinue = true;

                        if (!hasBreak && !hasContinue)
                            InterpretChild(scope, bodyNode);
                    }

                    if (hasBreak)
                        break;
                }

                //this.currentScope = this.currentScope.Parent;
                return ValueReference.Empty;
            }
            else if (node is ListNode)
            {
                var realNode = (ListNode)node;

                List<ScriptValue> values = new List<ScriptValue>();

                foreach (var value in realNode.Elements)
                    values.Add(InterpretExpression(interpretScope, value).Ref);

                return new ValueReference(new ScriptValue(values));
            }
            else if (node is DictNode)
            {
                var realNode = (DictNode)node;

                var keyValues = new Dictionary<ScriptValue, ScriptValue>();

                foreach (var keyValue in realNode.KeyValues)
                {
                    var key = InterpretExpression(interpretScope, keyValue.Item1).Ref;
                    var value = InterpretExpression(interpretScope, keyValue.Item2).Ref;
                    keyValues[key] = value;
                }

                return new ValueReference(new ScriptValue(keyValues));
            }
            else if (node is IndexersNode)
            {
                var realNode = (IndexersNode)node;

                var list = InterpretExpression(interpretScope, realNode.Object);

                if (!(list.Ref.Value is ScriptObject))
                    throw new SystemException("Indexer can only apply in object");

                ScriptObject obj = (ScriptObject)list.Ref.Value;

                if (obj.ClassType is ScriptList)
                {
                    List<ScriptValue> listValue = (List<ScriptValue>)obj.BuildInObject;

                    var index = InterpretExpression(interpretScope, realNode.Index);

                    if (!(((ScriptObject)index.Ref.Value).ClassType is ScriptInt))
                        throw new SystemException("Index in List must be integer");

                    long intValue = (long)((ScriptObject)index.Ref.Value).BuildInObject;

                    if (intValue < 0 || intValue >= listValue.Count)
                        throw new SystemException("Index out of range");
                    return new ValueReference(() => listValue[(int)intValue],
                        x => listValue[(int)intValue] = x);
                }
                else if (obj.ClassType is ScriptDict)
                {
                    Dictionary<ScriptValue, ScriptValue> dictValue = (Dictionary<ScriptValue, ScriptValue>)obj.BuildInObject;
                    var index = InterpretExpression(interpretScope, realNode.Index);

                    ScriptValue accessValue = index.Ref;

                    return new ValueReference(() => dictValue[accessValue], x => dictValue[accessValue] = x);
                }
                else
                {
                    throw new SystemException("The variable is not enumerable");
                }
            }
            else if (node is NullNode)
            {
                return new ValueReference(ScriptValue.Null);
            }
            else if (node is BooleanNode)
            {
                var realNode = (BooleanNode)node;
                return new ValueReference(new ScriptValue(realNode.Value));
            }
            else if (node is FnDefineNode)
            {
                var realNode = (FnDefineNode)node;
                List<FnParameter> fnParameters = new List<FnParameter>();
                foreach (var param in realNode.Parameters)
                {
                    ScriptClass dataType = null;
                    ScriptValue defaultValue = null;
                    if (param.DataType != null)
                    {
                        ScriptValue type = InterpretExpression(interpretScope, param.DataType).Ref;
                        if (!type.IsClass)
                            throw new SystemException($"Only class is accepted in {realNode.Name}.{param.Name}");
                        dataType = (ScriptClass)type.Value;
                    }
                    if (param.DefaultValue != null)
                        defaultValue = InterpretExpression(interpretScope, param.DefaultValue).Ref;
                    fnParameters.Add(new FnParameter(param.Name, dataType, defaultValue));
                }
                var scriptFns = new ScriptFns(realNode.Name);
                scriptFns.Fns.Add(new ScriptFn(fnParameters, realNode.Body,
                                interpretScope, null));
                return new ValueReference(new ScriptValue(scriptFns));
            }
            else if (node is TernaryNode)
            {
                var realNode = (TernaryNode)node;
                var condition = InterpretExpression(interpretScope, realNode.Condition);

                if (!(condition.Ref.Value is ScriptObject))
                    throw new SystemException("Ternary condition should be boolean");

                if (!(((ScriptObject)condition.Ref.Value).ClassType is ScriptBool))
                    throw new SystemException("Ternary condition should be boolean");

                if ((bool)((ScriptObject)condition.Ref.Value).BuildInObject)
                {
                    return InterpretExpression(interpretScope, realNode.Consequent);
                }
                return InterpretExpression(interpretScope, realNode.Alternative);
            }
            throw new SystemException("Unexcepted interpret expression: " + node.GetType().Name);
        }
    }
}
