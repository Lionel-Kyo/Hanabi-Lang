using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanabiLang.Parses;
using HanabiLang.Lexers;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Interprets.Exceptions;
using HanabiLang.Parses.Nodes;
using System.Threading;
using System.Xml.Linq;
using System.Reflection;
using HanabiLangLib.Parses.Nodes;
using HanabiLangLib.Interprets.ScriptTypes;

namespace HanabiLang.Interprets
{
    public class Interpreter
    {
        public ScriptScope PredefinedScope { get; private set; }
        public ScriptScope CurrentScope { get; private set; }
        private AbstractSyntaxTree ast { get; set; }
        public string Path { get; private set; }
        public static IEnumerable<string> Arguments { get; set; }

        /// <summary>
        /// Script Start or Import Script
        /// </summary>
        public Interpreter(AbstractSyntaxTree ast, ScriptScope existedScope, ScriptScope predefinedScope, string path, bool isMain)
        {
            this.ast = ast;
            this.PredefinedScope = predefinedScope;
            if (existedScope == null)
            {
                this.CurrentScope = new ScriptScope(null, this);
                this.Path = path.Replace("\\", "/");
                BuildInImports.Classes["Script"] = new ScriptScript(isMain, Arguments);
                // this.CurrentScope.Variables["Script"] = new ScriptVariable("Script", new ScriptScript(isMain, Arguments));
                this.CurrentScope.Variables.Add("object", new ScriptVariable(BasicTypes.ObjectClass.Name, BasicTypes.ObjectClass));
                // this.CurrentScope.Variables.Add("Type", new ScriptVariable(BasicTypes.TypeClass.Name, BasicTypes.TypeClass));
                // this.CurrentScope.Variables.Add("Function", new ScriptVariable(BasicTypes.FunctionClass.Name, BasicTypes.FunctionClass));
                this.CurrentScope.Variables.Add("str", new ScriptVariable(BasicTypes.Str.Name, BasicTypes.Str));
                this.CurrentScope.Variables.Add("int", new ScriptVariable(BasicTypes.Int.Name, BasicTypes.Int));
                this.CurrentScope.Variables.Add("float", new ScriptVariable(BasicTypes.Float.Name, BasicTypes.Float));
                // this.CurrentScope.Variables.Add("decimal", new ScriptVariable(BasicTypes.Decimal.Name, BasicTypes.Decimal));
                this.CurrentScope.Variables.Add("bool", new ScriptVariable(BasicTypes.Bool.Name, BasicTypes.Bool));
                this.CurrentScope.Variables.Add("range", new ScriptVariable(BasicTypes.Range.Name, BasicTypes.Range));
                this.CurrentScope.Variables.Add("slice", new ScriptVariable(BasicTypes.Slice.Name, BasicTypes.Slice));
                this.CurrentScope.Variables.Add("List", new ScriptVariable(BasicTypes.List.Name, BasicTypes.List));
                this.CurrentScope.Variables.Add("Dict", new ScriptVariable(BasicTypes.Dict.Name, BasicTypes.Dict));
                //this.CurrentScope.Variables.Add("Iterable", new ScriptVariable(BasicTypes.Iterable.Name, BasicTypes.Iterable));
                this.CurrentScope.Variables.Add("Exception", new ScriptVariable(BasicTypes.Exception.Name, BasicTypes.Exception));
                //this.CurrentScope.Variables.Add("FnEvent", new ScriptVariable(BasicTypes.FnEvent.Name, BasicTypes.FnEvent));
                //this.CurrentScope.Variables.Add("Json", new ScriptVariable(BasicTypes.Json.Name, BasicTypes.Json));
                BasicFns.AddBasicFunctions(this.CurrentScope);
            }
            else
            {
                this.CurrentScope = existedScope;
            }
        }

        public void Interpret(bool isThrowException, bool isPrintExpression)
        {
            try
            {
                if (this.PredefinedScope != null)
                {
                    foreach (var kv in this.PredefinedScope.Variables)
                    {
                        this.CurrentScope.Variables[kv.Key] = kv.Value;
                    }
                }

                foreach (var child in this.ast.Nodes)
                {
                    InterpretChild(this.CurrentScope, child, isPrintExpression);
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
                    Console.Error.WriteLine(ExceptionToString(ex));
                    Environment.ExitCode = ex.HResult;
                }
            }
        }

        public static string ExceptionToString(Exception ex)
        {
            StringBuilder result = new StringBuilder();
            for (Exception exception = ex; exception != null; exception = exception.InnerException)
            {
                if (exception is HanibiException)
                    result.AppendLine($"Unhandled Exception ({((HanibiException)exception).ExceptionObject.ClassType.Name}): {exception.Message}");
                else
                    result.AppendLine($"Unhandled Exception ({exception.GetType().Name}): {exception.Message}");
            }
            return result.ToString();
        }

        /// <returns>Type/Assembly/null</returns>
        private static object LoadTypeWithAssebly(bool isFromAssemblyName, string text)
        {
            //bool isFromAssemblyName = !System.IO.Path.IsPathRooted(text);
            string directory = System.IO.Path.GetDirectoryName(isFromAssemblyName ? typeof(System.Random).Assembly.Location : text);
            string typeName = isFromAssemblyName ? text : System.IO.Path.GetFileNameWithoutExtension(text);

