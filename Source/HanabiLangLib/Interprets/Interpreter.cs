﻿using System;
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
using static System.Runtime.CompilerServices.RuntimeHelpers;
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
                this.CurrentScope.Classes["Script"] = new ScriptScript(isMain, Arguments);
                this.CurrentScope.Classes.Add("object", BasicTypes.ObjectClass);
                this.CurrentScope.Classes.Add("Type", BasicTypes.TypeClass);
                this.CurrentScope.Classes.Add("Function", BasicTypes.FunctionClass);
                this.CurrentScope.Classes.Add("str", BasicTypes.Str);
                this.CurrentScope.Classes.Add("int", BasicTypes.Int);
                this.CurrentScope.Classes.Add("float", BasicTypes.Float);
                this.CurrentScope.Classes.Add("decimal", BasicTypes.Decimal);
                this.CurrentScope.Classes.Add("bool", BasicTypes.Bool);
                this.CurrentScope.Classes.Add("range", BasicTypes.Range);
                this.CurrentScope.Classes.Add("List", BasicTypes.List);
                this.CurrentScope.Classes.Add("Dict", BasicTypes.Dict);
                this.CurrentScope.Classes.Add("Iterator", BasicTypes.Iterator);
                this.CurrentScope.Classes.Add("Exception", BasicTypes.Exception);
                this.CurrentScope.Classes.Add("FnEvent", BasicTypes.FnEvent);
                this.CurrentScope.Classes.Add("Json", BasicTypes.Json);
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
                        if (assemblyName.Equals(typeName))
                        {
                            return assembly;
                        }
                        else
                        {
                            return assembly.GetType(typeName);
                        }
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
                                scriptClass.Scope.Classes[subScriptClass.Name] = subScriptClass;
                            }
                            catch { continue; }
                        }
                        ImportedItems.Assemblies[csAssembly] = scriptClass;
                    }
                }

                if (scriptClass == null)
                    throw new SystemException($"Fail to load {realNode.Path}");

                ScriptScope scriptScope = scriptClass.Scope;
                string className = string.IsNullOrEmpty(realNode.AsName) ? scriptClass.Name : realNode.AsName;

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

            if (fullPathIndex < 0)
                throw new SystemException($"File {realNode.Path} not found");

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

                if (string.IsNullOrEmpty(realNode.AsName))
                    interpretScope.Variables[fileNameWithoutExtension] = new ScriptVariable(fileNameWithoutExtension, null, jsonValue.Ref, true, true, AccessibilityLevel.Public);
                else
                    interpretScope.Variables[realNode.AsName] = new ScriptVariable(realNode.AsName, null, jsonValue.Ref, true, true, AccessibilityLevel.Public);
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
                {
                    var fns = (ScriptFns)scriptType;
                    if (left.Value is ScriptObject)
                        return new ValueReference(new ScriptValue(new ScriptBindedFns((ScriptFns)scriptType, (ScriptObject)left.Value)));
                    return new ValueReference(new ScriptValue((ScriptFns)scriptType));
                }
                else if (scriptType is ScriptClass)
                {
                    var _class = (ScriptClass)scriptType;
                    if ((int)accessLevel < (int)_class.Level)
                        throw new SystemException($"Cannot access {_class.Level} {_class.Name}");
                    return new ValueReference(new ScriptValue((ScriptClass)scriptType));
                }
                else if (scriptType is ScriptVariable)
                {
                    var variable = (ScriptVariable)scriptType;
                    if ((int)accessLevel < (int)variable.Level)
                        throw new SystemException($"Cannot access {variable.Level} {variable.Name}");

                    return variable.GetValueReference(isStaticAccess ? null : (ScriptObject)left.Value, accessLevel);
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
                ScriptObject _thisObject = FindThisFromScope(interpretScope);
                if (_thisObject == null)
                    throw new SystemException("Unexpected keyword this");
                return new ValueReference(new ScriptValue(_thisObject));
            }
            else if (node.Name.Equals("super"))
            {
                ScriptClass _superClass = FindSuperClassFromScope(interpretScope);
                if (_superClass == null)
                    throw new SystemException("Unexpected keyword suprr");
                return new ValueReference(new ScriptValue(_superClass));
            }

            // Checking if the variable is defined in any scope above the current one
            for (var scope = interpretScope; scope != null; scope = scope.Parent)
            {
                if (scope.TryGetValue(node.Name, out ScriptType scriptType))
                {
                    if (scriptType is ScriptVariable)
                    {
                        var variable = (ScriptVariable)scriptType;
                        return variable.GetValueReference(null, AccessibilityLevel.Public);
                    }
                    else if (scriptType is ScriptObject)
                    {
                        var _object = (ScriptObject)scriptType;
                        return new ValueReference(new ScriptValue(_object));
                    }
                    else if (scriptType is ScriptFns)
                    {
                        var fn = (ScriptFns)scriptType;
                        if (scope.Type is ScriptObject)
                            return new ValueReference(new ScriptValue(new ScriptBindedFns((ScriptFns)scriptType, (ScriptObject)scope.Type)));
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

        private static void VariableDefinition(VariableDefinitionNode node, ScriptScope scope)
        {
            bool isMultiDefine = node.Names.Count > 1;

            ScriptValue setValue = node.Value == null ? null : InterpretExpression(scope, node.Value).Ref;
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
                        if (ScriptIterator.TryGetIterator(_value.TryObject, out var _setValues))
                        {
                            if (_setValues is List<ScriptValue>)
                                setValues = (List<ScriptValue>)_setValues;
                            else
                                setValues = _setValues.ToList();

                            if (setValues.Count + 1 != node.Names.Count)
                            {
                                var ex = new SystemException($"Expect {setValues.Count + 1} variables to define, found {node.Names.Count} variable(s)");
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
                            var ex = new SystemException($"{_value} is not a iterator");
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
                if (!ScriptIterator.TryGetIterator(setValue.TryObject, out var _setValues))
                    throw new SystemException($"{setValue} is not a iterator");

                if (_setValues is List<ScriptValue>)
                    setValues = (List<ScriptValue>)_setValues;
                else
                    setValues = _setValues.ToList();

                if (setValues.Count != node.Names.Count)
                    throw new SystemException($"Expect {node.Names.Count} variables to define, found {setValues.Count} value(s)");
            }

            DefinedTypes dataTypes = null;
            if (node.DataType != null)
            {
                ScriptValue type = InterpretExpression(scope, node.DataType).Ref;
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
                            getFns = new ScriptFns($"get_{node.Names}");
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
                            setFns = new ScriptFns($"set_{node.Names}");
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
                        if (ScriptIterator.TryGetIterator(_value.TryObject, out var _assignValues))
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

                                var returnValue = assignValues.ToList();
                                returnValue.Add(toAssign.Ref);
                                returnRef = new ValueReference(new ScriptValue(returnValue));
                            }
                            else
                            {
                                ValueReference toAssign = InterpretExpression(scope, node.References[node.References.Count - 1]);
                                toAssign.Ref = new ScriptValue();

                                var returnValue = assignValues.ToList();
                                returnValue.Add(toAssign.Ref);
                                returnRef = new ValueReference(new ScriptValue(returnValue));
                            }
                            assignVarCount = node.References.Count - 1;
                        }
                        else
                        {
                            var ex = new SystemException($"{_value} is not a iterator");
                            ValueReference toAssign = InterpretExpression(scope, node.References[node.References.Count - 1]);
                            toAssign.Ref = new ScriptValue(BasicTypes.Exception.Create(ex));
                            assignValues = Enumerable.Range(0, node.References.Count - 1).Select(i => new ScriptValue()).ToList();
                            assignVarCount = node.References.Count - 1;

                            var returnValue = assignValues.ToList();
                            returnValue.Add(toAssign.Ref);
                            returnRef = new ValueReference(new ScriptValue(returnValue));
                        }
                    }
                    else
                    {
                        ValueReference toAssign = InterpretExpression(scope, node.References[node.References.Count - 1]);
                        toAssign.Ref = new ScriptValue();
                        assignValue = _value;

                        assignVarCount = node.References.Count - 1;

                        var returnValue = new List<ScriptValue>() { assignValue, toAssign.Ref };
                        returnRef = new ValueReference(new ScriptValue(returnValue));
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

                        var returnValue = assignValues.ToList();
                        returnValue.Add(toAssign.Ref);
                        returnRef = new ValueReference(new ScriptValue(returnValue));
                    }
                    else
                    {
                        assignValue = new ScriptValue();

                        var returnValue = new List<ScriptValue>() { assignValue, toAssign.Ref };
                        returnRef = new ValueReference(new ScriptValue(returnValue));
                    }
                    assignVarCount = node.References.Count - 1;
                }
            }

            if (isMultiAssign && assignValues == null)
            {
                if (!ScriptIterator.TryGetIterator(assignValue.TryObject, out var _assignValues))
                    throw new SystemException($"{assignValue} is not a iterator");

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
                    scriptObject.ClassType.Scope.TryGetValue(scriptObject.ClassType.ConstructorName, out ScriptType thisFns))
                {
                    var fn = (ScriptFns)thisFns;
                    var callableInfo = fn.FindCallableInfo(interpretScope, realNode.Args, realNode.KeyArgs);
                    return new ValueReference(fn.Call(scriptObject, callableInfo));
                }
                else if (specialAccess.Equals("super") &&
                    scriptObject.ClassType.SuperClass.Scope.TryGetValue(scriptObject.ClassType.SuperClass.ConstructorName, out ScriptType superFns))
                {
                    var fn = (ScriptFns)superFns;
                    var callableInfo = fn.FindCallableInfo(interpretScope, realNode.Args, realNode.KeyArgs);
                    return new ValueReference(fn.Call(scriptObject, callableInfo));
                }
                else
                {
                    throw new SystemException($"Unexpected {specialAccess}");
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
            // Null-Conditional
            else if (fnRef.IsNull && (realNode.IsNullConditional || (realNode.Reference is ExpressionNode && ((ExpressionNode)realNode.Reference).Operator == "?.")))
            {
                return new ValueReference(fnRef);
            }

            throw new SystemException($"{realNode.Reference.NodeName} is not a class or a function");
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        private static ValueReference FnReferenceCall(FnReferenceCallNode fnCall,
            ScriptScope interpretScope, ScriptValue left, ScriptScope leftScope,
            AccessibilityLevel accessLevel, bool isStaticAccess)
        {
            if (fnCall.Reference is FnReferenceCallNode)
            {
                ScriptValue referenceCallResult = FnReferenceCall((FnReferenceCallNode)fnCall.Reference, interpretScope, left,
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
                else if (referenceCallResult.IsNull && fnCall.IsNullConditional)
                {
                    return new ValueReference(referenceCallResult);
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
                    else if (value.IsNull && fnCall.IsNullConditional)
                    {
                        return new ValueReference(value);
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
                var result = InterpretExpression(interpretScope, node);
                if (isPrintExpression)
                {
                    string _temp;
                    if (result.Ref.IsClassTypeOf(BasicTypes.Str))
                        Console.WriteLine(ScriptJson.StringToJsonString(result.Ref.ToString(), false));
                    else
                        Console.WriteLine((_temp = ScriptJson.StringToJsonString(result.Ref.ToString(), false)).Substring(1, _temp.Length - 2));
                }
                return result;
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
                        InterpretChild(scope, item, false);
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
                var location = InterpretExpression(interpretScope, realNode.Iterator).Ref;

                if (!location.IsObject)
                    throw new SystemException("For loop running failed, variable is not enumerable");

                ScriptObject scriptObject = (ScriptObject)location.Value;

                if (!ScriptIterator.TryGetIterator(scriptObject, out var iter))
                    throw new SystemException("For loop running failed, variable is not enumerable");

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
                        if (!ScriptIterator.TryGetIterator(item.TryObject, out var _initializerValues))
                            new SystemException($"{item} is not a iterator");

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
                        else
                        {
                            InterpretChild(scope, bodyNode, false);
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

                if (!interpretScope.Functions.TryGetValue(realNode.Name, out ScriptFns scriptFns))
                {
                    scriptFns = new ScriptFns(realNode.Name);
                    interpretScope.Functions[realNode.Name] = scriptFns;
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

                interpretScope.Classes[realNode.Name] = new ScriptClass(realNode.Name, realNode.Body,
                    interpretScope, superClasses, realNode.IsStatic, realNode.Level);
                return ValueReference.Empty;
            }
            else if (node is EnumDefineNode)
            {
                var realNode = (EnumDefineNode)node;

                var enumClass = new ScriptClass(realNode.Name, new List<AstNode>(),
                    null, new List<ScriptClass> { BasicTypes.Enum }, false, realNode.Level);

                interpretScope.Classes[realNode.Name] = enumClass;

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
                string specialAccess = "";
                if (realNode.Left is VariableReferenceNode)
                {
                    string rawRef = ((VariableReferenceNode)realNode.Left).Name;
                    if (rawRef.Equals("this") || rawRef.Equals("super"))
                        specialAccess = rawRef;
                }

                ScriptValue left = null;
                if (specialAccess.Length <= 0)
                {
                    left = InterpretExpression(interpretScope, realNode.Left).Ref;
                }
                else if (specialAccess.Equals("this") || specialAccess.Equals("super"))
                {
                    ScriptObject _thisObject = FindThisFromScope(interpretScope);
                    if (_thisObject == null && specialAccess.Equals("super"))
                        left = new ScriptValue(FindSuperClassFromScope(interpretScope));
                    else
                        left = new ScriptValue(_thisObject);
                }
                else
                {
                    throw new SystemException($"Unexpected {specialAccess}");
                }

                if (left == null)
                    throw new SystemException($"Cannot use {specialAccess} out of object");

                if (_operater == "." || _operater == "?.")
                {
                    if (left.IsNull && _operater == "?.")
                    {
                        return new ValueReference(left);
                    }
                    bool isStaticAccess = left.Value is ScriptClass;

                    ScriptScope leftScope = null;
                    if (specialAccess.Equals("this"))
                        leftScope = ((ScriptObject)left.Value).Scope;
                    else if (specialAccess.Equals("super"))
                    {
                        if (left.IsClass)
                            leftScope = ((ScriptClass)left.Value).Scope;
                        else
                            leftScope = FindSuperClassFromScope(interpretScope).Scope;
                    }
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
                        // Obsolete
                        return FnReferenceCall((FnReferenceCallNode)realNode.Right, interpretScope, left,
                            leftScope, accessLevel, isStaticAccess);
                    }
                    else if (realNode.Right is VariableReferenceNode)
                    {
                        return VariableReference((VariableReferenceNode)realNode.Right, left,
                            leftScope, accessLevel, isStaticAccess);
                    }
                    throw new SystemException($"Unexcepted operation {left}'{_operater}'");
                }

                ScriptValue right = null;

                if (_operater == "&&")
                {
                    // Short-Circuit Evaluation
                    // Left operand is false, the entire expression will always be false.
                    if (left.IsObject && ((ScriptObject)left.Value).ClassType is ScriptBool &&
                        !(bool)((ScriptObject)left.Value).BuildInObject)
                        return new ValueReference(new ScriptValue(false));
                    right = InterpretExpression(interpretScope, realNode.Right).Ref;
                    return new ValueReference(ScriptValue.And(left, right));
                }
                else if (_operater == "||")
                {
                    // Short-Circuit Evaluation
                    // Left operand is true, the entire expression will always be true.
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
            else if (node is IndexersNode)
            {
                var realNode = (IndexersNode)node;

                var left = InterpretExpression(interpretScope, realNode.Object);

                if (realNode.IsNullConditional && left.Ref.IsNull)
                    return new ValueReference(new ScriptValue());

                ScriptObject obj = left.Ref.TryObject;
                if (obj == null)
                    throw new SystemException("Indexer can only apply in object");

                var index = InterpretExpression(interpretScope, realNode.Index);

                obj.ClassType.Scope.Functions.TryGetValue("get_[]", out ScriptFns get_fn);
                obj.ClassType.Scope.Functions.TryGetValue("set_[]", out ScriptFns set_fn);
                if (get_fn != null || set_fn != null)
                {
                    return new ValueReference(() => get_fn.Call(obj, index.Ref), x => set_fn.Call(obj, index.Ref, x));
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
                // Lambda Function
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
