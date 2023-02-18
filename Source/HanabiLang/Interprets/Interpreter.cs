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
            this.currentScope = new ScriptScope(null);
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
            Type csharpType = Type.GetType(realNode.Path);
            if (csharpType != null)
            {
                string className = string.IsNullOrEmpty(realNode.AsName) ? csharpType.Name : realNode.AsName;

                if (!ImportedItems.Types.TryGetValue(csharpType, out ScriptClass scriptClass))
                {
                    bool isStruct = csharpType.IsValueType && !csharpType.IsEnum;
                    bool isEnum = csharpType.IsValueType;
                    bool isClass = csharpType.IsClass;
                    if (!isClass && !isStruct/* && !isEnum*/)
                        throw new SystemException("Only C# class/struct can be imported");

                    bool isStatic = csharpType.IsAbstract && csharpType.IsSealed;

                    scriptClass = new ScriptClass(className, null, null, BasicTypes.ObjectClass, isStatic, AccessibilityLevel.Public);
                    BuildInClasses.CSharpClassToScriptClass(scriptClass, csharpType, isStatic);
                    ImportedItems.Types[csharpType] = scriptClass;
                }

                ScriptScope scriptScope = scriptClass.Scope;

                if (realNode.Imports == null)
                {
                    interpretScope.Classes[className] = scriptClass;
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

            string fullPath = System.IO.Path.GetFullPath(realNode.Path);

            if (!System.IO.File.Exists(fullPath))
                throw new SystemException($"File {realNode.Path} not found");
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullPath);
            string extension = System.IO.Path.GetExtension(fullPath).ToLower();
            if (extension.Equals(".json"))
            {
                if (!ImportedItems.Files.TryGetValue(fullPath, out Interpreter interpreter))
                {
                    List<string> lines = new List<string>();
                    lines.Add("const jsonData = ");
                    lines.AddRange(System.IO.File.ReadAllLines(fullPath));
                    var tokens = Lexer.Tokenize(lines);
                    var parser = new Parser(tokens);
                    var ast = parser.Parse();
                    //Interpreter interpreter = new Interpreter(ast, fullPath, false, this.Arguments);
                    interpreter = new Interpreter(ast, fullPath, false);
                    interpreter.Interpret();
                    ImportedItems.Files[fullPath] = interpreter;
                }

                var jsonData = interpreter.currentScope.Variables["jsonData"].Value;
                if (string.IsNullOrEmpty(realNode.AsName))
                    interpretScope.Variables[fileNameWithoutExtension] = new ScriptVariable(fileNameWithoutExtension, jsonData, true, true, AccessibilityLevel.Public);
                else
                    interpretScope.Variables[realNode.AsName] = new ScriptVariable(realNode.AsName, jsonData, true, true, AccessibilityLevel.Public);
            }
            else
            {
                if (!ImportedItems.Files.TryGetValue(fullPath, out Interpreter interpreter))
                {
                    string[] lines = System.IO.File.ReadAllLines(fullPath);
                    var tokens = Lexer.Tokenize(lines);
                    var parser = new Parser(tokens);
                    var ast = parser.Parse();
                    //Interpreter interpreter = new Interpreter(ast, fullPath, false, this.Arguments);
                    interpreter = new Interpreter(ast, fullPath, false);
                    interpreter.Interpret();
                    ImportedItems.Files[fullPath] = interpreter;
                }

                if (realNode.Imports == null)
                {
                    if (string.IsNullOrEmpty(realNode.AsName))
                        interpretScope.Classes[fileNameWithoutExtension] = new
                            ScriptClass(fileNameWithoutExtension, interpreter.ast.Nodes,
                                interpreter.currentScope, BasicTypes.ObjectClass, true, AccessibilityLevel.Public, true);
                    else
                        interpretScope.Classes[realNode.AsName] = new
                            ScriptClass(realNode.AsName, interpreter.ast.Nodes,
                                interpreter.currentScope, BasicTypes.ObjectClass, true, AccessibilityLevel.Public, true);
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

                ScriptValue setValue = realNode.Value == null ? new ScriptValue() :
                    InterpretExpression(interpretScope, realNode.Value).Ref;


                ScriptFns getFns = null;
                ScriptFns setFns = null;

                if (realNode.GetFn != null)
                {
                    if (realNode.GetFn.Body.Count == 0)
                    {
                        getFns = new ScriptFns($"get_{realNode.Name}");
                        getFns.Fns.Add(new ScriptFn(new List<FnParameter>(), null, args => setValue, realNode.GetFn.IsStatic, realNode.GetFn.Level));
                    }
                    else
                    {
                        var fn = InterpretExpression(interpretScope, realNode.GetFn).Ref;
                        if (!fn.IsFunction)
                            throw new SystemException("Getter must be a function");
                        getFns = (ScriptFns)fn.Value;
                    }
                }
                if (realNode.SetFn != null)
                {
                    if (realNode.GetFn.Body.Count == 0)
                    {
                        setFns = new ScriptFns($"set_{realNode.Name}");
                        if (interpretScope.Type is ScriptObject)
                            setFns.Fns.Add(new ScriptFn(new List<FnParameter>() { new FnParameter("value") },
                                null, args => setValue = args[1], realNode.SetFn.IsStatic, realNode.SetFn.Level));
                        else
                            setFns.Fns.Add(new ScriptFn(new List<FnParameter>() { new FnParameter("value") },
                                null, args => setValue = args[0], realNode.SetFn.IsStatic, realNode.SetFn.Level));
                    }
                    else
                    {
                        var fn = InterpretExpression(interpretScope, realNode.SetFn).Ref;
                        if (!fn.IsFunction)
                            throw new SystemException("Setter must be a function");
                        setFns = (ScriptFns)fn.Value;
                    }
                }

                if (getFns != null || setFns != null)
                {
                    interpretScope.Variables[realNode.Name] = new ScriptVariable(realNode.Name, getFns, setFns,
                        realNode.IsConstant, realNode.IsStatic, realNode.Level);
                }
                else
                {
                    interpretScope.Variables[realNode.Name] = new ScriptVariable(realNode.Name,
                        setValue,realNode.IsConstant, realNode.IsStatic, realNode.Level);
                }
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
                    realNode.Body, interpretScope, realNode.IsStatic, realNode.Level));
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

                //var scope = new ScriptScope(ScopeType.Class, interpretScope);
                interpretScope.Classes[realNode.Name] = new ScriptClass(realNode.Name, realNode.Body,
                    interpretScope, BasicTypes.ObjectClass, realNode.IsStatic, realNode.Level);
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
                    ScriptScope leftScope = null;
                    if (left.Value is ScriptClass)
                        leftScope = ((ScriptClass)left.Value).Scope;
                    else if (left.Value is ScriptFns)
                        throw new SystemException("Function cannot use operator '.'");
                    else
                        leftScope = ((ScriptObject)left.Value).Scope;

                    bool isPrivateAccess = interpretScope.ContainsScope(leftScope);
                    AccessibilityLevel accessLevel = isPrivateAccess ? AccessibilityLevel.Private : AccessibilityLevel.Public;
                    bool isStaticAccess = left.Value is ScriptClass;

                    if (realNode.Right is FnReferenceCallNode)
                    {
                        var fnCall = (FnReferenceCallNode)realNode.Right;
                        if (!(fnCall.Reference is VariableReferenceNode))
                            throw new SystemException($"Unexpected function call");
                        string fnName = ((VariableReferenceNode)fnCall.Reference).Name;

                        if (leftScope.TryGetValue(fnName, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptFns)
                            {
                                var fn = ((ScriptFns)scriptType);
                                var fnInfo = fn.GetFnInfo(interpretScope, fnCall.Args);
                                if ((int)accessLevel < (int)fnInfo.Item1.Level)
                                    throw new SystemException($"Cannot access {fnInfo.Item1.Level} {fn.Name}");
                                return new ValueReference(fn.Call(isStaticAccess ? null : (ScriptObject)left.Value, fnInfo));
                            }
                            else if (scriptType is ScriptClass)
                            {
                                var _class = ((ScriptClass)scriptType);
                                if ((int)accessLevel < (int)_class.Level)
                                    throw new SystemException($"Cannot access {_class.Level} {_class.Name}");
                                return new ValueReference(((ScriptClass)scriptType).Call(interpretScope, fnCall.Args));
                            }
                            else if (scriptType is ScriptVariable &&
                                ((ScriptVariable)(scriptType)).Value.IsFunction)
                            {
                                var fn = ((ScriptFns)scriptType);
                                var fnInfo = fn.GetFnInfo(interpretScope, fnCall.Args);
                                return new ValueReference(fn.Call(isStaticAccess ? null : (ScriptObject)left.Value, fnInfo));
                            }
                            else
                            {
                                throw new SystemException($"{fnName} is not callable");
                            }
                        }
                        else
                        {
                            throw new SystemException($"{left} does not contains {fnName}");
                        }
                    }
                    else if (realNode.Right is VariableReferenceNode)
                    {
                        var varRef = (VariableReferenceNode)realNode.Right;
                        if (leftScope.TryGetValue(varRef.Name, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptFns)
                                return new ValueReference(new ScriptValue((ScriptFns)scriptType));
                            else if (scriptType is ScriptClass)
                                return new ValueReference(new ScriptValue((ScriptClass)scriptType));
                            else if (scriptType is ScriptVariable)
                            {
                                var variable = (ScriptVariable)scriptType;
                                if ((int)accessLevel < (int)variable.Level)
                                    throw new SystemException($"Cannot access {variable.Level} {variable.Name}");

                                if (variable.Value == null)
                                {
                                    ScriptObject _this = isStaticAccess ? null : (ScriptObject)left.Value;
                                    return new ValueReference(
                                        () =>
                                        {
                                            if (variable.Get == null)
                                                throw new SystemException($"{variable.Name} cannot be read");
                                            var fnInfo = variable.Get.GetFnInfo();
                                            if ((int)accessLevel < (int)fnInfo.Item1.Level)
                                                throw new SystemException($"{variable.Name} cannot be read");
                                            return variable.Get.Call(_this, fnInfo);
                                        },
                                        x =>
                                        {
                                            if (variable.Set == null)
                                                throw new SystemException($"{variable.Name} cannot be written");
                                            var fnInfo = variable.Set.GetFnInfo(x);
                                            if ((int)accessLevel < (int)fnInfo.Item1.Level)
                                                throw new SystemException($"{variable.Name} cannot be written");
                                            variable.Set.Call(_this, fnInfo);
                                        }
                                    );
                                }
                                return new ValueReference(() => variable.Value, x => variable.Value = x);
                            }
                            else
                                throw new SystemException($"Unexcepted reference to variable");
                        }
                        else
                        {
                            throw new SystemException($"{left} does not contains {varRef.Name}");
                        }
                    }
                    throw new SystemException($"Unexcepted operation {left}'.'");
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
                            if (variable.Value == null)
                            {
                                return new ValueReference(
                                    () =>
                                    {
                                        if (variable.Get == null)
                                            throw new SystemException($"{realNode.Name} cannot be read");
                                        return variable.Get.Call(null);
                                    },
                                    x => 
                                    {
                                        if (variable.Set == null)
                                            throw new SystemException($"{realNode.Name} cannot be written");
                                        variable.Set.Call(null, x);
                                    });
                            }
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
                    var fnInfo = fn.GetFnInfo(interpretScope, realNode.Args);
                    return new ValueReference(fn.Call(null, fnInfo));
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

                var scope = new ScriptScope(null, interpretScope);

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
                                var scope = new ScriptScope(null, interpretScope);
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
                var defaultScope = new ScriptScope(null, interpretScope);
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
                    var scope = new ScriptScope(null, interpretScope);
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

                var enumeratorInfo = ((ScriptFns)getEnumerator).GetFnInfo(interpretScope, new Dictionary<string, AstNode>());
                var enumerator = ((ScriptFns)getEnumerator).Call(scriptObject, enumeratorInfo);

                if (!(((ScriptObject)enumerator.Value).BuildInObject is IEnumerable<ScriptValue>))
                    throw new SystemException("For loop running failed, variable is not enumerable");

                var list = (IEnumerable<ScriptValue>)(((ScriptObject)enumerator.Value).BuildInObject);

                /*this.currentScope = new ScriptScope(ScopeType.Loop, this.currentScope);

                this.currentScope.Variables[realNode.Initializer] =
                    new ScriptVariable(realNode.Initializer, new ScriptValue(0), false);*/

                var hasBreak = false;

                foreach (var item in list)
                {
                    var scope = new ScriptScope(null, interpretScope);

                    scope.Variables[realNode.Initializer] =
                        new ScriptVariable(realNode.Initializer, item, false, true, AccessibilityLevel.Private);

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
                                interpretScope, realNode.IsStatic, realNode.Level));
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
