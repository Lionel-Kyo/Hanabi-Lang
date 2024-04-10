using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HanabiLang.Parses;
using HanabiLang.Lexers;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Interprets.Exceptions;
using HanabiLang.Parses.Nodes;
using System.Threading;

namespace HanabiLang.Interprets
{
    class Interpreter
    {
        public ScriptScope PredefinedScope { get; private set; }
        public ScriptScope CurrentScope { get; private set; }
        private AbstractSyntaxTree ast { get; set; }
        public string Path { get; private set; }
        public static IEnumerable<string> Arguments { get; set; }

        /// <summary>
        /// Script Start or Import Script
        /// </summary>
        /// <param name="ast"></param>
        public Interpreter(AbstractSyntaxTree ast, ScriptScope predefinedScope, string path, bool isMain)
        {
            this.ast = ast;
            this.PredefinedScope = predefinedScope;
            this.CurrentScope = new ScriptScope(null, this);
            this.Path = path.Replace("\\", "/");
            this.CurrentScope.Classes["Script"] = new ScriptScript(isMain, Arguments);
            this.CurrentScope.Classes.Add("object", BasicTypes.ObjectClass);
            this.CurrentScope.Classes.Add("str", BasicTypes.Str);
            this.CurrentScope.Classes.Add("int", BasicTypes.Int);
            this.CurrentScope.Classes.Add("float", BasicTypes.Float);
            this.CurrentScope.Classes.Add("decimal", BasicTypes.Decimal);
            this.CurrentScope.Classes.Add("bool", BasicTypes.Bool);
            this.CurrentScope.Classes.Add("range", BasicTypes.Range);
            this.CurrentScope.Classes.Add("List", BasicTypes.List);
            this.CurrentScope.Classes.Add("Dict", BasicTypes.Dict);
            this.CurrentScope.Classes.Add("Enumerator", BasicTypes.Enumerator);
            this.CurrentScope.Classes.Add("Exception", BasicTypes.Exception);
            BuildInFns.AddBasicFunctions(this.CurrentScope);
        }

        public void Interpret(bool isThrowException)
        {
            try
            {
                if (this.PredefinedScope != null)
                {
                    foreach (var kv in this.PredefinedScope.Classes)
                    {
                        this.CurrentScope.Classes[kv.Key] = kv.Value;
                    }
                    foreach (var kv in this.PredefinedScope.Functions)
                    {
                        this.CurrentScope.Functions[kv.Key] = kv.Value;
                    }
                    foreach (var kv in this.PredefinedScope.Variables)
                    {
                        this.CurrentScope.Variables[kv.Key] = kv.Value;
                    }
                }

                foreach (var child in this.ast.Nodes)
                {
                    InterpretChild(this.CurrentScope, child);
                }
            }
            catch (Exception ex)
            {
                if (isThrowException)
                {
                    throw ex;
                }
                else
                {
                    StringBuilder result = new StringBuilder();
                    for (Exception exception = ex; exception != null; exception = exception.InnerException)
                    {
                        if (exception is HanibiException)
                            result.AppendLine($"Unhandled Exception ({((HanibiException)exception).Name}): {exception.Message}");
                        else
                            result.AppendLine($"Unhandled Exception ({exception.GetType().Name}): {exception.Message}");
                    }
                    Console.Error.WriteLine(result.ToString());
                    Environment.ExitCode = ex.HResult;
                }
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
                    scriptClass = BuildInClasses.CSharpClassToScriptClass(csharpType, className);
                }

                ScriptScope scriptScope = scriptClass.Scope;

                // Import as variable
                if (realNode.Imports == null)
                {
                    interpretScope.Classes[className] = scriptClass;
                }
                // Import all
                else if (realNode.Imports.Count <= 0)
                {
                    foreach (var kv in scriptScope.Classes)
                    {
                        if (kv.Value.Level != AccessibilityLevel.Public)
                            continue;

                        interpretScope.Classes[kv.Key] = kv.Value;
                    }
                    foreach (var kv in scriptScope.Functions)
                    {
                        interpretScope.Functions[kv.Key] = kv.Value;
                    }
                    foreach (var kv in scriptScope.Variables)
                    {
                        if (kv.Value.Level != AccessibilityLevel.Public)
                            continue;
                        interpretScope.Variables[kv.Key] = kv.Value;
                    }
                }
                // Import some
                else
                {
                    foreach (string item in realNode.Imports)
                    {
                        if (scriptScope.TryGetValue(item, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptClass)
                            {
                                if (((ScriptClass)scriptType).Level != AccessibilityLevel.Public)
                                    continue;
                                interpretScope.Classes[((ScriptClass)scriptType).Name] = (ScriptClass)scriptType;
                            }
                            if (scriptType is ScriptFns)
                            {
                                interpretScope.Functions[((ScriptFns)scriptType).Name] = (ScriptFns)scriptType;
                            }
                            else if (scriptType is ScriptVariable)
                            {
                                if (((ScriptVariable)scriptType).Level != AccessibilityLevel.Public)
                                    continue;
                                interpretScope.Variables[((ScriptVariable)scriptType).Name] = (ScriptVariable)scriptType;
                            }
                            else
                            {
                                throw new SystemException($"Unexpected script type");
                            }
                        }
                        else
                            throw new SystemException($"{item} is not defined in realNode.Path");
                    }
                }
                return;
            }

