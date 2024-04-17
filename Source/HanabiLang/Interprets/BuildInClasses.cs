﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HanabiLang.Parses.Nodes;
using HanabiLang.Interprets.ScriptTypes;

// To do: Script parameter with type defined cannot be pass null => C# nullable parameter cannot pass null in script calling

namespace HanabiLang.Interprets
{
    class BuildInClasses
    {
        private static dynamic GetCsArray(ScriptObject scriptList, Type valueType)
        {
            List<ScriptValue> list = (List<ScriptValue>)scriptList.BuildInObject;
            dynamic result = Array.CreateInstance(valueType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {

                result[i] = ToCsObject(list[i], valueType);
            }
            return result;
        }

        private static dynamic GetCsList(ScriptObject scriptList, Type valueType)
        {
            List<ScriptValue> list = (List<ScriptValue>)scriptList.BuildInObject;

            Type genericListType = typeof(List<>);
            Type concreteListType = genericListType.MakeGenericType(valueType);
            dynamic result = Activator.CreateInstance(concreteListType, new object[] { });

            for (int i = 0; i < list.Count; i++)
            {

                result.Add(ToCsObject(list[i], valueType));
            }
            return result;
        }

        private static dynamic GetCsDictionary(ScriptObject scriptDict, Type keyType, Type valueType)
        {
            var dict = (Dictionary<ScriptValue, ScriptValue>)scriptDict.BuildInObject;

            Type genericDictType = typeof(Dictionary<,>);
            Type concreteDictType = genericDictType.MakeGenericType(keyType, valueType);
            dynamic result = Activator.CreateInstance(concreteDictType, new object[] { });
            foreach (var keyValue in dict)
            {
                result[ToCsObject(keyValue.Key, keyType)] = ToCsObject(keyValue.Value, valueType);
            }
            return result;
        }

        private static ScriptValue FromCsArray(object obj)
        {
            List<ScriptValue> scriptList = new List<ScriptValue>();
            dynamic arr = obj;
            for (int i = 0; i < arr.Length; i++)
            {
                scriptList.Add(FromCsObject((object)arr[i]));
            }
            return new ScriptValue(scriptList);
        }

        private static ScriptValue FromCsList(object obj)
        {
            List<ScriptValue> scriptList = new List<ScriptValue>();
            dynamic list = obj;
            for (int i = 0; i < list.Count; i++)
            {
                scriptList.Add(FromCsObject((object)list[i]));
            }
            return new ScriptValue(scriptList);
        }

        private static ScriptValue FromCsDictionary(object obj)
        {
            Dictionary<ScriptValue, ScriptValue> scriptDict = new Dictionary<ScriptValue, ScriptValue>();
            dynamic dict = obj;
            foreach (var keyValue in dict)
            {
                scriptDict[FromCsObject((object)keyValue.Key)] = FromCsObject((object)keyValue.Value);
            }
            return new ScriptValue(scriptDict);
        }

        private static dynamic ToCsObject(ScriptValue value, Type csType)
        {
            /*if (value.IsClass || value.IsFunction)
                throw new SystemException("Class and function is not supported");*/
            if (csType == typeof(ScriptValue))
                return value;

            var obj = (ScriptObject)value.Value;

            if (obj.ClassType is ScriptNull)
            {
                return null;
            }
            else if (csType.IsGenericType)
            {
                Type genericType = csType.GetGenericTypeDefinition();
                Type[] genericArgs = csType.GenericTypeArguments;
                if (genericType == typeof(Nullable<>))
                {
                    Type genericListType = typeof(Nullable<>);
                    Type concreteListType = genericListType.MakeGenericType(genericArgs[0]);
                    return Activator.CreateInstance(concreteListType, new object[] { ToCsObject(value, genericArgs[0]) });
                }
                else if (genericType == typeof(List<>))
                {
                    return GetCsList(obj, genericArgs[0]);
                }
                else if (genericType == typeof(Dictionary<,>))
                {
                    return GetCsDictionary(obj, genericArgs[0], genericArgs[1]);
                }
            }
            else if (obj.ClassType is ScriptList)
            {
                if (csType == typeof(object))
                {
                    return GetCsList(obj, typeof(object));
                }
                else if (csType.IsArray)
                {
                    return GetCsArray(obj, csType.GetElementType());
                }
            }
            else if (obj.ClassType is ScriptDict) 
            {
                if (csType == typeof(object))
                {
                    return GetCsDictionary(obj, typeof(object), typeof(object));
                }
            }
            else if (obj.ClassType is ScriptInt)
            {
                long intValue = (long)obj.BuildInObject;
                if (csType == typeof(long) || csType == typeof(object))
                {
                    return (long)intValue;
                }
                else if (csType == typeof(sbyte))
                {
                    if (intValue < sbyte.MinValue || intValue > sbyte.MaxValue)
                        throw new OverflowException($"value: {intValue}, sbyte is {sbyte.MinValue}-{sbyte.MaxValue}");
                    return (sbyte)intValue;
                }
                else if (csType == typeof(short))
                {
                    if (intValue < short.MinValue || intValue > short.MaxValue)
                        throw new OverflowException($"value: {intValue}, short is {short.MinValue}-{short.MaxValue}");
                    return (short)intValue;
                }
                else if (csType == typeof(int))
                {
                    if (intValue < int.MinValue || intValue > int.MaxValue)
                        throw new OverflowException($"value: {intValue}, int is {int.MinValue}-{int.MaxValue}");
                    return (int)intValue;
                }
                else if (csType == typeof(byte))
                {
                    if (intValue < byte.MinValue || intValue > byte.MaxValue)
                        throw new OverflowException($"value: {intValue}, byte is {byte.MinValue}-{byte.MaxValue}");
                    return (byte)intValue;
                }
                else if (csType == typeof(ushort))
                {
                    if (intValue < ushort.MinValue || intValue > ushort.MaxValue)
                        throw new OverflowException($"value: {intValue}, ushort is {ushort.MinValue}-{ushort.MaxValue}");
                    return (ushort)intValue;
                }
                else if (csType == typeof(uint))
                {
                    if (intValue < uint.MinValue || intValue > uint.MaxValue)
                        throw new OverflowException($"value: {intValue}, uint is {uint.MinValue}-{uint.MaxValue}");
                    return (uint)intValue;
                }
                else if (csType == typeof(ulong))
                {
                    if (intValue < 0)
                        throw new OverflowException($"value: {intValue}, ulong is {ulong.MinValue}-{ulong.MaxValue}");
                    return (ulong)intValue;
                }
                // Accept pass int to these types
                else if (csType == typeof(double))
                {
                    return (double)intValue;
                }
                else if (csType == typeof(float))
                {
                    return (float)intValue;
                }
                else if (csType == typeof(decimal))
                {
                    return (decimal)intValue;
                }
            }
            else if (obj.ClassType is ScriptStr)
            {
                if ((csType == typeof(string) || csType == typeof(object)))
                    return (string)obj.BuildInObject;
                else if (csType == typeof(StringBuilder))
                    return new StringBuilder((string)obj.BuildInObject);
            }
            else if (obj.ClassType is ScriptBool)
            {
                if ((csType == typeof(bool) || csType == typeof(object)))
                    return (bool)obj.BuildInObject;
            }
            else if (obj.ClassType is ScriptFloat)
            {
                double floatValue = (double)obj.BuildInObject;
                if ((csType == typeof(double) || csType == typeof(object)))
                {
                    return floatValue;
                }
                else if (csType == typeof(float))
                {
                    if (floatValue < float.MinValue || floatValue > float.MaxValue)
                        throw new OverflowException($"value: {floatValue}, float is {float.MinValue}-{float.MaxValue}");
                    return (float)floatValue;
                }
                else if (csType == typeof(decimal))
                {
                    if (floatValue < (double)decimal.MinValue || floatValue > (double)decimal.MaxValue)
                        throw new OverflowException($"value: {floatValue}, float is {float.MinValue}-{float.MaxValue}");
                    return (decimal)floatValue;
                }
            }
            else if (obj.ClassType is ScriptDecimal)
            {
                decimal decimalValue = (decimal)obj.BuildInObject;
                // decimal must smaller than float/double
                if ((csType == typeof(decimal) || csType == typeof(object)))
                    return decimalValue;
                else if (csType == typeof(float))
                    return (float)decimalValue;
                else if (csType == typeof(double))
                    return (double)decimalValue;
            }
            else if (ImportedItems.Types.ContainsValue(obj.ClassType))
            {
                return obj.BuildInObject;
            }

            throw new SystemException($"Expected type: {csType.Name}");
        }

        private static ScriptValue FromCsObject(object csObj)
        {
            if (csObj == null)
                return ScriptValue.Null;
            Type csType = csObj.GetType();

            if (csType == typeof(ScriptValue))
                return (ScriptValue)csObj;

            else if (csType == typeof(bool))
                return new ScriptValue((bool)csObj);

            else if (csType == typeof(string))
                return new ScriptValue((string)csObj);
            else if (csType == typeof(StringBuilder))
                return new ScriptValue((StringBuilder)csObj);

            if (csType == typeof(sbyte))
                return new ScriptValue((sbyte)csObj);
            else if (csType == typeof(short))
                return new ScriptValue((short)csObj);
            else if (csType == typeof(int))
                return new ScriptValue((int)csObj);
            else if (csType == typeof(long))
                return new ScriptValue((long)csObj);
            else if (csType == typeof(byte))
                return new ScriptValue((byte)csObj);
            else if (csType == typeof(ushort))
                return new ScriptValue((ushort)csObj);
            else if (csType == typeof(uint))
                return new ScriptValue((uint)csObj);
            else if (csType == typeof(ulong))
                return new ScriptValue((long)(ulong)csObj);

            else if (csType == typeof(float))
                return new ScriptValue((float)csObj);
            else if (csType == typeof(double))
                return new ScriptValue((double)csObj);
            else if (csType == typeof(decimal))
                return new ScriptValue((decimal)csObj);


            else if (csType.IsGenericType)
            {
                Type genericType = csType.GetGenericTypeDefinition();
                // Type[] genericArgs = csType.GenericTypeArguments;
                if (genericType == typeof(Nullable<>))
                {
                    return FromCsObject(csType.GenericTypeArguments[0]);
                }
                else if (genericType == typeof(List<>))
                {
                    return FromCsList(csObj);
                }
                else if (genericType == typeof(Dictionary<,>))
                {
                    return FromCsDictionary(csObj);
                }
                throw new SystemException($"Unexpected type: {csType.Name}");
            }
            else if (csType.IsArray)
            {
                return FromCsArray(csObj);
            }
            else if (ImportedItems.Types.TryGetValue(csType, out var scriptClass))
            {
                return new ScriptValue(new ScriptObject(scriptClass, csObj));
            }

            throw new SystemException($"Unexpected type: {csType.Name}");
        }

        private static ScriptClass ToScriptType(Type type, out IEnumerable<ScriptClass> acceptedTypes)
        {
            acceptedTypes = null;
            if (type == typeof(ScriptValue))
                return null;
            else if (type == typeof(sbyte))
                return BasicTypes.Int;
            else if (type == typeof(short))
                return BasicTypes.Int;
            else if (type == typeof(int))
                return BasicTypes.Int;
            else if (type == typeof(long))
                return BasicTypes.Int;
            else if (type == typeof(byte))
                return BasicTypes.Int;
            else if (type == typeof(ushort))
                return BasicTypes.Int;
            else if (type == typeof(uint))
                return BasicTypes.Int;
            else if (type == typeof(ulong))
                return BasicTypes.Int;

            else if (type == typeof(bool))
                return BasicTypes.Bool;

            else if (type == typeof(string))
            {
                acceptedTypes = new ScriptClass[] { BasicTypes.Null };
                return BasicTypes.Str;
            }
            else if (type == typeof(StringBuilder))
            {
                acceptedTypes = new ScriptClass[] { BasicTypes.Null };
                return BasicTypes.Str;
            }

            else if (type == typeof(float))
            {
                acceptedTypes = new ScriptClass[] { BasicTypes.Int, BasicTypes.Decimal };
                return BasicTypes.Float;
            }
            else if (type == typeof(double))
            {
                acceptedTypes = new ScriptClass[] { BasicTypes.Int, BasicTypes.Decimal };
                return BasicTypes.Float;
            }
            else if (type == typeof(decimal))
            {
                acceptedTypes = new ScriptClass[] { BasicTypes.Int, BasicTypes.Float };
                return BasicTypes.Decimal;
            }
            else if (type == typeof(object))
            {
                return null;
            }
            else if (type.IsGenericType)
            {
                acceptedTypes = new ScriptClass[] { BasicTypes.Null };
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(Nullable<>))
                {
                    var nullableType = ToScriptType(type.GenericTypeArguments[0], out IEnumerable<ScriptClass> _acceptedTypes);
                    if (_acceptedTypes != null)
                        acceptedTypes = acceptedTypes.Concat(_acceptedTypes);
                    return nullableType;
                }
                else if (genericType == typeof(List<>))
                    return BasicTypes.List;
                else if (genericType == typeof(Dictionary<,>))
                    return BasicTypes.Dict;

                throw new NotImplementedException($"Not supported datatype {type.Name}");
            }
            else if (type.IsArray)
            {
                acceptedTypes = new ScriptClass[] { BasicTypes.Null };
                return BasicTypes.List;
            }
            else if (ImportedItems.Types.TryGetValue(type, out var scriptClass))
            {
                acceptedTypes = new ScriptClass[] { BasicTypes.Null };
                return scriptClass;
            }

            try
            {
                return CSharpClassToScriptClass(type);
            }
            catch
            {
                throw new NotImplementedException($"Not supported datatype {type.Name}");
            }
        }