            string[] dlls = System.IO.Directory.GetFiles(directory, "*.dll", System.IO.SearchOption.TopDirectoryOnly);
            string[] splitedTypeName = typeName.Split('.');
            for (int i = 0; i < splitedTypeName.Length; i++)
            {
                string tempDllName = System.IO.Path.Combine(directory, string.Join(".", splitedTypeName.Take(splitedTypeName.Length - i)) + ".dll");
                int index = Array.FindIndex(dlls, f => f.EndsWith(tempDllName));
                if (index >= 0)
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(dlls[index]);
                        string[] splitedManifestModuleName = assembly.ManifestModule.Name.Split('.');
                        string assemblyName = string.Join(".", splitedManifestModuleName.Take(splitedManifestModuleName.Length - 1));
                        return ((object)assembly.GetType(typeName)) ?? assembly;
                    }
                    catch { }
                }
            }
            return null;
        }

        public static void ImportFile(ScriptScope interpretScope, AstNode node)
        {
            var realNode = (ImportNode)node;

            List<string> fullPaths = new List<string>();
            if (!string.IsNullOrEmpty(interpretScope?.ParentInterpreter?.Path))
                fullPaths.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(interpretScope.ParentInterpreter.Path), realNode.Path));
            fullPaths.Add(realNode.Path);
            fullPaths = fullPaths.Select(x => System.IO.Path.GetFullPath(x).Replace("\\", "/")).ToList();

            int fullPathIndex = fullPaths.FindIndex(x => System.IO.File.Exists(x));

            object loadReferenceResult = fullPathIndex >= 0 ? LoadTypeWithAssebly(false, fullPaths[fullPathIndex]) : Type.GetType(realNode.Path) ?? LoadTypeWithAssebly(true, realNode.Path);
            ScriptClass scriptClass = null;
            if (loadReferenceResult != null)
            {
                if (loadReferenceResult is Type)
                {
                    Type csType = (Type)loadReferenceResult;

                    if (!ImportedItems.Types.TryGetValue(csType, out scriptClass))
                    {
                        scriptClass = BuildInClasses.CSharpClassToScriptClass(csType, csType.Name);
                        ImportedItems.Types[csType] = scriptClass;
                    }
                }
                else if (loadReferenceResult is Assembly)
                {
                    Assembly csAssembly = (Assembly)loadReferenceResult;
                    string[] splitedManifestModuleName = csAssembly.ManifestModule.Name.Split('.');

                    if (!ImportedItems.Assemblies.TryGetValue(csAssembly, out scriptClass))
                    {
                        scriptClass = new ScriptClass(splitedManifestModuleName[splitedManifestModuleName.Length - 2], null, null, null, true, AccessibilityLevel.Public);
                        Type[] csTypes = csAssembly.DefinedTypes.Where(t => t.Attributes.HasFlag(TypeAttributes.Public)).ToArray();
                        foreach (Type csType in csTypes)
                        {
                            try
                            {
                                if (!ImportedItems.Types.TryGetValue(csType, out ScriptClass subScriptClass))
                                {
                                    subScriptClass = BuildInClasses.CSharpClassToScriptClass(csType, csType.Name);
                                    ImportedItems.Types[csType] = subScriptClass;
                                }
                                scriptClass.Scope.Variables[subScriptClass.Name] = new ScriptVariable(subScriptClass.Name, subScriptClass);
                            }
                            catch { continue; }
                        }
                        ImportedItems.Assemblies[csAssembly] = scriptClass;
                    }
                }

                if (scriptClass == null)
                    throw new SystemException($"Fail to load {realNode.Path}");

                ScriptScope scriptScope = scriptClass.Scope;
                string className = realNode.AsName ?? scriptClass.Name;

                // Import as variable
                if (realNode.Imports == null)
                {
                    if (string.IsNullOrEmpty(realNode.AsName))
                        interpretScope.Variables[className] = new ScriptVariable(className, scriptClass);
                    else
                        interpretScope.Variables[realNode.AsName] = new ScriptVariable(realNode.AsName, scriptClass);
                }
                // Import all
                else if (realNode.Imports.Count <= 0)
                {
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
                    foreach (var item in realNode.Imports)
                    {
                        string importedName = item.Item2 ?? item.Item1;
                        if (interpretScope.TryGetValue(importedName, out _))
                            throw new SystemException($"Import failed, {importedName} exists");

                        if (scriptScope.TryGetValue(item.Item1, out ScriptVariable scriptType))
                        {
                            if (scriptType.Level != AccessibilityLevel.Public)
                                throw new SystemException($"Import failed, {item.Item1} is not public");
                            interpretScope.Variables[importedName] = (ScriptVariable)scriptType;
                        }
                        else
                        {
                            throw new SystemException($"{item.Item1} is not defined in {realNode.Path}");
                        }
                    }
                }
                return;
            }

            else if (fullPathIndex >= 0)
            {
                string fullPath = fullPaths[fullPathIndex];
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullPath);
                string extension = System.IO.Path.GetExtension(realNode.Path).ToLower();

                DateTime lastWriteTimeUtc = System.IO.File.GetLastWriteTimeUtc(fullPath);
                if (extension.Equals(".json"))
                {
                    var tokens = Lexer.Tokenize(System.IO.File.ReadAllLines(fullPath));
                    var parser = new Parser(tokens);
                    var ast = parser.Parse();
                    if (ast.Nodes.Count != 1)
                        throw new SystemException("Incorrect format of json");

                    var jsonValue = InterpretJson(ast.Nodes.First());

                    interpretScope.Variables[realNode.AsName ?? fileNameWithoutExtension] = new ScriptVariable(realNode.AsName ?? fileNameWithoutExtension, null, jsonValue.Ref, true, true, AccessibilityLevel.Public);
                }
                else
                {
                    Interpreter newInterpreter = null;
                    if (!ImportedItems.Files.TryGetValue(fullPath, out Tuple<DateTime, Interpreter> scriptInfo) || lastWriteTimeUtc != scriptInfo.Item1)
                    {
                        string[] lines = Lexer.ReadScriptToLines(fullPath);
                        var tokens = Lexer.Tokenize(lines);
                        var parser = new Parser(tokens);
                        var ast = parser.Parse();
                        //Interpreter interpreter = new Interpreter(ast, fullPath, false, this.Arguments);
                        newInterpreter = new Interpreter(ast, null, interpretScope?.ParentInterpreter?.PredefinedScope, fullPath, false);
                        newInterpreter.Interpret(true, false);
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
                            interpretScope.Variables[fileNameWithoutExtension] = new ScriptVariable(
                                fileNameWithoutExtension,
                                new ScriptClass(fileNameWithoutExtension, newInterpreter.ast.Nodes,
                                    newInterpreter.CurrentScope, null, true, AccessibilityLevel.Public, true)
                            );
                        else
                            interpretScope.Variables[realNode.AsName] = new ScriptVariable(
                                realNode.AsName,
                                new ScriptClass(realNode.AsName, newInterpreter.ast.Nodes,
                                    newInterpreter.CurrentScope, null, true, AccessibilityLevel.Public, true)
                            );
                    }
                    // Import all
                    else if (realNode.Imports.Count <= 0)
                    {
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
                        foreach (var item in realNode.Imports)
                        {
                            string importedName = item.Item2 ?? item.Item1;
                            if (interpretScope.TryGetValue(importedName, out _))
                                throw new SystemException($"Import failed, {importedName} exists");

                            if (newInterpreter.CurrentScope.TryGetValue(item.Item1, out ScriptVariable scriptType))
                            {
                                if (scriptType.Level != AccessibilityLevel.Public)
                                    throw new SystemException($"Import failed, {item.Item1} is not public");
                                interpretScope.Variables[importedName] = (ScriptVariable)scriptType;
                            }
                            else
                            {
                                throw new SystemException($"{item.Item1} is not defined in {realNode.Path}");
                            }
                        }
                    }
                }
            }

            else if (BuildInImports.Classes.TryGetValue(realNode.Path, out var buildInClassImport))
            {
                // Import as variable
                if (realNode.Imports == null)
                {
                    if (string.IsNullOrEmpty(realNode.AsName))
                        interpretScope.Variables[buildInClassImport.Name] = new ScriptVariable(buildInClassImport.Name, buildInClassImport);
                    else
                        interpretScope.Variables[realNode.AsName] = new ScriptVariable(realNode.AsName, buildInClassImport);
                }
                // Import all
                else if (realNode.Imports.Count <= 0)
                {
                    foreach (var kv in buildInClassImport.Scope.Variables)
                    {
                        if (kv.Value.Level != AccessibilityLevel.Public)
                            continue;
                        interpretScope.Variables[kv.Key] = kv.Value;
                    }
                }
                // Import some
                else
                {
                    foreach (var item in realNode.Imports)
                    {
                        string importedName = item.Item2 ?? item.Item1;
                        if (interpretScope.TryGetValue(importedName, out _))
                            throw new SystemException($"Import failed, {item.Item1} exists");

                        if (buildInClassImport.Scope.TryGetValue(item.Item1, out ScriptVariable scriptType))
                        {
                            if (scriptType.Level != AccessibilityLevel.Public)
                                throw new SystemException($"Import failed, {item.Item1} is not public");
                            interpretScope.Variables[importedName] = (ScriptVariable)scriptType;
                        }
                        else
                        {
                            throw new SystemException($"{item.Item1} is not defined in {realNode.Path}");
                        }
                    }
                }
            }

            else
            {
                throw new SystemException($"{realNode.Path} is not a valid import.");
            }
        }

        private static bool TryGetValueAllScope(ScriptScope interpretScope, string name, out ScriptVariable result)
        {
            for (var scope = interpretScope; scope != null; scope = scope.Parent)
            {
                if (scope.TryGetValue(name, out result))
                {
                    return true;
                }
            }
            result = null;
            return false;
        }

        private static ValueReference VariableReference(VariableReferenceNode node, ScriptValue left,
            ScriptScope leftScope, AccessibilityLevel accessLevel, bool isStaticAccess)
        {
            if (leftScope.TryGetValue(node.Name, out ScriptVariable variable))
            {
                var fns = variable.Value?.TryFunction;
                if (fns != null && !(fns is ScriptBoundFns))
                {
                    if (left.IsObject)
                        return new ValueReference(new ScriptValue(new ScriptBoundFns(fns, left.TryObject, accessLevel)));

                    else if (left.IsClass)
                        return new ValueReference(new ScriptValue(new ScriptBoundFns(fns, null, accessLevel)));

                    return new ValueReference(new ScriptValue(fns));
                }

                if (accessLevel < variable.Level)
                    throw new SystemException($"Cannot access {variable.Level} {variable.Name}");

                return variable.GetValueReference(isStaticAccess ? null : (ScriptObject)left.Value, accessLevel);
            }
            else
            {
                throw new SystemException($"{left} does not contains {node.Name}");
            }
        }

        private static ValueReference VariableReference(VariableReferenceNode node, ScriptScope interpretScope)
        {
            if (node.Name.Equals("super"))
            {
                ScriptObject _object = null;
                if (!TryGetValueAllScope(interpretScope, "this", out ScriptVariable scriptVariable) || (_object = scriptVariable.Value.TryObject) == null)
                    throw new SystemException($"Unexpected super access");

                ScriptClass _superClass = FindSuperClassFromScope(interpretScope);

                if (_superClass == null)
                    throw new SystemException("Unexpected super access");

                _superClass = new ScriptBoundSuperClass(_superClass, _object);

                return new ValueReference(new ScriptValue(_superClass));
            }

            // Checking if the variable is defined in any scope above the current one
            if (TryGetValueAllScope(interpretScope, node.Name, out var ScriptVariable))
            {
                return ScriptVariable.GetValueReference(null, AccessibilityLevel.Public);
            }

            throw new SystemException($"{node.Name} is not defined");
        }

        internal static void VariableDefinition(VariableDefinitionNode node, ScriptScope scope, ScriptScope InterpretExpressionScope = null)
        {
            if (InterpretExpressionScope == null)
                InterpretExpressionScope = scope;

            bool isMultiDefine = node.Names.Count > 1;

            ScriptValue setValue = node.Value == null ? null : InterpretExpression(InterpretExpressionScope, node.Value).Ref;
            List<ScriptValue> setValues = null;
            int setVarCount = node.Names.Count;

            if (setValue != null && setValue.IsCatchedExpresion) 
            {
                isMultiDefine = node.Names.Count > 2;
                var catchedExpresion = ScriptCatchedExpression.AsCSharp(setValue.TryObject);

                if (node.Names.Count == 1)
                {
                    var variable = new ScriptVariable(node.Names[0], null, catchedExpresion.Item2 == null ? new ScriptValue() : new ScriptValue(BasicTypes.Exception.Create(catchedExpresion.Item2)), node.IsConstant, node.IsStatic, node.Level);
                    scope.Variables[node.Names[0]] = variable;
                    return;
                }

                if (catchedExpresion.Item2 == null)
                {
                    var _value = catchedExpresion.Item1 ?? new ScriptValue();
                    if (isMultiDefine)
                    {
                        if (ScriptIterable.TryGetIterable(_value.TryObject, out var _setValues))
                        {
                            if (_setValues is List<ScriptValue>)
                                setValues = (List<ScriptValue>)_setValues;
                            else
                                setValues = _setValues.ToList();

                            if (setValues.Count + 1 != node.Names.Count)
                            {
                                var ex = new SystemException($"Expect {setValues.Count + 1} variables to define, found {node.Names.Count} variable{(node.Names.Count > 1 ? "s" : "")}");
                                var variable = new ScriptVariable(node.Names[node.Names.Count - 1], null, new ScriptValue(BasicTypes.Exception.Create(ex)), node.IsConstant, node.IsStatic, node.Level);
                                scope.Variables[node.Names[node.Names.Count - 1]] = variable;
                                setValues = Enumerable.Range(0, node.Names.Count - 1).Select(i => new ScriptValue()).ToList();
                            }
                            else
                            {
                                var variable = new ScriptVariable(node.Names[node.Names.Count - 1], null, new ScriptValue(), node.IsConstant, node.IsStatic, node.Level);
                                scope.Variables[node.Names[node.Names.Count - 1]] = variable;
                            }
                            setVarCount = node.Names.Count - 1;
                        }
                        else
                        {
                            var ex = new SystemException($"{_value} is not iterable");
                            var variable = new ScriptVariable(node.Names[node.Names.Count - 1], null, new ScriptValue(BasicTypes.Exception.Create(ex)), node.IsConstant, node.IsStatic, node.Level);
                            scope.Variables[node.Names[node.Names.Count - 1]] = variable;
                            setValues = Enumerable.Range(0, node.Names.Count - 1).Select(i => new ScriptValue()).ToList();
                            setVarCount = node.Names.Count - 1;
                        }
                    }
                    else
                    {
                        var variable = new ScriptVariable(node.Names[node.Names.Count - 1], null, new ScriptValue(), node.IsConstant, node.IsStatic, node.Level);
                        scope.Variables[node.Names[node.Names.Count - 1]] = variable;
                        setValue = _value;
                        setVarCount = node.Names.Count - 1;
                    }
                }
                else
                {
                    var ex = catchedExpresion.Item2;
                    var variable = new ScriptVariable(node.Names[node.Names.Count - 1], null, new ScriptValue(BasicTypes.Exception.Create(ex)), node.IsConstant, node.IsStatic, node.Level);
                    scope.Variables[node.Names[node.Names.Count - 1]] = variable;
                    if (isMultiDefine)
                        setValues = Enumerable.Range(0, node.Names.Count - 1).Select(i => new ScriptValue()).ToList();
                    else
                        setValue = new ScriptValue();
                    setVarCount = node.Names.Count - 1;
                }
            }


            if (isMultiDefine && setValue != null && setValues == null)
            {
                if (!ScriptIterable.TryGetIterable(setValue.TryObject, out var _setValues))
                    throw new SystemException($"{setValue} is not iterable");

                if (_setValues is List<ScriptValue>)
                    setValues = (List<ScriptValue>)_setValues;
                else
                    setValues = _setValues.ToList();

                if (setValues.Count != node.Names.Count)
                    throw new SystemException($"Expect {node.Names.Count} variables to define, found {setValues.Count} value{(setValues.Count > 1 ? "s" : "")}");
            }

            DefinedTypes dataTypes = null;
            if (node.DataType != null)
            {
                ScriptValue type = InterpretExpression(InterpretExpressionScope, node.DataType).Ref;
                if (!type.IsDefinedTypes)
                    throw new SystemException($"Unexpected error: {node.Names}");
                dataTypes = (DefinedTypes)type.Value;
            }

            for (int i = 0; i < setVarCount; i++) 
            {
                if (scope.Variables.TryGetValue(node.Names[i], out ScriptVariable existedVariable))
                {
                    if (existedVariable.IsConstant)
                        throw new SystemException($"Cannot reassigned constant variable {node.Names}");
                }

                if (setValue != null)
                {
                    if (dataTypes != null)
                    {
                        if (!setValue.IsObject)
                            throw new SystemException($"Variable ({node.Names}) cannot be assigned with {setValue}");

                        if (!dataTypes.Value.Contains(setValue.TryObject?.ClassType))
                            throw new SystemException($"Variable ({node.Names}) cannot be assigned with {setValue}");
                    }

                    var variable = new ScriptVariable(node.Names[i], dataTypes?.Value, isMultiDefine ? setValues[i] : setValue, node.IsConstant, node.IsStatic, node.Level);
                    scope.Variables[node.Names[i]] = variable;
                }
                else
                {
                    setValue = setValue ?? new ScriptValue();

                    ScriptFns getFns = null;
                    ScriptFns setFns = null;

                    if (node.GetFn != null)
                    {
                        if (node.GetFn.Body.Count == 0)
                        {
                            getFns = new ScriptFns($"get_{string.Join("_", node.Names)}");
                            if (node.GetFn.IsStatic)
                                getFns.Fns.Add(new ScriptFn(new List<FnParameter>(), null, args => setValue, node.GetFn.IsStatic, node.GetFn.Level));
                            else
                                getFns.Fns.Add(new ScriptFn(new List<FnParameter>() { new FnParameter("this") }, null, args => setValue, node.GetFn.IsStatic, node.GetFn.Level));
                        }
                        else
                        {
                            var fn = InterpretExpression(InterpretExpressionScope, node.GetFn).Ref;
                            if (!fn.IsFunction)
                                throw new SystemException("Getter must be a function");
                            getFns = (ScriptFns)fn.Value;
                        }
                    }
                    if (node.SetFn != null)
                    {
                        if (node.GetFn.Body.Count == 0)
                        {
                            setFns = new ScriptFns($"set_{string.Join("_", node.Names)}");
                            if (node.SetFn.IsStatic)
                                setFns.Fns.Add(new ScriptFn(new List<FnParameter>() { new FnParameter("value") },
                                    null, args => setValue = args[0], node.SetFn.IsStatic, node.SetFn.Level));
                            else
                                setFns.Fns.Add(new ScriptFn(new List<FnParameter>() { new FnParameter("this"), new FnParameter("value") },
                                    null, args => setValue = args[1], node.SetFn.IsStatic, node.SetFn.Level));
                        }
                        else
                        {
                            var fn = InterpretExpression(InterpretExpressionScope, node.SetFn).Ref;
                            if (!fn.IsFunction)
                                throw new SystemException("Setter must be a function");
                            setFns = (ScriptFns)fn.Value;
                        }
                    }

                    var variable = new ScriptVariable(node.Names[i], dataTypes?.Value, getFns, setFns,
                        node.IsConstant, node.IsStatic, node.Level);
                    scope.Variables[node.Names[i]] = variable;
                }
            }
        }

        private static ValueReference VariableAssignment(VariableAssignmentNode node, ScriptScope scope)
        {
            var returnRef = InterpretExpression(scope, node.Value);
            var assignValue = returnRef.Ref;
            bool isCatchedExpresion = assignValue.IsCatchedExpresion;
            bool isMultiAssign = isCatchedExpresion ? node.References.Count > 2 : node.References.Count > 1;

            List<ScriptValue> assignValues = null;
            int assignVarCount = node.References.Count;

            if (isCatchedExpresion)
            {
                var catchedExpresion = ScriptCatchedExpression.AsCSharp(assignValue.TryObject);

                if (node.References.Count == 1)
                {
                    ValueReference toAssign = InterpretExpression(scope, node.References[0]);
                    var errorValue = catchedExpresion.Item2 == null ? new ScriptValue() : new ScriptValue(BasicTypes.Exception.Create(catchedExpresion.Item2));
                    toAssign.Ref = errorValue;
                    return toAssign;
                }

                if (catchedExpresion.Item2 == null)
                {
                    var _value = catchedExpresion.Item1 ?? new ScriptValue();
                    if (isMultiAssign)
                    {
                        if (ScriptIterable.TryGetIterable(_value.TryObject, out var _assignValues))
                        {
                            if (_assignValues is List<ScriptValue>)
                                assignValues = (List<ScriptValue>)_assignValues;
                            else
                                assignValues = _assignValues.ToList();

                            if (assignValues.Count + 1 != node.References.Count)
                            {
                                var ex = new SystemException($"Expect {assignValues.Count + 1} variables to define, found {node.References.Count} variable(s)");
                                ValueReference toAssign = InterpretExpression(scope, node.References[node.References.Count - 1]);
                                toAssign.Ref = new ScriptValue(BasicTypes.Exception.Create(ex));
                                assignValues = Enumerable.Range(0, node.References.Count - 1).Select(i => new ScriptValue()).ToList();

                                // If there is no reading, the value will not be executed.
                                returnRef = new ValueReference(() =>
                                {
                                    var returnValue = assignValues.ToList();
                                    returnValue.Add(toAssign.Ref);
                                    return new ScriptValue(returnValue);
                                }, ___ => throw new SystemException("Multiple assigned value cannot be written"));
                            }
                            else
                            {
                                ValueReference toAssign = InterpretExpression(scope, node.References[node.References.Count - 1]);
                                toAssign.Ref = new ScriptValue();

                                // If there is no reading, the value will not be executed.
                                returnRef = new ValueReference(() =>
                                {
                                    var returnValue = assignValues.ToList();
                                    returnValue.Add(toAssign.Ref);
                                    return new ScriptValue(returnValue);
                                }, ___ => throw new SystemException("Multiple assigned value cannot be written"));
                            }
                            assignVarCount = node.References.Count - 1;
                        }
                        else
                        {
                            var ex = new SystemException($"{_value} is not iterable");
                            ValueReference toAssign = InterpretExpression(scope, node.References[node.References.Count - 1]);
                            toAssign.Ref = new ScriptValue(BasicTypes.Exception.Create(ex));
                            assignValues = Enumerable.Range(0, node.References.Count - 1).Select(i => new ScriptValue()).ToList();
                            assignVarCount = node.References.Count - 1;

                            // If there is no reading, the value will not be executed.
                            returnRef = new ValueReference(() =>
                            {
                                var returnValue = assignValues.ToList();
                                returnValue.Add(toAssign.Ref);
                                return new ScriptValue(returnValue);
                            }, ___ => throw new SystemException("Multiple assigned value cannot be written"));
                        }
                    }
                    else
                    {
                        ValueReference toAssign = InterpretExpression(scope, node.References[node.References.Count - 1]);
                        toAssign.Ref = new ScriptValue();
                        assignValue = _value;

                        assignVarCount = node.References.Count - 1;

                        // If there is no reading, the value will not be executed.
                        returnRef = new ValueReference(() =>
                        {
                            var returnValue = new List<ScriptValue>() { assignValue, toAssign.Ref };
                            return new ScriptValue(returnValue);
                        }, ___ => throw new SystemException("Multiple assigned value cannot be written"));
                    }
                }
                else
                {
                    var ex = catchedExpresion.Item2;
                    ValueReference toAssign = InterpretExpression(scope, node.References[node.References.Count - 1]);
                    toAssign.Ref = new ScriptValue(BasicTypes.Exception.Create(ex));
                    if (isMultiAssign)
                    {
                        assignValues = Enumerable.Range(0, node.References.Count - 1).Select(i => new ScriptValue()).ToList();

                        // If there is no reading, the value will not be executed.
                        returnRef = new ValueReference(() =>
                        {
                            var returnValue = assignValues.ToList();
                            returnValue.Add(toAssign.Ref);
                            return new ScriptValue(returnValue);
                        }, ___ => throw new SystemException("Multiple assigned value cannot be written"));
                    }
                    else
                    {
                        assignValue = new ScriptValue();

                        // If there is no reading, the value will not be executed.
                        returnRef = new ValueReference(() =>
                        {
                            var returnValue = new List<ScriptValue>() { assignValue, toAssign.Ref };
                            return new ScriptValue(returnValue);
                        }, ___ => throw new SystemException("Multiple assigned value cannot be written"));
                    }
                    assignVarCount = node.References.Count - 1;
                }
            }

            if (isMultiAssign && assignValues == null)
            {
                if (!ScriptIterable.TryGetIterable(assignValue.TryObject, out var _assignValues))
                    throw new SystemException($"{assignValue} is not iterable");

                if (_assignValues is List<ScriptValue>)
                    assignValues = (List<ScriptValue>)_assignValues;
                else
                    assignValues = _assignValues.ToList();

                if (assignValues.Count != node.References.Count)
                    throw new SystemException($"Expect {node.References.Count} variables to assign, found {assignValues.Count} value(s)");
            }

            for (int i = 0; i < assignVarCount; i++)
            {
                ValueReference left = InterpretExpression(scope, node.References[i]);
                left.Ref = isMultiAssign ? assignValues[i] : assignValue;
            }

            return returnRef;
        }

        private static ValueReference FnReferenceCall(FnReferenceCallNode realNode, ScriptScope interpretScope)
        {
            ValueReference valueReference = InterpretExpression(interpretScope, realNode.Reference);
            ScriptValue scriptValue = valueReference.Ref;

            if (scriptValue.IsFunction)
            {
                var fn = scriptValue.TryFunction;
                //ScriptObject _object = valueReference.Left == null ? null : valueReference.Left.Ref.TryObject;
                var callableInfo = fn.FindCallableInfo(interpretScope, null, realNode.Args, realNode.KeyArgs);
                return new ValueReference(fn.Call(callableInfo));
            }
            else if (scriptValue.IsClass)
            {
                if (scriptValue.Value is ScriptBoundSuperClass)
                {
                    var _superClass = (ScriptBoundSuperClass)scriptValue.Value;
                    ScriptFns _superFns = null;
                    if (!_superClass.TryGetValue(_superClass.ConstructorName, out ScriptVariable superFns) || (_superFns = superFns.Value.TryFunction) == null)
                        throw new SystemException($"Unexpected super access");

                    var callableInfo = _superFns.FindCallableInfo(interpretScope, _superClass.BoundObject, realNode.Args, realNode.KeyArgs);
                    return new ValueReference(_superFns.Call(callableInfo));
                }
                var _class = scriptValue.TryClass;
                return new ValueReference(_class.Call(interpretScope, realNode.Args, realNode.KeyArgs));
            }
            // Null-Conditional
            else if (scriptValue.IsNull && (realNode.IsNullConditional || (realNode.Reference is ExpressionNode && ((ExpressionNode)realNode.Reference).Operator == "?.")))
            {
                return new ValueReference(scriptValue);
            }

            throw new SystemException($"{realNode.Reference.NodeName} is not a class or a function");
        }

        public static bool IsStatementNode(AstNode node)
        {
            return node is IStatementNode;
        }

        public static bool IsExpressionNode(AstNode node)
        {
            return node is IExpressionNode;
        }

        public static ValueReference InterpretChild(ScriptScope interpretScope, AstNode node, bool isPrintExpression)
        {
            if (IsExpressionNode(node))
            {
                var resultRef = InterpretExpression(interpretScope, node);
                if (isPrintExpression)
                {
                    var result = resultRef.Ref;
                    string _temp;
                    if (result.IsClassTypeOf(BasicTypes.Str))
                        Console.WriteLine(ScriptJson.StringToJsonString(result.ToString(), false));
                    else
                        Console.WriteLine((_temp = ScriptJson.StringToJsonString(result.ToString(), false)).Substring(1, _temp.Length - 2));
                }
                return resultRef;
            }
            else if (IsStatementNode(node))
            {
                return InterpretStatement(interpretScope, node);
            }
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

                ScriptObject exceptionObject = null;
                bool catched = false;
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
                            InterpretChild(tryScope, item, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is HanibiException)
                        exceptionObject = ((HanibiException)ex).ExceptionObject;
                    else
                        exceptionObject = BasicTypes.Exception.Create(ex);
                }

                if (exceptionObject != null && realNode.CatchBranch != null)
                {
                    var catchScope = new ScriptScope(null, interpretScope);
                    foreach (var item in realNode.CatchBranch)
                    {
                        DefinedTypes dataTypes = null;

                        if (item.DataType != null)
                        {
                            ScriptValue type = InterpretExpression(interpretScope, item.DataType).Ref;
                            if (!type.IsDefinedTypes)
                                throw new SystemException($"Unexpected error: {item.Name}");
                            dataTypes = (DefinedTypes)type.Value;

                            if (!dataTypes.Value.Any(ex => exceptionObject.IsTypeOrSubOf(ex)))
                                continue;
                        }

                        if (!string.IsNullOrEmpty(item.Name))
                        {
                            catchScope.Variables[item.Name] = new ScriptVariable(item.Name, null, new ScriptValue(exceptionObject), false, false, AccessibilityLevel.Public);
                        }

                        catched = true;
                        foreach (var body in item.Body)
                        {
                            if (body is ReturnNode)
                            {
                                var returnNode = (ReturnNode)body;

                                if (returnNode.Value != null)
                                {
                                    var value = InterpretExpression(catchScope, returnNode.Value);

                                    return value;
                                }
                                return new ValueReference(ScriptValue.Null);
                            }
                            else if (body is BreakNode)
                            {
                                return new ValueReference(ScriptValue.Break);
                            }
                            else if (body is ContinueNode)
                            {
                                return new ValueReference(ScriptValue.Continue);
                            }
                            else if (IsStatementNode(body))
                            {
                                var statementResult = InterpretStatement(catchScope, body);
                                if (!statementResult.IsEmpty)
                                    return statementResult;
                            }
                            else
                            {
                                InterpretChild(catchScope, body, false);
                            }
                        }
                        break;
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
                                if (exceptionObject != null && !catched)
                                    throw (Exception)exceptionObject.BuildInObject;
                                return value;
                            }
                            return new ValueReference(ScriptValue.Null);
                        }
                        else if (item is BreakNode)
                        {
                            if (exceptionObject != null && !catched)
                                throw (Exception)exceptionObject.BuildInObject;
                            return new ValueReference(ScriptValue.Break);
                        }
                        else if (item is ContinueNode)
                        {
                            if (exceptionObject != null && !catched)
                                throw (Exception)exceptionObject.BuildInObject;
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
                            InterpretChild(finallyScope, item, false);
                        }
                    }
                }

                if (exceptionObject != null && !catched)
                    throw (Exception)exceptionObject.BuildInObject;

                return ValueReference.Empty;
            }
            else if (node is IfNode)
            {
                var realNode = (IfNode)node;

                var scope = new ScriptScope(null, interpretScope);

                var compareResult = InterpretExpression(scope, realNode.Condition).Ref;
                var compareResultObj = compareResult.TryObject;
                if (compareResultObj == null)
                    throw new SystemException($"Cannot compare {compareResult}");

                if (!(compareResultObj.ClassType is ScriptBool))
                    throw new SystemException($"Cannot compare {compareResult}");

                List<AstNode> currentBranch;
                if (ScriptBool.AsCSharp(compareResultObj)) 
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
                    else if (IsExpressionNode(item))
                    {
                        InterpretChild(scope, item, false);
                    }
                    else
                    {
                        throw new SystemException($"Unexpected if body");
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
                                    InterpretChild(scope, item, false);
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
                        InterpretChild(defaultScope, item, false);
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
                            InterpretChild(scope, bodyNode, false);
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
                var location = InterpretExpression(interpretScope, realNode.Iterable).Ref;

                if (!ScriptIterable.TryGetIterable(location.TryObject, out var iter))
                    throw new SystemException("For loop running failed, variable is not iterable");

                var hasBreak = false;

                foreach (var item in iter)
                {
                    var scope = new ScriptScope(null, interpretScope);

                    if (realNode.Initializers.Count == 1)
                    {
                        scope.Variables[realNode.Initializers[0]] =
                            new ScriptVariable(realNode.Initializers[0], null, item, false, true, AccessibilityLevel.Private);
                    }
                    else
                    {
                        if (!ScriptIterable.TryGetIterable(item.TryObject, out var _initializerValues))
                            throw new SystemException($"{item} is not iterable");

                        List<ScriptValue> initializerValues;
                        if (_initializerValues is List<ScriptValue>)
                            initializerValues = (List<ScriptValue>)_initializerValues;
                        else
                            initializerValues = _initializerValues.ToList();

                        if (initializerValues.Count != realNode.Initializers.Count)
                            throw new SystemException($"Expect {realNode.Initializers.Count} initializers in for loop, found {initializerValues.Count} initializers");

                        for (int i = 0; i < initializerValues.Count; i++) 
                        {
                            scope.Variables[realNode.Initializers[i]] =
                                new ScriptVariable(realNode.Initializers[i], null, initializerValues[i], false, true, AccessibilityLevel.Private);
                        }
                    }

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
                        else if (IsExpressionNode(bodyNode))
                        {
                            InterpretExpression(scope, bodyNode);
                        }
                        else
                        {
                            throw new SystemException($"Unexpected for loop body");
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
                // Normal Function
                var realNode = (FnDefineNode)node;

                ScriptFns scriptFns = null;
                if (interpretScope.Variables.TryGetValue(realNode.Name, out ScriptVariable scriptVar))
                {
                    if (scriptVar.Value.IsFunction)
                    {
                        scriptFns = scriptVar.Value.TryFunction;
                    }
                    else
                    {
                        if (scriptVar.IsConstant)
                            throw new SystemException($"Cannot define fn {realNode.Name} to constant variable.");
                    }
                }
                if (scriptFns == null)
                {
                    scriptFns = new ScriptFns(realNode.Name);
                    scriptVar = new ScriptVariable(realNode.Name, scriptFns, AccessibilityLevel.Public);
                    interpretScope.Variables[realNode.Name] = scriptVar;
                }

                List<FnParameter> fnParameters = new List<FnParameter>();
                foreach (var param in realNode.Parameters)
                {
                    DefinedTypes dataTypes = null;
                    ScriptValue defaultValue = null;
                    if (param.DataType != null)
                    {
                        ScriptValue type = InterpretExpression(interpretScope, param.DataType).Ref;
                        if (!type.IsDefinedTypes)
                            throw new SystemException($"Unexpected error: {realNode.Name}.{param.Name}");
                        dataTypes = (DefinedTypes)type.Value;
                    }
                    if (param.DefaultValue != null)
                        defaultValue = InterpretExpression(interpretScope, param.DefaultValue).Ref;
                    fnParameters.Add(new FnParameter(param.Name, dataTypes?.Value, defaultValue, param.IsMultiArgs));
                }

                scriptFns.AddFn(new ScriptFn(fnParameters,
                    realNode.Body, interpretScope, realNode.IsStatic, realNode.Level), true);
                return ValueReference.Empty;
            }
            else if (node is ClassDefineNode)
            {
                var realNode = (ClassDefineNode)node;

                List<ScriptClass> superClasses = new List<ScriptClass>();
                foreach (var item in realNode.SuperClasses)
                {
                    var superClass = InterpretExpression(interpretScope, item);
                    if (!superClass.Ref.IsClass)
                        throw new SystemException($"{superClass} cannot be inherited");
                    superClasses.Add((ScriptClass)superClass.Ref.Value);
                }

                if (interpretScope.Variables.TryGetValue(realNode.Name, out ScriptVariable scriptVar))
                {
                    if (scriptVar.IsConstant && !scriptVar.Value.IsClass)
                        throw new SystemException($"Cannot define class {realNode.Name} to constant variable.");
                }

                ScriptClass scriptClass = new ScriptClass(realNode.Name, realNode.Body, interpretScope, superClasses, realNode.IsStatic, realNode.Level);
                scriptVar = new ScriptVariable(realNode.Name, scriptClass);
                interpretScope.Variables[realNode.Name] = scriptVar;

                return ValueReference.Empty;
            }
            else if (node is EnumDefineNode)
            {
                var realNode = (EnumDefineNode)node;

                var enumClass = new ScriptClass(realNode.Name, new List<AstNode>(),
                    null, new List<ScriptClass> { BasicTypes.Enum }, false, realNode.Level);

                if (interpretScope.Variables.TryGetValue(realNode.Name, out ScriptVariable scriptVar))
                {
                    if (scriptVar.IsConstant && !scriptVar.Value.IsClass)
                        throw new SystemException($"Cannot define class {realNode.Name} to constant variable.");
                }

                scriptVar = new ScriptVariable(realNode.Name, enumClass);
                interpretScope.Variables[realNode.Name] = scriptVar;

                foreach (var kv in realNode.Members)
                {
                    var value = InterpretExpression(interpretScope, kv.Value).Ref;
                    ScriptObject obj = enumClass.Create();
                    obj.BuildInObject = Tuple.Create(kv.Key, value);
                    enumClass.Scope.Variables[kv.Key] = new ScriptVariable(kv.Key, null,
                        new ScriptValue(obj), true, true, AccessibilityLevel.Public);
                }
                return ValueReference.Empty;
            }
            throw new SystemException("Unexcepted interpret statement: " + node.NodeName);
        }

        private static ScriptObject FindThisFromScope(ScriptScope interpretScope)
        {
            ScriptScope tempScope;
            if (interpretScope.Type is ScriptObject)
                return (ScriptObject)interpretScope.Type;
            else if ((tempScope = interpretScope.GetParentScope(s => s.Type is ScriptObject)) != null)
                return (ScriptObject)tempScope.Type;

            return null;
        }

        private static ScriptClass FindSuperClassFromScope(ScriptScope interpretScope)
        {
            ScriptClass currentClass = null;
            ScriptScope _scope = interpretScope;
            while (_scope != null)
            {
                if (_scope.Type is ScriptClass)
                {
                    currentClass = (ScriptClass)_scope.Type;
                    break;
                }

                if (_scope.Type is ScriptFn)
                {
                    _scope = ((ScriptFn)_scope.Type).Scope;
                }
                else if (_scope.Type is ScriptObject)
                {
                    currentClass = ((ScriptObject)_scope.Type).ClassType;
                    break;
                }
                else
                {
                    break;
                }
            }

            if (currentClass != null)
                return currentClass.SuperClass;

            return null;
        }

        public static ValueReference InterpretJson(AstNode node)
        {
            if (node is IntNode)
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
            else if (node is UnaryNode)
            {
                var realNode = (UnaryNode)node;
                ScriptValue value = InterpretJson(realNode.Node).Ref;

                if (realNode.Operator == "+")
                    return new ValueReference(+value);
                else if (realNode.Operator == "-")
                    return new ValueReference(-value);
                else
                    throw new SystemException($"Unexpected Unary Operator: {realNode.Operator}");
            }
            else if (node is ListNode)
            {
                var realNode = (ListNode)node;

                List<ScriptValue> values = new List<ScriptValue>();

                foreach (var value in realNode.Elements)
                {
                    ScriptValue scriptValue = InterpretJson(value).Ref;
                    if (scriptValue.IsUnzipable)
                        values.AddRange(scriptValue.TryUnzipable);
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
                    var key = InterpretJson(keyValue.Item1).Ref;
                    var value = InterpretJson(keyValue.Item2).Ref;
                    keyValues[key] = value;
                }

                return new ValueReference(new ScriptValue(keyValues));
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
            throw new SystemException($"Unexpected Node: {node.NodeName}");
        }

        public static ValueReference InterpretExpression(ScriptScope interpretScope, AstNode node)
        {
            if (node is ExpressionNode)
            {
                var realNode = (ExpressionNode)node;

                var _operater = realNode.Operator;

                ValueReference leftRef = InterpretExpression(interpretScope, realNode.Left);
                ScriptValue left = leftRef.Ref;

                if (_operater == "." || _operater == "?.")
                {
                    if (left.IsNull && _operater == "?.")
                    {
                        return new ValueReference(left);
                    }
                    bool isStaticAccess = left.Value is ScriptClass;

                    ScriptScope leftScope = null;
                    AccessibilityLevel accessLevel = AccessibilityLevel.Public;
                    if (left.Value is ScriptBoundSuperClass)
                    {
                        leftScope = ((ScriptBoundSuperClass)left.Value).Scope;
                        left = new ScriptValue(((ScriptBoundSuperClass)left.Value).BoundObject);
                        if (interpretScope.ContainsScope(((ScriptObject)left.Value).ClassType.Scope))
                            accessLevel = AccessibilityLevel.Protected;
                    }
                    else if (left.Value is ScriptObject)
                    {
                        leftScope = ((ScriptObject)left.Value).Scope;
                        if (interpretScope.ContainsScope(((ScriptObject)left.Value).ClassType.Scope))
                            accessLevel = AccessibilityLevel.Private;
                    }
                    else if (left.Value is ScriptClass)
                    {
                        leftScope = ((ScriptClass)left.Value).Scope;
                        if (interpretScope.ContainsScope(leftScope))
                            accessLevel = AccessibilityLevel.Private;
                        //if (((ScriptClass)left.Value).SuperClasses
                    }
                    else/* if (left.Value is ScriptFns)*/
                    {
                        throw new SystemException("Function cannot use operator '.'");
                    }

                    if (realNode.Right is VariableReferenceNode)
                    {
                        return VariableReference((VariableReferenceNode)realNode.Right, left,
                            leftScope, accessLevel, isStaticAccess);
                    }
                    else if (realNode.Right is FnReferenceCallNode)
                    {
                        // Obsolete
                        throw new SystemException($"Unexcepted obsolete operation {left}'{_operater}'");
                    }
                    throw new SystemException($"Unexcepted operation {left}'{_operater}'");
                }

                ScriptValue right = null;

                if (_operater == "&&")
                {
                    // Short-Circuit Evaluation
                    // Left operand is false, the entire expression will always be false.
                    ScriptObject leftObj = left.TryObject;
                    if (leftObj != null && leftObj.ClassType is ScriptBool && !ScriptBool.AsCSharp(leftObj))
                        return new ValueReference(new ScriptValue(false));
                    right = InterpretExpression(interpretScope, realNode.Right).Ref;
                    return new ValueReference(ScriptValue.And(left, right));
                }
                else if (_operater == "||")
                {
                    // Short-Circuit Evaluation
                    // Left operand is true, the entire expression will always be true.
                    ScriptObject leftObj = left.TryObject;
                    if (leftObj != null && leftObj.ClassType is ScriptBool && ScriptBool.AsCSharp(leftObj))
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
                    if (scriptValue.IsUnzipable)
                        values.AddRange(scriptValue.TryUnzipable);
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
            else if (node is IndexerNode)
            {
                var realNode = (IndexerNode)node;

                var left = InterpretExpression(interpretScope, realNode.Object);

                if (realNode.IsNullConditional && left.Ref.IsNull)
                    return new ValueReference(new ScriptValue());

                ScriptObject obj = left.Ref.TryObject;
                if (obj == null)
                    throw new SystemException("Indexer can only apply in object");

                List<ScriptValue> indexes = new List<ScriptValue>();
                foreach (var index in realNode.Indexes)
                {
                    indexes.Add(InterpretExpression(interpretScope, index).Ref);
                }
                ScriptValue indexer = new ScriptValue(indexes);

                obj.ClassType.Scope.Variables.TryGetValue("__GetIndexer__", out ScriptVariable getFn);
                obj.ClassType.Scope.Variables.TryGetValue("__SetIndexer__", out ScriptVariable setFn);

                if ((getFn != null && getFn.Value.IsFunction) || (setFn != null && setFn.Value.IsFunction))
                {
                    return new ValueReference(() => getFn.Value.TryFunction.Call(obj, indexer), x => setFn.Value.TryFunction.Call(obj, indexer, x));
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
                // Lambda Function / get_fn / set_fn
                var realNode = (FnDefineNode)node;
                List<FnParameter> fnParameters = new List<FnParameter>();
                foreach (var param in realNode.Parameters)
                {
                    HashSet<ScriptClass> dataTypes = null;
                    ScriptValue defaultValue = null;
                    if (param.DataType != null)
                    {
                        ScriptValue type = InterpretExpression(interpretScope, param.DataType).Ref;
                        if (!type.IsDefinedTypes)
                            throw new SystemException($"Unexpected error: {realNode.Name}.{param.Name}");
                        dataTypes = ((DefinedTypes)type.Value).Value;
                    }
                    if (param.DefaultValue != null)
                        defaultValue = InterpretExpression(interpretScope, param.DefaultValue).Ref;
                    fnParameters.Add(new FnParameter(param.Name, dataTypes, defaultValue, param.IsMultiArgs));
                }
                var scriptFns = new ScriptFns(realNode.Name);
                scriptFns.Fns.Add(new ScriptFn(fnParameters, realNode.Body,
                                interpretScope, realNode.IsStatic, realNode.Level));

                if (scriptFns.IsLambda)
                    return new ValueReference(new ScriptValue(new ScriptBoundFns(scriptFns, null, AccessibilityLevel.Public)));
                else 
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
            else if (node is NullCoalescingNode)
            {
                var realNode = (NullCoalescingNode)node;
                var value = InterpretExpression(interpretScope, realNode.Value);

                if (value.Ref.IsNull)
                {
                    return InterpretExpression(interpretScope, realNode.Consequent);
                }
                return value;
            }
            else if (node is VariableAssignmentNode)
            {
                return VariableAssignment((VariableAssignmentNode)node, interpretScope);
            }
            else if (node is TypeNode)
            {
                var realNode = (TypeNode)node;
                HashSet<ScriptClass> types = new HashSet<ScriptClass>();
                foreach (var value in realNode.Types)
                {
                    ScriptValue scriptValue = InterpretExpression(interpretScope, value).Ref;
                    if (scriptValue.IsNull)
                        types.Add(BasicTypes.Null);
                    else if (scriptValue.IsClass)
                        types.Add(scriptValue.TryClass);
                    else
                        throw new SystemException($"{scriptValue} is not a type");
                }
                return new ValueReference(new ScriptValue(new DefinedTypes(types)));
            }
            else if (node is CatchExpressionNode)
            {
                var realNode = (CatchExpressionNode)node;
                ScriptValue result = null;
                Exception catchedException = null;
                try
                {
                    result = InterpretExpression(interpretScope, realNode.Expression).Ref;
                }
                catch (Exception _ex)
                {
                    catchedException = _ex;
                }
                if (realNode.DefaultValue != null)
                {
                    if (catchedException == null)
                        return new ValueReference(result);
                    else
                        return InterpretExpression(interpretScope, realNode.DefaultValue);
                }
                return new ValueReference(new ScriptValue(BasicTypes.CatchedExpression.Create(result, catchedException)));
            }
            throw new SystemException("Unexcepted interpret expression: " + node.NodeName);
        }
    }
}