            List<string> fullPaths = new List<string>();
            if (!string.IsNullOrEmpty(interpretScope?.ParentInterpreter?.Path))
                fullPaths.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(interpretScope.ParentInterpreter.Path), realNode.Path));
            fullPaths.Add(realNode.Path);
            fullPaths = fullPaths.Select(x => System.IO.Path.GetFullPath(x).Replace("\\", "/")).ToList();

            int fullPathIndex = fullPaths.FindIndex(x => System.IO.File.Exists(x));
            if (fullPathIndex < 0)
                throw new SystemException($"File {realNode.Path} not found");

            string fullPath = fullPaths[fullPathIndex];
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullPath);
            string extension = System.IO.Path.GetExtension(fullPath).ToLower();
            DateTime lastWriteTimeUtc = System.IO.File.GetLastWriteTimeUtc(fullPath);
            if (extension.Equals(".json"))
            {
                Interpreter newInterpreter = null;
                if (!ImportedItems.Files.TryGetValue(fullPath, out Tuple<DateTime, Interpreter> scriptInfo) || lastWriteTimeUtc != scriptInfo.Item1)
                {
                    List<string> lines = new List<string>();
                    lines.Add("const jsonData = ");
                    lines.AddRange(System.IO.File.ReadAllLines(fullPath));
                    var tokens = Lexer.Tokenize(lines);
                    var parser = new Parser(tokens);
                    var ast = parser.Parse();
                    //Interpreter interpreter = new Interpreter(ast, fullPath, false, this.Arguments);
                    newInterpreter = new Interpreter(ast, interpretScope?.ParentInterpreter?.PredefinedScope, fullPath, false);
                    newInterpreter.Interpret(true);
                    ImportedItems.Files[fullPath] = Tuple.Create(lastWriteTimeUtc, newInterpreter);
                }
                else
                {
                    newInterpreter = scriptInfo.Item2;
                }

                var jsonData = newInterpreter.CurrentScope.Variables["jsonData"].Value;
                if (string.IsNullOrEmpty(realNode.AsName))
                    interpretScope.Variables[fileNameWithoutExtension] = new ScriptVariable(fileNameWithoutExtension, jsonData, true, true, AccessibilityLevel.Public);
                else
                    interpretScope.Variables[realNode.AsName] = new ScriptVariable(realNode.AsName, jsonData, true, true, AccessibilityLevel.Public);
            }
            else
            {
                Interpreter newInterpreter = null;
                if (!ImportedItems.Files.TryGetValue(fullPath, out Tuple<DateTime, Interpreter> scriptInfo) || lastWriteTimeUtc != scriptInfo.Item1)
                {
                    string[] lines = System.IO.File.ReadAllLines(fullPath);
                    var tokens = Lexer.Tokenize(lines);
                    var parser = new Parser(tokens);
                    var ast = parser.Parse();
                    //Interpreter interpreter = new Interpreter(ast, fullPath, false, this.Arguments);
                    newInterpreter = new Interpreter(ast, interpretScope?.ParentInterpreter?.PredefinedScope, fullPath, false);
                    newInterpreter.Interpret(true);
                    ImportedItems.Files[fullPath] = Tuple.Create(lastWriteTimeUtc, newInterpreter);
                }
                else
                {
                    newInterpreter = scriptInfo.Item2;
                }

                // Import as variable
                if (realNode.Imports == null)
                {
                    if (string.IsNullOrEmpty(realNode.AsName))
                        interpretScope.Classes[fileNameWithoutExtension] = new
                            ScriptClass(fileNameWithoutExtension, newInterpreter.ast.Nodes,
                                newInterpreter.CurrentScope, null, true, AccessibilityLevel.Public, true);
                    else
                        interpretScope.Classes[realNode.AsName] = new
                            ScriptClass(realNode.AsName, newInterpreter.ast.Nodes,
                                newInterpreter.CurrentScope, null, true, AccessibilityLevel.Public, true);
                }                
                // Import all
                else if (realNode.Imports.Count <= 0)
                {
                    foreach (var kv in newInterpreter.CurrentScope.Classes)
                    {
                        if (kv.Value.Level != AccessibilityLevel.Public)
                            continue;
                        interpretScope.Classes[kv.Key] = kv.Value;
                    }
                    foreach (var kv in newInterpreter.CurrentScope.Functions)
                    {
                        interpretScope.Functions[kv.Key] = kv.Value;
                    }
                    foreach (var kv in newInterpreter.CurrentScope.Variables)
                    {
                        if (kv.Value.Level != AccessibilityLevel.Public)
                            continue;
                        interpretScope.Variables[kv.Key] = kv.Value;
                    }
                }
                // Import some
                else
                {
                    foreach (string item in realNode.Imports)
                    {
                        if (interpretScope.TryGetValue(item, out _))
                            throw new SystemException($"Import failed, value {item} exists");

                        if (newInterpreter.CurrentScope.TryGetValue(item, out ScriptType scriptType))
                        {
                            if (scriptType is ScriptClass)
                            {
                                if (((ScriptClass)scriptType).Level != AccessibilityLevel.Public)
                                    continue;
                                interpretScope.Classes[((ScriptClass)scriptType).Name] = (ScriptClass)scriptType;
                            }
                            if (scriptType is ScriptFns)
                            {
                                interpretScope.Functions[((ScriptFns)scriptType).Name] = (ScriptFns)scriptType;
                            }
                            else if (scriptType is ScriptVariable)
                            {
                                if (((ScriptVariable)scriptType).Level != AccessibilityLevel.Public)
                                    continue;
                                interpretScope.Variables[((ScriptVariable)scriptType).Name] = (ScriptVariable)scriptType;
                            }
                            else
                            {
                                throw new SystemException($"Unexpected script type");
                            }
                        }
                        else
                            throw new SystemException($"{item} is not defined in realNode.Path");
                    }
                }
            }
        }

        private static ValueReference VariableReference(VariableReferenceNode node, ScriptValue left,
            ScriptScope leftScope, AccessibilityLevel accessLevel, bool isStaticAccess)
        {
            if (leftScope.TryGetValue(node.Name, out ScriptType scriptType))
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
                                var fnInfo = variable.Get.FindCallableInfo();
                                if ((int)accessLevel < (int)fnInfo.Item1.Level)
                                    throw new SystemException($"{variable.Name} cannot be read");
                                return variable.Get.Call(_this, fnInfo);
                            },
                            x =>
                            {
                                if (variable.IsConstant || variable.Set == null)
                                    throw new SystemException($"{variable.Name} cannot be written");

                                var fnInfo = variable.Set.FindCallableInfo(x);
                                if ((int)accessLevel < (int)fnInfo.Item1.Level)
                                    throw new SystemException($"{variable.Name} cannot be written");
                                variable.Set.Call(_this, fnInfo);
                            }
                        );
                    }
                    return new ValueReference(() => variable.Value, x =>
                    {
                        if (variable.IsConstant)
                            throw new SystemException($"const {variable.Name} cannot be written");
                        variable.Value = x;
                    });
                }
                else
                    throw new SystemException($"Unexcepted reference to variable");
            }
            else
            {
                throw new SystemException($"{left} does not contains {node.Name}");
            }
        }

        private static ValueReference VariableReference(VariableReferenceNode node, ScriptScope interpretScope)
        {
            if (node.Name.Equals("this"))
            {
                if (interpretScope.Type is ScriptObject)
                    return new ValueReference(new ScriptValue((ScriptObject)interpretScope.Type));
                else if (interpretScope.Type is ScriptFn)
                {
                    for (ScriptScope scope = interpretScope.Parent; scope != null; scope = scope.Parent)
                    {
                        if (scope.Type is ScriptObject)
                            return new ValueReference(new ScriptValue((ScriptObject)scope.Type));
                    }
                    throw new SystemException($"Unexpected keyword this, cannot call this outside the object");
                }
                throw new SystemException($"Unexpected keyword this");
            }
            else if (node.Name.Equals("super"))
                throw new SystemException($"super keyword cannot used without reference to its member");

            // Checking if the variable is defined in any scope above the current one
            for (var scope = interpretScope; scope != null; scope = scope.Parent)
            {
                if (scope.TryGetValue(node.Name, out ScriptType scriptType))
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
                                        throw new SystemException($"{node.Name} cannot be read");
                                    return variable.Get.Call(null);
                                },
                                x =>
                                {
                                    if (variable.Set == null)
                                        throw new SystemException($"{node.Name} cannot be written");
                                    variable.Set.Call(null, x);
                                });
                        }
                        return new ValueReference(() => variable.Value, x =>
                        {
                            if (variable.IsConstant)
                                throw new SystemException($"const {variable.Name} cannot be written");
                            variable.Value = x;
                        });
                    }
                    else if (scriptType is ScriptObject)
                    {
                        var _object = (ScriptObject)scriptType;
                        return new ValueReference(new ScriptValue(_object));
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

            throw new SystemException($"{node.Name} is not defined");
        }

        private static ValueReference VariableDefinition(VariableDefinitionNode node, ScriptScope scope)
        {
            if (scope.Variables.TryGetValue(node.Name, out ScriptVariable existedVariable))
            {
                if (existedVariable.IsConstant)
                    throw new SystemException($"Cannot reassigned constant variable {node.Name}");
            }
             
            ScriptValue setValue = node.Value == null ? new ScriptValue() :
                InterpretExpression(scope, node.Value).Ref;


            ScriptFns getFns = null;
            ScriptFns setFns = null;

            if (node.GetFn != null)
            {
                if (node.GetFn.Body.Count == 0)
                {
                    getFns = new ScriptFns($"get_{node.Name}");
                    getFns.Fns.Add(new ScriptFn(new List<FnParameter>(), null, args => setValue, node.GetFn.IsStatic, node.GetFn.Level));
                }
                else
                {
                    var fn = InterpretExpression(scope, node.GetFn).Ref;
                    if (!fn.IsFunction)
                        throw new SystemException("Getter must be a function");
                    getFns = (ScriptFns)fn.Value;
                }
            }
            if (node.SetFn != null)
            {
                if (node.GetFn.Body.Count == 0)
                {
                    setFns = new ScriptFns($"set_{node.Name}");
                    if (scope.Type is ScriptObject)
                        setFns.Fns.Add(new ScriptFn(new List<FnParameter>() { new FnParameter("value") },
                            null, args => setValue = args[1], node.SetFn.IsStatic, node.SetFn.Level));
                    else
                        setFns.Fns.Add(new ScriptFn(new List<FnParameter>() { new FnParameter("value") },
                            null, args => setValue = args[0], node.SetFn.IsStatic, node.SetFn.Level));
                }
                else
                {
                    var fn = InterpretExpression(scope, node.SetFn).Ref;
                    if (!fn.IsFunction)
                        throw new SystemException("Setter must be a function");
                    setFns = (ScriptFns)fn.Value;
                }
            }

            if (getFns != null || setFns != null)
            {
                var variable = new ScriptVariable(node.Name, getFns, setFns,
                    node.IsConstant, node.IsStatic, node.Level);
                scope.Variables[node.Name] = variable;
                return new ValueReference(variable.Value);
            }
            else
            {
                var variable = new ScriptVariable(node.Name,
                    setValue, node.IsConstant, node.IsStatic, node.Level);
                scope.Variables[node.Name] = variable;
                return new ValueReference(variable.Value);
            }
        }

        private static ValueReference VariableAssignment(VariableAssignmentNode node, ScriptScope scope)
        {
            ValueReference left = InterpretExpression(scope, node.Name);
            var assignValue = InterpretExpression(scope, node.Value);
            left.Ref = assignValue.Ref;
            return left;
        }

        private static ValueReference FnReferenceCall(FnReferenceCallNode realNode, ScriptScope interpretScope)
        {
            string specialAccess = "";
            if (realNode.Reference is VariableReferenceNode)
            {
                string rawRef = ((VariableReferenceNode)realNode.Reference).Name;
                if (rawRef.Equals("this") || rawRef.Equals("super"))
                    specialAccess = rawRef;
            }

            ScriptValue fnRef = specialAccess.Length <= 0 ? InterpretExpression(interpretScope, realNode.Reference).Ref : null;

            if (fnRef == null)
            {
                // InterpretScope Type is ScriptFn not ScriptFns
                if (!(interpretScope.Type is ScriptFn))
                    throw new SystemException("Cannot call this/super function out of function");
                if (!(interpretScope.Parent.Type is ScriptObject))
                    throw new SystemException("Cannot call this/super function out of object");

                var scriptObject = (ScriptObject)interpretScope.Parent.Type;
                if (specialAccess.Equals("this") &&
                    scriptObject.ClassType.Scope.TryGetValue(scriptObject.ClassType.Name, out ScriptType thisFns))
                {
                    var fn = (ScriptFns)thisFns;
                    var callableInfo = fn.FindCallableInfo(interpretScope, realNode.Args, realNode.KeyArgs);
                    return new ValueReference(fn.Call(scriptObject, callableInfo));
                }
                else if (specialAccess.Equals("super") &&
                    scriptObject.ClassType.SuperClass.Scope.TryGetValue(scriptObject.ClassType.SuperClass.Name, out ScriptType superFns))
                {
                    var fn = (ScriptFns)superFns;
                    var callableInfo = fn.FindCallableInfo(interpretScope, realNode.Args, realNode.KeyArgs);
                    return new ValueReference(fn.Call(scriptObject, callableInfo));
                }
            }
            else if (fnRef.IsFunction)
            {
                var fn = (ScriptFns)fnRef.Value;
                var callableInfo = fn.FindCallableInfo(interpretScope, realNode.Args, realNode.KeyArgs);
                return new ValueReference(fn.Call(null, callableInfo));
            }
            else if (fnRef.IsClass)
            {
                var _class = (ScriptClass)fnRef.Value;
                return new ValueReference(_class.Call(interpretScope, realNode.Args, realNode.KeyArgs));
            }

            throw new SystemException($"{realNode.Reference.NodeName} is not a class or a function");
        }

        private static ValueReference FnReferenceCall(FnReferenceCallNode fnCall, 
            ScriptScope interpretScope, ScriptValue left, ScriptScope leftScope, 
            AccessibilityLevel accessLevel, bool isStaticAccess)
        {
            if (fnCall.Reference is FnReferenceCallNode)
            {
                ScriptValue referenceCallResult =  FnReferenceCall((FnReferenceCallNode)fnCall.Reference, interpretScope, left,
                    leftScope, accessLevel, isStaticAccess).Ref;
                if (referenceCallResult.IsFunction)
                {
                    var fn = (ScriptFns)referenceCallResult.Value;
                    var callableInfo = fn.FindCallableInfo(interpretScope, fnCall.Args, fnCall.KeyArgs);
                    return new ValueReference(fn.Call(null, callableInfo));
                }
                else if (referenceCallResult.IsClass)
                {
                    var _class = (ScriptClass)referenceCallResult.Value;
                    return new ValueReference(_class.Call(interpretScope, fnCall.Args, fnCall.KeyArgs));
                }

                throw new SystemException($"Unexpected function call");
            }

            if (!(fnCall.Reference is VariableReferenceNode))
                throw new SystemException($"Unexpected function call");

            string fnName = ((VariableReferenceNode)fnCall.Reference).Name;

            if (leftScope.TryGetValue(fnName, out ScriptType scriptType))
            {
                if (scriptType is ScriptFns)
                {
                    var fn = ((ScriptFns)scriptType);
                    var callableInfo = fn.FindCallableInfo(interpretScope, fnCall.Args, fnCall.KeyArgs);
                    if ((int)accessLevel < (int)callableInfo.Item1.Level)
                        throw new SystemException($"Cannot access {callableInfo.Item1.Level} {fn.Name}");

                    return new ValueReference(fn.Call(isStaticAccess ? null : (ScriptObject)left.Value, callableInfo));
                }
                else if (scriptType is ScriptClass)
                {
                    var _class = ((ScriptClass)scriptType);
                    if ((int)accessLevel < (int)_class.Level)
                        throw new SystemException($"Cannot access {_class.Level} {_class.Name}");
                    return new ValueReference(((ScriptClass)scriptType).Call(interpretScope, fnCall.Args, fnCall.KeyArgs));
                }
                else if (scriptType is ScriptVariable)
                {
                    if (!(fnCall.Reference is VariableReferenceNode))
                        throw new SystemException($"{fnName} is not callable");

                    ScriptValue value = VariableReference((VariableReferenceNode)fnCall.Reference,
                        left, leftScope, accessLevel, isStaticAccess).Ref;

                    if (value.IsFunction)
                    {
                        var fn = (ScriptFns)value.Value;
                        var callableInfo = fn.FindCallableInfo(interpretScope, fnCall.Args, fnCall.KeyArgs);
                        if ((int)accessLevel < (int)callableInfo.Item1.Level)
                            throw new SystemException($"Cannot access {callableInfo.Item1.Level} {fn.Name}");

                        return new ValueReference(fn.Call(isStaticAccess ? null : (ScriptObject)left.Value, callableInfo));
                    }
                    else if (value.IsClass)
                    {
                        var _class = (ScriptClass)value.Value;
                        if ((int)accessLevel < (int)_class.Level)
                            throw new SystemException($"Cannot access {_class.Level} {_class.Name}");
                        return new ValueReference(_class.Call(interpretScope, fnCall.Args, fnCall.KeyArgs));
                    }
                    throw new SystemException($"{fnName} is not callable");
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

        public static bool IsExpressionNode(AstNode node)
        {
            Type nodeType = node.GetType();
            return nodeType == typeof(ExpressionNode) || nodeType == typeof(IntNode) ||
                nodeType == typeof(FloatNode) || nodeType == typeof(InterpolatedString) ||
                nodeType == typeof(UnaryNode) || nodeType == typeof(StringNode) || nodeType == typeof(VariableAssignmentNode) ||
                nodeType == typeof(VariableReferenceNode) || nodeType == typeof(FnReferenceCallNode) ||
                nodeType == typeof(TernaryNode);
        }

        public static bool IsStatementNode(AstNode node)
        {
            Type nodeType = node.GetType();
            return nodeType == typeof(ForNode) || nodeType == typeof(WhileNode) ||
                nodeType == typeof(SwitchNode) || nodeType == typeof(IfNode) || nodeType == typeof(TryCatchNode) ||
                nodeType == typeof(ImportNode) || nodeType == typeof(ThrowNode) || nodeType == typeof(VariableDefinitionNode) ||
                nodeType == typeof(FnDefineNode) || nodeType == typeof(ClassDefineNode);
        }

        public static void InterpretChild(ScriptScope interpretScope, AstNode node)
        {
            if (IsExpressionNode(node))
                InterpretExpression(interpretScope, node);
            else if (IsStatementNode(node))
                InterpretStatement(interpretScope, node);
            else
                throw new SystemException("Unexcepted child: " + node.NodeName);
        }

        /// <summary>
        /// </summary>
        /// <param name="interpretScope"></param>
        /// <param name="node"></param>
        /// <returns>
        /// Empty = Normal Statement<br/>
        /// Empty AND Ref.IsBreak = Break<br/>
        /// Empty AND NOT Ref.IsContinue = Continue<br/>
        /// Empty AND NOT Ref.IsBreak AND NOT Ref.IsContinue = return value
        /// </returns>
        public static ValueReference InterpretStatement(ScriptScope interpretScope, AstNode node)
        {
            if (node is TryCatchNode)
            {
                var realNode = (TryCatchNode)node;

                var tryScope = new ScriptScope(null, interpretScope);

                Exception exception = null;

                try
                {
                    foreach (var item in realNode.TryBranch)
                    {
                        if (item is ReturnNode)
                        {
                            var returnNode = (ReturnNode)item;

                            if (returnNode.Value != null)
                            {
                                var value = InterpretExpression(tryScope, returnNode.Value);

                                return value;
                            }
                            return new ValueReference(ScriptValue.Null);
                        }
                        else if (item is BreakNode)
                        {
                            return new ValueReference(ScriptValue.Break);
                        }
                        else if (item is ContinueNode)
                        {
                            return new ValueReference(ScriptValue.Continue);
                        }
                        else if (IsStatementNode(item))
                        {
                            var statementResult = InterpretStatement(tryScope, item);
                            if (!statementResult.IsEmpty)
                                return statementResult;
                        }
                        else
                        {
                            InterpretChild(tryScope, item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;

                }

                if (exception != null && realNode.CatchBranch != null)
                {
                    var catchScope = new ScriptScope(null, interpretScope);
                    foreach (var item in realNode.CatchBranch)
                    {
                        if (item is ReturnNode)
                        {
                            var returnNode = (ReturnNode)item;

                            if (returnNode.Value != null)
                            {
                                var value = InterpretExpression(catchScope, returnNode.Value);

                                return value;
                            }
                            return new ValueReference(ScriptValue.Null);
                        }
                        else if (item is BreakNode)
                        {
                            return new ValueReference(ScriptValue.Break);
                        }
                        else if (item is ContinueNode)
                        {
                            return new ValueReference(ScriptValue.Continue);
                        }
                        else if (IsStatementNode(item))
                        {
                            var statementResult = InterpretStatement(catchScope, item);
                            if (!statementResult.IsEmpty)
                                return statementResult;
                        }
                        else
                        {
                            InterpretChild(catchScope, item);
                        }
                    }
                }

                if (realNode.FinallyBranch != null)
                {
                    var finallyScope = new ScriptScope(null, interpretScope);
                    foreach (var item in realNode.FinallyBranch)
                    {
                        if (item is ReturnNode)
                        {
                            var returnNode = (ReturnNode)item;

                            if (returnNode.Value != null)
                            {
                                var value = InterpretExpression(finallyScope, returnNode.Value);
                                if (exception != null && realNode.CatchBranch == null)
                                    throw exception;
                                return value;
                            }
                            return new ValueReference(ScriptValue.Null);
                        }
                        else if (item is BreakNode)
                        {
                            if (exception != null && realNode.CatchBranch == null)
                                throw exception;
                            return new ValueReference(ScriptValue.Break);
                        }
                        else if (item is ContinueNode)
                        {
                            if (exception != null && realNode.CatchBranch == null)
                                throw exception;
                            return new ValueReference(ScriptValue.Continue);
                        }
                        else if (IsStatementNode(item))
                        {
                            var statementResult = InterpretStatement(finallyScope, item);
                            if (!statementResult.IsEmpty)
                                return statementResult;
                        }
                        else
                        {
                            InterpretChild(finallyScope, item);
                        }
                    }
                }

                if (exception != null && realNode.CatchBranch == null)
                    throw exception;

                return ValueReference.Empty;
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
                    else if (item is BreakNode)
                    {
                        return new ValueReference(ScriptValue.Break);
                    }
                    else if (item is ContinueNode)
                    {
                        return new ValueReference(ScriptValue.Continue);
                    }
                    else if (IsStatementNode(item))
                    {
                        var statementResult = InterpretStatement(scope, item);
                        if (!statementResult.IsEmpty)
                            return statementResult;
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
                                else if (IsStatementNode(item))
                                {
                                    var statementResult = InterpretStatement(scope, item);
                                    if (!statementResult.IsEmpty)
                                        return statementResult;
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
                    else if (IsStatementNode(item))
                    {
                        var statementResult = InterpretStatement(defaultScope, item);
                        if (!statementResult.IsEmpty)
                            return statementResult;
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
                        if (bodyNode is BreakNode)
                        {
                            hasBreak = true;
                            break;
                        }
                        else if (bodyNode is ContinueNode)
                        {
                            break;
                        }
                        else if (bodyNode is ReturnNode)
                        {
                            var returnNode = (ReturnNode)bodyNode;

                            if (returnNode.Value != null)
                            {
                                var value = InterpretExpression(scope, returnNode.Value);

                                return value;
                            }

                            return new ValueReference(ScriptValue.Null);
                        }
                        else if (IsStatementNode(bodyNode))
                        {
                            var statementResult = InterpretStatement(scope, bodyNode);
                            if (!statementResult.IsEmpty)
                            {
                                if (statementResult.Ref.IsBreak)
                                {
                                    hasBreak = true;
                                    break;
                                }
                                else if (statementResult.Ref.IsContinue)
                                {
                                    break;
                                }
                                return statementResult;
                            }
                        }
                        else
                        {
                            InterpretChild(scope, bodyNode);
                        }
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

                //var enumeratorInfo = ((ScriptFns)getEnumerator).GetFnInfo(interpretScope, new Dictionary<string, AstNode>());
                var enumeratorInfo = ((ScriptFns)getEnumerator).FindCallableInfo();
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

                    foreach (var bodyNode in realNode.Body)
                    {
                        if (bodyNode is BreakNode)
                        {
                            hasBreak = true;
                            break;
                        }
                        else if (bodyNode is ContinueNode)
                        {
                            break;
                        }
                        else if (bodyNode is ReturnNode)
                        {
                            var returnNode = (ReturnNode)bodyNode;

                            if (returnNode.Value != null)
                            {
                                var value = InterpretExpression(scope, returnNode.Value);

                                return value;
                            }

                            return new ValueReference(ScriptValue.Null);
                        }
                        else if (IsStatementNode(bodyNode))
                        {
                            var statementResult = InterpretStatement(scope, bodyNode);
                            if (!statementResult.IsEmpty)
                            {
                                if (statementResult.Ref.IsBreak)
                                {
                                    hasBreak = true;
                                    break;
                                }
                                else if (statementResult.Ref.IsContinue)
                                {
                                    break;
                                }
                                return statementResult;
                            }
                        }
                        else
                        {
                            InterpretChild(scope, bodyNode);
                        }
                    }

                    if (hasBreak)
                        break;
                }

                //this.currentScope = this.currentScope.Parent;
                return ValueReference.Empty;
            }
            else if (node is ImportNode)
            {
                ImportFile(interpretScope, node);
                return ValueReference.Empty;
            }
            else if (node is ThrowNode)
            {
                var realNode = (ThrowNode)node;
                var result = InterpretExpression(interpretScope, realNode.Value);
                if (!result.Ref.IsObject)
                    throw new SystemException($"{result.Ref} is not exception type");
                object exception = ((ScriptObject)result.Ref.Value).BuildInObject;
                if (!(exception is Exception))
                    throw new SystemException($"{result.Ref} is not exception type");
                throw (Exception)exception;
            }
            else if (node is VariableDefinitionNode)
            {
                VariableDefinition((VariableDefinitionNode)node, interpretScope);
                return ValueReference.Empty;
            }
            /*else if (node is VariableAssignmentNode)
            {
                VariableAssignment((VariableAssignmentNode)node, interpretScope);
                return ValueReference.Empty;
            }*/
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
                    HashSet<ScriptClass> dataTypes = null;
                    ScriptValue defaultValue = null;
                    if (param.DataType != null)
                    {
                        ScriptValue type = InterpretExpression(interpretScope, param.DataType).Ref;
                        if (!type.IsClass)
                            throw new SystemException($"Only class is accepted in {realNode.Name}.{param.Name}");
                        dataTypes = new HashSet<ScriptClass> { (ScriptClass)type.Value };
                    }
                    if (param.DefaultValue != null)
                        defaultValue = InterpretExpression(interpretScope, param.DefaultValue).Ref;
                    fnParameters.Add(new FnParameter(param.Name, dataTypes, defaultValue, param.IsMultiArgs));
                }

                scriptFns.AddFn(new ScriptFn(fnParameters,
                    realNode.Body, interpretScope, realNode.IsStatic, realNode.Level), true);
                return ValueReference.Empty;
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

                List<ScriptClass> superClasses = new List<ScriptClass>();
                foreach (var item in realNode.SuperClasses)
                {
                    var superClass = InterpretExpression(interpretScope, item);
                    if (!superClass.Ref.IsClass)
                        throw new SystemException($"{superClass} cannot be inherited");
                    superClasses.Add((ScriptClass)superClass.Ref.Value);
                }

                interpretScope.Classes[realNode.Name] = new ScriptClass(realNode.Name, realNode.Body,
                    interpretScope, superClasses, realNode.IsStatic, realNode.Level);
                return ValueReference.Empty;
            }
            throw new SystemException("Unexcepted interpret statement: " + node.NodeName);
        }
        public static ValueReference InterpretExpression(ScriptScope interpretScope, AstNode node)
        {
            if (node is ExpressionNode)
            {
                var realNode = (ExpressionNode)node;

                var _operater = realNode.Operator;
                string specialAccess = "";
                if (realNode.Left is VariableReferenceNode)
                {
                    string rawRef = ((VariableReferenceNode)realNode.Left).Name;
                    if (rawRef.Equals("this") || rawRef.Equals("super"))
                        specialAccess = rawRef;
                }

                ScriptValue left = specialAccess.Length <= 0 ? InterpretExpression(interpretScope, realNode.Left).Ref :
                    new ScriptValue((ScriptObject)interpretScope.Parent.Type);

                if (_operater == ".")
                {
                    bool isStaticAccess = left.Value is ScriptClass;

                    ScriptScope leftScope = null;
                    if (specialAccess.Equals("this"))
                        leftScope = ((ScriptObject)left.Value).Scope;
                    else if (specialAccess.Equals("super"))
                        leftScope = ((ScriptObject)left.Value).ClassType.SuperClass.Scope;
                    else if (left.Value is ScriptObject)
                        leftScope = ((ScriptObject)left.Value).Scope;
                    else if (left.Value is ScriptClass)
                        leftScope = ((ScriptClass)left.Value).Scope;
                    else/* if (left.Value is ScriptFns)*/
                        throw new SystemException("Function cannot use operator '.'");

                    bool isPrivateAccess = interpretScope.ContainsScope(leftScope);
                    AccessibilityLevel accessLevel = isPrivateAccess ? AccessibilityLevel.Private : AccessibilityLevel.Public;

                    if (realNode.Right is FnReferenceCallNode)
                    {
                        return FnReferenceCall((FnReferenceCallNode)realNode.Right, interpretScope, left,
                            leftScope, accessLevel, isStaticAccess);
                    }
                    else if (realNode.Right is VariableReferenceNode)
                    {
                        return VariableReference((VariableReferenceNode)realNode.Right, left,
                            leftScope, accessLevel, isStaticAccess);
                    }
                    throw new SystemException($"Unexcepted operation {left}'.'");
                }

                ScriptValue right = null;

                if (_operater == "&&")
                {
                    if (left.IsObject && ((ScriptObject)left.Value).ClassType is ScriptBool &&
                        !(bool)((ScriptObject)left.Value).BuildInObject)
                        return new ValueReference(new ScriptValue(false));
                    right = InterpretExpression(interpretScope, realNode.Right).Ref;
                    return new ValueReference(ScriptValue.And(left, right));
                }
                else if (_operater == "||")
                {
                    if (left.IsObject && ((ScriptObject)left.Value).ClassType is ScriptBool &&
                        (bool)((ScriptObject)left.Value).BuildInObject)
                        return new ValueReference(new ScriptValue(true));
                    right = InterpretExpression(interpretScope, realNode.Right).Ref;
                    return new ValueReference(ScriptValue.Or(left, right));
                }

                right = InterpretExpression(interpretScope, realNode.Right).Ref;
                if (_operater == "+") return new ValueReference(left + right);
                else if (_operater == "-") return new ValueReference(left - right);
                else if (_operater == "*") return new ValueReference(left * right);
                else if (_operater == "/") return new ValueReference(left / right);
                else if (_operater == "%") return new ValueReference(left % right);
                else if (_operater == "==") return new ValueReference(new ScriptValue(left.Equals(right)));
                else if (_operater == "!=") return new ValueReference(new ScriptValue(!left.Equals(right)));
                else if (_operater == "<") return new ValueReference(left < right);
                else if (_operater == ">") return new ValueReference(left > right);
                else if (_operater == "<=") return new ValueReference(left <= right);
                else if (_operater == ">=") return new ValueReference(left >= right);
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
                else if (realNode.Operator == "-")
                    return new ValueReference(-value);
                else if (realNode.Operator == "*")
                    return new ValueReference(ScriptValue.OperatorSingleUnzip(value));
                else
                    throw new SystemException($"Unexpected Unary Operator: {realNode.Operator}");
            }
            else if (node is VariableReferenceNode)
            {
                return VariableReference((VariableReferenceNode)node, interpretScope);
            }
            else if (node is FnReferenceCallNode)
            {
                return FnReferenceCall((FnReferenceCallNode)node, interpretScope);
            }
            else if (node is ListNode)
            {
                var realNode = (ListNode)node;

                List<ScriptValue> values = new List<ScriptValue>();

                foreach (var value in realNode.Elements)
                {
                    ScriptValue scriptValue = InterpretExpression(interpretScope, value).Ref;
                    if (scriptValue.Value is SingleUnzipList)
                        values.AddRange(((SingleUnzipList)scriptValue.Value).Value);
                    else
                        values.Add(scriptValue);
                }

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

                    int intIndex = (int)ScriptInt.Modulo(intValue, listValue.Count);
                    return new ValueReference(() => listValue[intIndex],
                        x => listValue[intIndex] = x);
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
                    throw new SystemException("The variable cannot use indexer");
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
                    HashSet<ScriptClass> dataTypes = null;
                    ScriptValue defaultValue = null;
                    if (param.DataType != null)
                    {
                        ScriptValue type = InterpretExpression(interpretScope, param.DataType).Ref;
                        if (!type.IsClass)
                            throw new SystemException($"Only class is accepted in {realNode.Name}.{param.Name}");
                        dataTypes = new HashSet<ScriptClass> { (ScriptClass)type.Value };
                    }
                    if (param.DefaultValue != null)
                        defaultValue = InterpretExpression(interpretScope, param.DefaultValue).Ref;
                    fnParameters.Add(new FnParameter(param.Name, dataTypes, defaultValue, param.IsMultiArgs));
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
            else if (node is VariableAssignmentNode)
            {
                return VariableAssignment((VariableAssignmentNode)node, interpretScope);
            }
            throw new SystemException("Unexcepted interpret expression: " + node.NodeName);
        }
    }
}