        public static double FindParametersPoint(IEnumerable<ParameterInfo> paramters)
        {
            double result = 1;
            foreach (ParameterInfo parameterInfo in paramters)
            {
                Type type = parameterInfo.ParameterType;
                if (type == typeof(sbyte))
                    result *= 4.0;
                else if (type == typeof(short))
                    result *= 3.0;
                else if (type == typeof(int))
                    result *= 1.5;
                else if (type == typeof(byte))
                    result *= 3.5;
                else if (type == typeof(ushort))
                    result *= 2.5;
                else if (type == typeof(uint))
                    result *= 1.5;
                else if (type == typeof(ulong))
                    result *= 2.0;
                else if (type == typeof(float))
                    result *= 1.5;
            }
            return result;
        }

        public static ScriptClass CSharpClassToScriptClass(Type type, string rename = null)
        {
            bool isStruct = type.IsValueType && !type.IsEnum;
            bool isEnum = type.IsValueType;
            bool isClass = type.IsClass;
            if (!isClass && !isStruct/* && !isEnum*/)
                throw new SystemException("Only C# class/struct can be imported");

            bool isStatic = type.IsAbstract && type.IsSealed;

            var scriptClass = new ScriptClass(string.IsNullOrEmpty(rename) ? type.Name : rename, null, null, null, isStatic, AccessibilityLevel.Public);
            ImportedItems.Types[type] = scriptClass;
            BuildInClasses.CSharpClassToScriptClass(scriptClass, type);
            return scriptClass;
        }

