using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public IEnumerable<string> Arguments { get; private set; }

        /// <summary>
        /// Script Start or Import Script
        /// </summary>
        /// <param name="ast"></param>
        public Interpreter(AbstractSyntaxTree ast, string path, bool isMain, IEnumerable<string> arugments)
        {
            this.ast = ast;
            this.currentScope = new ScriptScope(ScopeType.Normal);
            this.Path = path;
            this.Arguments = arugments;
            this.currentScope.Classes["Script"] = ScriptScript.CreateBuildInClass(isMain, arugments);
            this.currentScope.Classes.Add("str", ScriptStr.CreateBuildInClass());
            this.currentScope.Classes.Add("int", ScriptInt.CreateBuildInClass());
            this.currentScope.Classes.Add("float", ScriptFloat.CreateBuildInClass());
            this.currentScope.Classes.Add("decimal", ScriptDecimal.CreateBuildInClass());
            this.currentScope.Classes.Add("bool", ScriptBool.CreateBuildInClass());
            this.currentScope.Classes.Add("range", ScriptRange.CreateBuildInClass());
            this.currentScope.Classes.Add("List", ScriptList.CreateBuildInClass());
            this.currentScope.Classes.Add("Dict", ScriptDict.CreateBuildInClass());
            foreach (var fn in BuildInFns.Fns)
            {
                currentScope.Functions[fn.Key] = new ScriptFn(fn.Key, BuildInFns.GetBuildInFnParams(fn.Value),
                    null, this.currentScope, fn.Value);
            }
        }

        /// <summary>
        /// For Function Call
        /// </summary>
        /// <param name="scope">Function Scope</param>
        public Interpreter(ScriptScope scope)
        {
            this.currentScope = scope;
            this.ast = new AbstractSyntaxTree();
        }

        public void Interpret()
        {
            foreach (var child in this.ast.Nodes)
            {
                this.InterpretChild(child);
            }
        }

        public void ImportFile(AstNode node)
        {
            var realNode = (ImportNode)node;
            try
            {
                Type type = Type.GetType(realNode.Path);
                ScriptScope scriptScope =  BuildInClasses.FromStaticClass(type);
                if (realNode.Imports == null)
                {
                    if (string.IsNullOrEmpty(realNode.AsName))
                        this.currentScope.Classes[type.Name] = new
                            ScriptClass(type.Name, null,
                            new List<string>(), scriptScope, true);
                    else
                        this.currentScope.Classes[realNode.AsName] = new
                            ScriptClass(realNode.AsName, null,
                            new List<string>(), scriptScope, true);
                }
                else
                {
                    foreach (string item in realNode.Imports)
                    {
                        if (scriptScope.TryGetValue(item, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptFn)
                                this.currentScope.Functions[((ScriptFn)scriptType).Name] = (ScriptFn)scriptType;
                            else if (scriptType is ScriptClass)
                                this.currentScope.Classes[((ScriptClass)scriptType).Name] = (ScriptClass)scriptType;
                            else if (scriptType is ScriptVariable)
                                this.currentScope.Variables[((ScriptVariable)scriptType).Name] = (ScriptVariable)scriptType;
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
                Interpreter interpreter = new Interpreter(ast, fullPath, false, this.Arguments);
                interpreter.Interpret();
                var jsonData = interpreter.currentScope.Variables["jsonData"].Value;
                if (string.IsNullOrEmpty(realNode.AsName))
                    this.currentScope.Variables[fileNameWithoutExtension] = new ScriptVariable(fileNameWithoutExtension, jsonData, true);
                else
                    this.currentScope.Variables[realNode.AsName] = new ScriptVariable(realNode.AsName, jsonData, true);
            }
            else
            {
                string[] lines = System.IO.File.ReadAllLines(fullPath);
                var tokens = Lexer.Tokenize(lines);
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                Interpreter interpreter = new Interpreter(ast, fullPath, false, this.Arguments);
                interpreter.Interpret();

                if (realNode.Imports == null)
                {
                    if (string.IsNullOrEmpty(realNode.AsName))
                        this.currentScope.Classes[fileNameWithoutExtension] = new
                            ScriptClass(fileNameWithoutExtension, interpreter.ast.Nodes,
                            new List<string>(), interpreter.currentScope, true);
                    else
                        this.currentScope.Classes[realNode.AsName] = new
                            ScriptClass(realNode.AsName, interpreter.ast.Nodes,
                            new List<string>(), interpreter.currentScope, true);
                }
                else
                {
                    foreach (string item in realNode.Imports)
                    {
                        if (interpreter.currentScope.TryGetValue(item, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptFn)
                                this.currentScope.Functions[((ScriptFn)scriptType).Name] = (ScriptFn)scriptType;
                            else if (scriptType is ScriptClass)
                                this.currentScope.Classes[((ScriptClass)scriptType).Name] = (ScriptClass)scriptType;
                            else if (scriptType is ScriptVariable)
                                this.currentScope.Variables[((ScriptVariable)scriptType).Name] = (ScriptVariable)scriptType;
                            else
                                throw new SystemException($"Unexpected script type");
                        }
                        else
                            throw new SystemException($"{item} is not defined in realNode.Path");
                    }
                }
            }
        }

        public void InterpretChild(AstNode node)
        {
            // Interpret the child node
            if (node is ExpressionNode || node is IntNode ||
                node is FloatNode ||
                node is UnaryNode || node is StringNode ||
                node is VariableReferenceNode || node is FnCallNode || node is FnReferenceCallNode ||
                node is ForNode || node is WhileNode ||
                node is SwitchNode || node is SwitchCaseNode || node is IfNode)
            {
                this.InterpretExpression(node);
            }
            else if (node is ImportNode)
            {
                ImportFile(node);
            }
            else if (node is VariableDefinitionNode)
            {
                var realNode = (VariableDefinitionNode)node;

                if (this.currentScope.Variables.ContainsKey(realNode.Name))
                    throw new SystemException($"Cannot reassigned variable {realNode.Name}");

                this.currentScope.Variables[realNode.Name] = new ScriptVariable(realNode.Name,
                                                            this.InterpretExpression(realNode.Value).Ref,
                                                            realNode.IsConstant);
            }
            else if (node is VariableAssignmentNode)
            {
                var realNode = (VariableAssignmentNode)node;

                ValueReference left = InterpretExpression(realNode.Name);
                var assignValue = InterpretExpression(realNode.Value);
                left.Ref = assignValue.Ref;
                return;
                throw new SystemException($"Variable: {realNode.Name} is not defined");
            }
            else if (node is FnDefineNode)
            {
                var realNode = (FnDefineNode)node;

                this.currentScope.Functions[realNode.Name] = 
                    new ScriptFn(realNode.Name, realNode.Parameters,
                    realNode.Body, this.currentScope, null);
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

                var scope = new ScriptScope(ScopeType.Class, this.currentScope);
                this.currentScope.Classes[realNode.Name] = new ScriptClass(realNode.Name, realNode.Body,
                    realNode.Constructor, scope, false);
            }
        }

        public ValueReference InterpretExpression(AstNode node)
        {
            if (node is ExpressionNode)
            {
                var realNode = (ExpressionNode)node;

                var _operater = realNode.Operator;
                var left = this.InterpretExpression(realNode.Left).Ref;
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
                    else if (left.Value is ScriptFn)
                        leftScope = ((ScriptFn)left.Value).Scope;
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
                            if (scriptType is ScriptFn)
                                return new ValueReference(((ScriptFn)scriptType).Call(this.currentScope, fnCall.Args));
                            else if (scriptType is ScriptClass)
                                return new ValueReference(((ScriptClass)scriptType).Call(this.currentScope, fnCall.Args));
                            else if (scriptType is ScriptVariable &&
                                ((ScriptVariable)(scriptType)).Value.IsFunction)
                                return new ValueReference(((ScriptFn)((ScriptVariable)(scriptType)).Value.Value).Call(this.currentScope, fnCall.Args));
                            else
                                throw new SystemException($"{((VariableReferenceNode)fnCall.Reference).Name} is not callable");
                        }
                    }
                    else if (realNode.Right is VariableReferenceNode)
                    {
                        var varRef = (VariableReferenceNode)realNode.Right;
                        if (leftScope.TryGetValue(varRef.Name, out ScriptType scriptType))
                            if (scriptType is ScriptFn)
                                return new ValueReference(new ScriptValue((ScriptFn)scriptType));
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
                var right = this.InterpretExpression(realNode.Right).Ref;
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
                foreach (string str in realNode.Values)
                {
                    if (str == null)
                    {
                        text.Append(InterpretExpression(realNode.InterpolatedNodes.Dequeue()).Ref.ToString());
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
                ScriptValue value = this.InterpretExpression(realNode.Node).Ref;

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
                for (var scope = this.currentScope; scope != null; scope = scope.Parent)
                {
                    if (scope.TryGetValue(realNode.Name, out ScriptType scriptType))
                    {
                        if (scriptType is ScriptVariable)
                        {
                            var variable = (ScriptVariable)scriptType;
                            return new ValueReference(() => variable.Value, x => variable.Value = x);
                        }
                        else if (scriptType is ScriptFn)
                        {
                            var fn = (ScriptFn)scriptType;
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
                var fnRef = InterpretExpression(realNode.Reference);
                if (fnRef.Ref.IsFunction)
                {
                    var fn = (ScriptFn)fnRef.Ref.Value;
                    return new ValueReference(fn.Call(this.currentScope, realNode.Args));
                }
                else if (fnRef.Ref.IsClass)
                {
                    var _class = (ScriptClass)fnRef.Ref.Value;
                    return new ValueReference(_class.Call(this.currentScope, realNode.Args));
                }

                throw new SystemException($"{realNode.Reference.NodeName} is not a class or a function");
            }
            else if (node is IfNode)
            {
                var realNode = (IfNode)node;

                this.currentScope = new ScriptScope(ScopeType.Conditon, this.currentScope);

                var compareResult = this.InterpretExpression(realNode.Condition).Ref;

                if (!(compareResult.Value is ScriptBool))
                    throw new SystemException($"Cannot compare {compareResult}");

                List<AstNode> currentBranch = null;
                if (((ScriptBool)compareResult.Value).Value)
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
                            var value = this.InterpretExpression(returnNode.Value);

                            this.currentScope = this.currentScope.Parent;
                            return value;
                        }

                        this.currentScope = this.currentScope.Parent;
                        return new ValueReference(ScriptValue.Null);
                    }
                    else
                    {
                        this.InterpretChild(item);
                    }
                }

                this.currentScope = this.currentScope.Parent;
                return null;
            }
            else if (node is SwitchNode)
            {
                var realNode = (SwitchNode)node;
                var switchValue = this.InterpretExpression(realNode.Condition);
                bool hasMatchCase = false;

                foreach (var caseNode in realNode.Cases)
                {
                    foreach (var condition in caseNode.Conditions)
                    {
                        var caseExpression = this.InterpretExpression(condition);
                        if (caseExpression.Ref.Equals(switchValue.Ref))
                        {
                            hasMatchCase = true;
                            foreach (var item in caseNode.Body)
                            {
                                this.currentScope = new ScriptScope(ScopeType.Conditon, this.currentScope);
                                if (item is ReturnNode)
                                {
                                    var returnNode = (ReturnNode)item;

                                    if (returnNode.Value != null)
                                    {
                                        var value = this.InterpretExpression(returnNode.Value);

                                        this.currentScope = this.currentScope.Parent;
                                        return value;
                                    }

                                    this.currentScope = this.currentScope.Parent;
                                    return new ValueReference(ScriptValue.Null);
                                }
                                else if (item is BreakNode)
                                {
                                    return null;
                                }
                                else
                                {
                                    this.InterpretChild(item);
                                }
                                this.currentScope = this.currentScope.Parent;
                            }
                        }
                    }
                }

                if (realNode.DefaultCase == null || hasMatchCase)
                    return null;
                this.currentScope = new ScriptScope(ScopeType.Conditon, this.currentScope);
                foreach (var item in realNode.DefaultCase.Body)
                {
                    if (item is ReturnNode)
                    {
                        var returnNode = (ReturnNode)item;

                        if (returnNode.Value != null)
                        {
                            var value = this.InterpretExpression(returnNode.Value);

                            this.currentScope = this.currentScope.Parent;
                            return value;
                        }

                        this.currentScope = this.currentScope.Parent;
                        return new ValueReference(ScriptValue.Null);
                    }
                    else
                    {
                        this.InterpretChild(item);
                    }
                }
                this.currentScope = this.currentScope.Parent;
                return null;
            }
            else if (node is WhileNode)
            {
                var realNode = (WhileNode)node;
                var condition = realNode.Condition;

                while (((ScriptBool)(this.InterpretExpression(condition).Ref.Value)).Value)
                {
                    this.currentScope = new ScriptScope(ScopeType.Loop, this.currentScope);
                    var hasBreak = false;

                    foreach (var bodyNode in realNode.Body)
                    {
                        var hasContinue = false;
                        if (bodyNode is ReturnNode)
                        {
                            var returnNode = (ReturnNode)bodyNode;

                            if (returnNode.Value != null)
                            {
                                var value = this.InterpretExpression(returnNode.Value);

                                this.currentScope = this.currentScope.Parent;
                                return value;
                            }
                            this.currentScope = this.currentScope.Parent;
                            return new ValueReference(ScriptValue.Null);
                        }

                        if (bodyNode is BreakNode)
                            hasBreak = true;

                        if (bodyNode is ContinueNode)
                            hasContinue = true;

                        if (!hasBreak && !hasContinue)
                            this.InterpretChild(bodyNode);

                        /*if (((ScriptBool)(this.InterpretExpression(condition).Ref.Value)).Value)
                            break;*/
                    }

                    this.currentScope = this.currentScope.Parent;
                    if (hasBreak)
                        break;
                }

                return null;
            }
            else if (node is ForNode)
            {
                var realNode = (ForNode)node;
                var location = this.InterpretExpression(realNode.Location).Ref;

                if (!(location.Value is IEnumerable<ScriptValue>))
                    throw new SystemException("For loop running failed, variable is not enumerable");

                var list = (IEnumerable<ScriptValue>)location.Value;

                /*this.currentScope = new ScriptScope(ScopeType.Loop, this.currentScope);

                this.currentScope.Variables[realNode.Initializer] =
                    new ScriptVariable(realNode.Initializer, new ScriptValue(0), false);*/

                var hasBreak = false;

                foreach (var item in list)
                {
                    this.currentScope = new ScriptScope(ScopeType.Loop, this.currentScope);

                    this.currentScope.Variables[realNode.Initializer] =
                        new ScriptVariable(realNode.Initializer, item, false);

                    var hasContinue = false;

                    foreach (var bodyNode in realNode.Body)
                    {
                        if (bodyNode is ReturnNode)
                        {
                            var returnNode = (ReturnNode)bodyNode;

                            if (returnNode.Value != null)
                            {
                                var value = this.InterpretExpression(returnNode.Value);

                                this.currentScope = this.currentScope.Parent;
                                return value;
                            }

                            this.currentScope = this.currentScope.Parent;
                            return new ValueReference(ScriptValue.Null);
                        }

                        if (bodyNode is BreakNode)
                            hasBreak = true;

                        if (bodyNode is ContinueNode)
                            hasContinue = true;

                        if (!hasBreak && !hasContinue)
                            this.InterpretChild(bodyNode);
                    }

                    this.currentScope = this.currentScope.Parent;
                    if (hasBreak)
                        break;
                }

                //this.currentScope = this.currentScope.Parent;
                return null;
            }
            else if (node is ListNode)
            {
                var realNode = (ListNode)node;

                List<ScriptValue> values = new List<ScriptValue>();

                foreach (var value in realNode.Elements)
                    values.Add(this.InterpretExpression(value).Ref);

                return new ValueReference(new ScriptValue(values));
            }
            else if (node is DictNode)
            {
                var realNode = (DictNode)node;

                var keyValues = new Dictionary<ScriptValue, ScriptValue>();

                foreach (var keyValue in realNode.KeyValues)
                {
                    var key = this.InterpretExpression(keyValue.Item1).Ref;
                    var value = this.InterpretExpression(keyValue.Item2).Ref;
                    keyValues[key] = value;
                }

                return new ValueReference(new ScriptValue(keyValues));
            }
            else if (node is IndexersNode)
            {
                var realNode = (IndexersNode)node;

                var list = this.InterpretExpression(realNode.Object);

                if (list.Ref.Value is ScriptList)
                {
                    ScriptList listValue = (ScriptList)list.Ref.Value;

                    var index = this.InterpretExpression(realNode.Index);

                    if (!(index.Ref.Value is ScriptInt))
                        throw new SystemException("Index in List must be integer");

                    ScriptInt intValue = (ScriptInt)index.Ref.Value;

                    if (intValue.Value < 0 || intValue.Value >= listValue.Value.Count)
                        throw new SystemException("Index out of range");
                    return new ValueReference(() => listValue.Value[(int)intValue.Value], x => listValue.Value[(int)intValue.Value] = x);
                }
                else if (list.Ref.Value is ScriptDict)
                {
                    ScriptDict dictValue = (ScriptDict)list.Ref.Value;

                    var index = this.InterpretExpression(realNode.Index);

                    ScriptValue accessValue = index.Ref;

                    return new ValueReference(() => dictValue.Value[accessValue], x => dictValue.Value[accessValue] = x);
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
                return new ValueReference(
                    new ScriptValue(new ScriptFn(realNode.Name, realNode.Parameters, realNode.Body,
                                this.currentScope, null)));
            }
            else if (node is TernaryNode)
            {
                var realNode = (TernaryNode)node;
                var condition = this.InterpretExpression(realNode.Condition);
                if (!(condition.Ref.Value is ScriptBool))
                    throw new SystemException("Ternary condition should be boolean");
                if (((ScriptBool)condition.Ref.Value).Value)
                {
                    return this.InterpretExpression(realNode.Consequent);
                }
                return this.InterpretExpression(realNode.Alternative);
            }
            throw new SystemException("Unexcepted interpret expression: " + node.GetType().Name);
        }
    }
}
