using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HanabiLang.Parses.Nodes;
using HanabiLang.Interprets.ScriptTypes;

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

                result.Add(ToCsObject(list[i], valueType), valueType);
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
                scriptList.Add(FromCsObject(arr[i]));
            }
            return new ScriptValue(scriptList);
        }

        private static ScriptValue FromCsList(object obj)
        {
            List<ScriptValue> scriptList = new List<ScriptValue>();
            dynamic list = obj;
            for (int i = 0; i < list.Count; i++)
            {
                scriptList.Add(FromCsObject(list[i]));
            }
            return new ScriptValue(scriptList);
        }

        private static ScriptValue FromCsDictionary(object obj)
        {
            Dictionary<ScriptValue, ScriptValue> scriptDict = new Dictionary<ScriptValue, ScriptValue>();
            dynamic dict = obj;
            foreach (var keyValue in dict)
            {
                scriptDict[FromCsObject(keyValue.Key)] = FromCsObject(keyValue.Value);
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
                return null;

            if (csType == typeof(sbyte) && obj.ClassType is ScriptInt)
                return (sbyte)((long)obj.BuildInObject);
            else if (csType == typeof(short) && obj.ClassType is ScriptInt)
                return (short)((long)obj.BuildInObject);
            else if (csType == typeof(int) && obj.ClassType is ScriptInt)
                return (int)((long)obj.BuildInObject);
            else if ((csType == typeof(long) || csType == typeof(object)) && obj.ClassType is ScriptInt)
                return (long)((long)obj.BuildInObject);
            else if (csType == typeof(byte) && obj.ClassType is ScriptInt)
                return (byte)((long)obj.BuildInObject);
            else if (csType == typeof(ushort) && obj.ClassType is ScriptInt)
                return (ushort)((long)obj.BuildInObject);
            else if (csType == typeof(uint) && obj.ClassType is ScriptInt)
                return (uint)((long)obj.BuildInObject);
            else if (csType == typeof(ulong) && obj.ClassType is ScriptInt)
                return (ulong)((long)obj.BuildInObject);

            else if ((csType == typeof(string) || csType == typeof(object)) && obj.ClassType is ScriptStr)
                return (string)obj.BuildInObject;
            else if (csType == typeof(StringBuilder) && obj.ClassType is ScriptStr)
                return new StringBuilder((string)obj.BuildInObject);

            else if ((csType == typeof(bool) || csType == typeof(object)) && obj.ClassType is ScriptBool)
                return (bool)obj.BuildInObject;

            else if (csType == typeof(float) && obj.ClassType is ScriptFloat)
                return (float)obj.BuildInObject;
            else if ((csType == typeof(double) || csType == typeof(object)) && obj.ClassType is ScriptFloat)
                return (double)obj.BuildInObject;
            else if (csType == typeof(decimal) && obj.ClassType is ScriptFloat)
                return (decimal)obj.BuildInObject;

            else if (csType == typeof(float) && obj.ClassType is ScriptDecimal)
                return (float)obj.BuildInObject;
            else if (csType == typeof(double) && obj.ClassType is ScriptDecimal)
                return (double)obj.BuildInObject;
            else if ((csType == typeof(decimal) || csType == typeof(object)) && obj.ClassType is ScriptDecimal)
                return (decimal)obj.BuildInObject;

            else if (csType.IsGenericType)
            {
                Type genericType = csType.GetGenericTypeDefinition();
                Type[] genericArgs = csType.GenericTypeArguments;
                if (genericType == typeof(List<>))
                {
                    return GetCsList(obj, genericArgs[0]);
                }
                else if (genericType == typeof(Dictionary<,>))
                {
                    return GetCsDictionary(obj, genericArgs[0], genericArgs[1]);
                }
            }
            else if (csType == typeof(object) && obj.ClassType is ScriptList)
            {
                return GetCsList(obj, typeof(object));
            }
            else if (csType == typeof(object) && obj.ClassType is ScriptDict)
            {
                return GetCsDictionary(obj, typeof(object), typeof(object));
            }
            else if (csType.IsArray && obj.ClassType is ScriptList)
            {
                return GetCsArray(obj, csType.GetElementType());
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

            else if (csType == typeof(string))
                return new ScriptValue((string)csObj);
            else if (csType == typeof(StringBuilder))
                return new ScriptValue((StringBuilder)csObj);

            else if (csType == typeof(bool))
                return new ScriptValue((bool)csObj);

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
                if (genericType == typeof(List<>))
                {
                    return FromCsList(csObj);
                }
                else if (genericType == typeof(Dictionary<,>))
                {
                    return FromCsDictionary(csObj);
                }
            }
            else if (csType.IsArray)
            {
                return FromCsArray(csObj);
            }

            throw new SystemException($"Unexpected type: {csType.Name}");
        }

        public static void CSharpClassToScriptClass(ScriptClass scriptClass, Type type)
        {
            CSharpClassToScriptClass(scriptClass, type, null);
        }

        public static void CSharpClassToScriptClass(ScriptClass scriptClass, Type type, object createdObject)
        {
            bool isStatic = type.IsAbstract && type.IsSealed;

            var staticFns = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
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
                var instanceFns = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var constructors = type.GetConstructors();
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

        private static ScriptClass ToScriptType(Type type)
        {
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

            else if (type == typeof(string))
                return BasicTypes.Str;
            else if (type == typeof(StringBuilder))
                return BasicTypes.Str;

            else if (type == typeof(bool))
                return BasicTypes.Bool;

            else if (type == typeof(float))
                return BasicTypes.Float;
            else if (type == typeof(double))
                return BasicTypes.Float;
            else if (type == typeof(decimal))
                return BasicTypes.Decimal;
            else if (type == typeof(object))
                return null;
            else if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(List<>))
                    return BasicTypes.List;
                else if (genericType == typeof(Dictionary<,>))
                    return BasicTypes.Dict;
            }
            else if (type.IsArray)
                return BasicTypes.List;

            throw new NotImplementedException($"Not supported datatype {type.Name}");
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
                bool isMultipleArgs = parameter.IsDefined(typeof(ParamArrayAttribute), false);
                csParameters.Add(type);
                ScriptClass scriptType = isMultipleArgs ? ToScriptType(type.GetElementType()) : ToScriptType(type);
                scriptParameters.Add(new FnParameter(name, scriptType, defaultValue, isMultipleArgs));
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

                int startCount = (isStatic || createdObject != null) ? 0 : 1;
                for (int i = startCount; i < csParameters.Count; i++)
                {
                    csObjects[i] = ToCsObject(args[i], csParameters[i]);
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
                scriptParameters.Add(new FnParameter(name, ToScriptType(type)));
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