        public static void CSharpClassToScriptClass(ScriptClass scriptClass, Type type)
        {
            CSharpClassToScriptClass(scriptClass, type, null);
        }

        public static void CSharpClassToScriptClass(ScriptClass scriptClass, Type type, object createdObject)
        {
            bool isStatic = type.IsAbstract && type.IsSealed;

            var staticFns = type.GetMethods(BindingFlags.Public | BindingFlags.Static).OrderBy(x => FindParametersPoint(x.GetParameters()));
            var fields = type.GetFields(); // non get set variables
            var properties = type.GetProperties(); // get set variables

            ScriptScope classScope = scriptClass.Scope;

            foreach (var fn in staticFns)
            {
                if (!classScope.Functions.TryGetValue(fn.Name, out ScriptFns scriptFns))
                {
                    scriptFns = new ScriptFns(fn.Name);
                    classScope.Functions[fn.Name] = scriptFns;
                }
                try
                {
                    var scriptFn = ToScriptFn(fn);
                    scriptFns.Fns.Add(new ScriptFn(scriptFn.Item1, classScope, scriptFn.Item2, isStatic, AccessibilityLevel.Public));
                }
                catch (NotImplementedException) { }
            }

            foreach (var field in fields)
            {
                BuildInFns.ScriptFnType getFn = args =>
                {
                    object value = null;
                    if (field.IsStatic)
                    {
                        value = field.GetValue(null);
                    }
                    else if (createdObject != null)
                    {
                        value = field.GetValue(createdObject);
                    }
                    else
                    {
                        object _this = ((ScriptObject)args[0].Value).BuildInObject;
                        value = field.GetValue(field.IsStatic ? null : _this);
                    }
                    return FromCsObject(value);
                };

                AccessibilityLevel level = field.IsPublic ? AccessibilityLevel.Public : AccessibilityLevel.Private;
                var getFns = new ScriptFns(field.Name);
                getFns.Fns.Add(new ScriptFn(new List<FnParameter>(), null, getFn, field.IsStatic, level));

                BuildInFns.ScriptFnType setFn = args =>
                {
                    if (field.IsStatic)
                    {
                        object value = ToCsObject(args[0], field.FieldType);
                        field.SetValue(null, value);
                    }
                    else if (createdObject != null)
                    {
                        object value = ToCsObject(args[0], field.FieldType);
                        field.SetValue(createdObject, value);
                    }
                    else
                    {
                        object _this = ((ScriptObject)args[0].Value).BuildInObject;
                        object value = ToCsObject(args[1], field.FieldType);
                        field.SetValue(_this, value);
                    }
                    return ScriptValue.Null;
                };


                var setFns = new ScriptFns(field.Name);
                setFns.Fns.Add(new ScriptFn(new List<FnParameter>()
                    {
                        new FnParameter("value")
                    }, null, setFn, createdObject != null ? true : field.IsStatic, level));

                classScope.Variables[field.Name] = new ScriptVariable(field.Name, getFns, setFns, false, field.IsStatic, level);
            }

            foreach (var property in properties)
            {
                BuildInFns.ScriptFnType getFn = null;
                ScriptFns getFns = null;
                BuildInFns.ScriptFnType setFn = null;
                ScriptFns setFns = null;

                if (property.CanRead)
                {
                    getFn = args =>
                    {
                        object value = null;
                        if (property.GetMethod.IsStatic)
                        {
                            value = property.GetValue(null);
                        }
                        else if (createdObject != null)
                        {
                            value = property.GetValue(createdObject);
                        }
                        else
                        {
                            object _this = ((ScriptObject)args[0].Value).BuildInObject;
                            value = property.GetValue(_this);
                        }
                        return FromCsObject(value);
                    };
                }

                AccessibilityLevel getLevel = property.CanRead && property.GetMethod.IsPublic ? 
                    AccessibilityLevel.Public : AccessibilityLevel.Private;

                bool isGetFnStatic = true;
                if (createdObject != null)
                    isGetFnStatic = true;
                else if (property.CanRead)
                    isGetFnStatic = property.GetMethod.IsStatic;

                getFns = new ScriptFns(property.Name);
                getFns.Fns.Add(new ScriptFn(new List<FnParameter>(), null, getFn, isGetFnStatic, getLevel));

                if (property.CanWrite)
                {
                    setFn = args =>
                    {
                        if (property.SetMethod.IsStatic)
                        {
                            object value = ToCsObject(args[0], property.PropertyType);
                            property.SetValue(null, value);
                        }
                        else if (createdObject != null)
                        {
                            object value = ToCsObject(args[0], property.PropertyType);
                            property.SetValue(createdObject, value);
                        }
                        else
                        {
                            object _this = ((ScriptObject)args[0].Value).BuildInObject;
                            object value = ToCsObject(args[1], property.PropertyType);
                            property.SetValue(_this, value);
                        }
                        return ScriptValue.Null;
                    };
                }

                AccessibilityLevel setLevel = property.CanWrite && property.SetMethod.IsPublic ? 
                    AccessibilityLevel.Public : AccessibilityLevel.Private;

                bool isSetFnStatic = true;
                if (createdObject != null)
                    isSetFnStatic = true;
                else if (property.CanWrite)
                    isSetFnStatic = property.SetMethod.IsStatic;

                AccessibilityLevel overAllLevel = (int)getLevel < (int)setLevel ? getLevel : setLevel;

                bool overAllIsStatic = true;

                if (createdObject != null)
                    overAllIsStatic = true;
                else if (property.CanRead)
                    overAllIsStatic = property.GetMethod.IsStatic;
                else if (property.CanWrite)
                    overAllIsStatic = property.GetMethod.IsStatic;

                setFns = new ScriptFns(property.Name);
                setFns.Fns.Add(new ScriptFn(new List<FnParameter>()
                {
                    new FnParameter("value")
                }, null, setFn, isSetFnStatic, setLevel));

                classScope.Variables[property.Name] = new ScriptVariable(property.Name, getFns, setFns, false, overAllIsStatic, overAllLevel);
            }


            if (!isStatic)
            {
                var instanceFns = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => FindParametersPoint(x.GetParameters()));
                var constructors = type.GetConstructors().OrderBy(x => FindParametersPoint(x.GetParameters()));
                foreach (var fn in instanceFns)
                {
                    if (fn.IsSpecialName)
                        continue;
                    if (!classScope.Functions.TryGetValue(fn.Name, out ScriptFns scriptFns))
                    {
                        scriptFns = new ScriptFns(fn.Name);
                        classScope.Functions[fn.Name] = scriptFns;
                    }
                    try
                    {
                        var scriptFn = ToScriptFn(fn, createdObject);
                        scriptFns.Fns.Add(new ScriptFn(scriptFn.Item1, classScope, scriptFn.Item2, createdObject != null ? true : isStatic, AccessibilityLevel.Public));
                    }
                    catch (NotImplementedException) { }
                }

                foreach (var constructor in constructors)
                {
                    try
                    {
                        var scriptConstructor = ToScriptConstructor(scriptClass, constructor);
                        scriptClass.BuildInConstructor.Fns.Add(new ScriptFn(scriptConstructor.Item1, classScope, scriptConstructor.Item2, isStatic, AccessibilityLevel.Public));
                    }
                    catch (NotImplementedException) { }
                }
            }
        }

        public static Tuple<List<FnParameter>, BuildInFns.ScriptFnType> ToScriptFn(MethodInfo method) => ToScriptFn(method, null);

        public static Tuple<List<FnParameter>, BuildInFns.ScriptFnType> ToScriptFn(MethodInfo method, object createdObject)
        {
            var returnType = method.ReturnType;
            var isStatic = method.IsStatic;
            var csParameters = new List<Type>();
            var scriptParameters = new List<FnParameter>();
            foreach (var parameter in method.GetParameters())
            {
                string name = parameter.Name;
                Type type = parameter.ParameterType;
                ScriptValue defaultValue = parameter.HasDefaultValue ? FromCsObject(parameter.DefaultValue) : null;
                csParameters.Add(type);
                bool isMultipleArgs = parameter.IsDefined(typeof(ParamArrayAttribute), false);
                IEnumerable<ScriptClass> acceptableTypes;
                ScriptClass dataType = isMultipleArgs ? ToScriptType(type.GetElementType(), out acceptableTypes) : ToScriptType(type, out acceptableTypes);
                HashSet<ScriptClass> dataTypes = dataType == null ? null : new HashSet<ScriptClass> { dataType };
                if (dataType != null && acceptableTypes != null)
                {
                    foreach (var t in acceptableTypes)
                    {
                        dataTypes.Add(t);
                    }
                }
                scriptParameters.Add(new FnParameter(name, dataTypes, defaultValue, isMultipleArgs));
            }

            BuildInFns.ScriptFnType fn = args =>
            {
                if (isStatic || createdObject != null)
                {
                    if (args.Count != csParameters.Count)
                        throw new SystemException($"Required {csParameters.Count}, recevied {args.Count}");
                }
                else
                {
                    if (args.Count - 1 != csParameters.Count)
                        throw new SystemException($"Required {csParameters.Count}, recevied {args.Count}");
                }

                object[] csObjects = new object[csParameters.Count];

                int argsStartCount = (isStatic || createdObject != null) ? 0 : 1;
                for (int i = 0; i < csObjects.Length; i++)
                {
                    csObjects[i] = ToCsObject(args[i + argsStartCount], csParameters[i]);
                }

                object invokeObject = null;
                if (createdObject != null)
                {
                    invokeObject = createdObject;
                }
                else if (isStatic)
                {
                    invokeObject = null;
                }
                else
                {
                    invokeObject = ((ScriptObject)args[0].Value).BuildInObject;
                }

                object returnObj = method.Invoke(invokeObject, csObjects);

                return FromCsObject(returnObj);
            };
           
            return Tuple.Create(scriptParameters, fn);
        }

        public static Tuple<List<FnParameter>, BuildInFns.ScriptFnType> ToScriptConstructor(ScriptClass scriptClass, ConstructorInfo constructor)
        {
            var isStatic = constructor.IsStatic;
            var csParameters = new List<Type>();
            var scriptParameters = new List<FnParameter>();
            foreach (var parameter in constructor.GetParameters())
            {
                string name = parameter.Name;
                Type type = parameter.ParameterType;
                object defaultValue = parameter.DefaultValue;
                csParameters.Add(type);
                bool isMultipleArgs = parameter.IsDefined(typeof(ParamArrayAttribute), false);
                IEnumerable<ScriptClass> acceptableTypes;
                ScriptClass dataType = isMultipleArgs ? ToScriptType(type.GetElementType(), out acceptableTypes) : ToScriptType(type, out acceptableTypes);
                HashSet<ScriptClass> dataTypes = dataType == null ? null : new HashSet<ScriptClass> { dataType };
                if (dataType != null && acceptableTypes != null)
                {
                    foreach (var t in acceptableTypes)
                    {
                        dataTypes.Add(t);
                    }
                }
                scriptParameters.Add(new FnParameter(name, dataTypes));
            }

            BuildInFns.ScriptFnType fn = args =>
            {
                object[] csObjects = new object[csParameters.Count];

                for (int i = 0; i < csParameters.Count; i++)
                {
                    csObjects[i] = ToCsObject(args[i], csParameters[i]);
                }

                object returnObj = constructor.Invoke(csObjects);

                return new ScriptValue(new ScriptObject(scriptClass, returnObj));
            };

            return Tuple.Create(scriptParameters, fn);
        }
    }
}
