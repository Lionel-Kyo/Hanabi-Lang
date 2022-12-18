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
        private static T[] GetCsArray<T>(ScriptList scriptList, T defaultValue)
        {
            T[] result = new T[scriptList.Value.Count];
            var valueType = result.GetType().GetElementType();
            for (int i = 0; i < scriptList.Value.Count; i++)
            {

                result[i] = (T)ToCsObject(scriptList.Value[i], valueType);
            }
            return result;
        }

        private static List<T> GetCsList<T>(ScriptList scriptList, T defaultValue)
        {
            List<T> result = new List<T>(scriptList.Value.Count);
            var valueType = result.GetType().GenericTypeArguments[0];
            for (int i = 0; i < scriptList.Value.Count; i++)
            {
                result.Add((T)ToCsObject(scriptList.Value[i], valueType));
            }
            return result;
        }

        private static Dictionary<TKey,TValue> GetCsDictionary<TKey, TValue>(ScriptDict scriptDict, TKey defaultKey, TValue defaultValue)
        {
            var result = new Dictionary<TKey, TValue>();
            var keyType = result.GetType().GenericTypeArguments[0];
            var valueType = result.GetType().GenericTypeArguments[1];
            foreach (var keyValue in scriptDict.Value)
            {
                result[(TKey)ToCsObject(keyValue.Key, keyType)] = (TValue)ToCsObject(keyValue.Value, valueType);
            }
            return result;
        }

        private static dynamic GetDefaultValue(Type type)
        {
            if (type == typeof(sbyte))
                return (sbyte)0;
            else if (type == typeof(short))
                return (short)0;
            else if (type == typeof(int))
                return (int)0;
            else if (type == typeof(long))
                return (long)0;
            else if (type == typeof(byte))
                return (byte)0;
            else if (type == typeof(ushort))
                return (ushort)0;
            else if (type == typeof(uint))
                return (uint)0;
            else if (type == typeof(ulong))
                return (ulong)0;

            else if (type == typeof(string))
                return (string)"";
            else if (type == typeof(StringBuilder))
                return (StringBuilder)new StringBuilder();

            else if (type == typeof(bool))
                return (bool)false;

            else if (type == typeof(float))
                return (float)0.0f;
            else if (type == typeof(double))
                return (double)0.0d;
            else if (type == typeof(decimal))
                return (decimal)0.0m;

            throw new SystemException($"Unexpected type: {type.Name}");
        }

        private static object ToCsObject(ScriptValue value, Type csType)
        {
            if (value.IsClass || value.IsFunction)
                throw new SystemException("Class and function is not supported");
            var obj = (ScriptObject)value.Value;

            if (obj is ScriptNull)
                return null;

            if (csType == typeof(sbyte) && obj is ScriptInt)
                return (sbyte)((ScriptInt)obj).Value;
            else if (csType == typeof(short) && obj is ScriptInt)
                return (short)((ScriptInt)obj).Value;
            else if (csType == typeof(int) && obj is ScriptInt)
                return (int)((ScriptInt)obj).Value;
            else if (csType == typeof(long) && obj is ScriptInt)
                return (long)((ScriptInt)obj).Value;
            else if (csType == typeof(byte) && obj is ScriptInt)
                return (byte)((ScriptInt)obj).Value;
            else if (csType == typeof(ushort) && obj is ScriptInt)
                return (ushort)((ScriptInt)obj).Value;
            else if (csType == typeof(uint) && obj is ScriptInt)
                return (uint)((ScriptInt)obj).Value;
            else if (csType == typeof(ulong) && obj is ScriptInt)
                return (ulong)((ScriptInt)obj).Value;

            else if (csType == typeof(string) && obj is ScriptStr)
                return (string)((ScriptStr)obj).Value;
            else if (csType == typeof(StringBuilder) && obj is ScriptStr)
                return new StringBuilder(((ScriptStr)obj).Value);

            else if (csType == typeof(bool) && obj is ScriptBool)
                return (bool)((ScriptBool)obj).Value;

            else if (csType == typeof(float) && obj is ScriptFloat)
                return (float)((ScriptFloat)obj).Value;
            else if (csType == typeof(double) && obj is ScriptFloat)
                return (double)((ScriptFloat)obj).Value;
            else if (csType == typeof(decimal) && obj is ScriptFloat)
                return (decimal)((ScriptFloat)obj).Value;

            else if (csType == typeof(float) && obj is ScriptDecimal)
                return (float)((ScriptFloat)obj).Value;
            else if (csType == typeof(double) && obj is ScriptDecimal)
                return (double)((ScriptFloat)obj).Value;
            else if (csType == typeof(decimal) && obj is ScriptDecimal)
                return (decimal)((ScriptFloat)obj).Value;

            else if (csType.IsGenericType)
            {
                Type genericType = csType.GetGenericTypeDefinition();
                Type[] genericArgs = csType.GenericTypeArguments;
                if (genericType == typeof(List<>))
                {
                    return GetCsList((ScriptList)obj, GetDefaultValue(genericArgs[0]));
                }
                else if (genericType == typeof(Dictionary<,>))
                {
                    return GetCsDictionary((ScriptDict)obj, GetDefaultValue(genericArgs[0]), GetDefaultValue(genericArgs[1]));
                }
            }
            else if (csType.IsArray && obj is ScriptList)
            {
                return GetCsArray((ScriptList)obj, GetDefaultValue(csType.GetElementType()));
            }

            throw new SystemException($"Expected type: {csType.Name}");
        }

        private static ScriptValue FromCsObject(object csObj)
        {
            if (csObj == null)
                return ScriptValue.Null;
            Type csType = csObj.GetType();

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
            else if (csType == typeof(List<>))
            {
                Console.WriteLine(csType);
            }

            throw new SystemException($"Unexpected type: {csType.Name}");
        }

        public static ScriptScope FromStaticClass(Type type)
        {
            var fns = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var fnMatch = new Dictionary<string, List<BuildInFns.ScriptFnType>>();
            foreach (var fn in fns)
            {
                if (fnMatch.TryGetValue(fn.Name, out List<BuildInFns.ScriptFnType> list))
                {
                    list.Add(ToScriptFn(fn));
                }
                else
                {
                    fnMatch[fn.Name] = new List<BuildInFns.ScriptFnType>() { ToScriptFn(fn) };
                }
            }
            var newScrope = new ScriptScope(ScopeType.Class);

            foreach (var match in fnMatch)
            {
                newScrope.Functions[match.Key] = new ScriptFn(match.Key, new List<FnParameter>(), null,
                    new ScriptScope(ScopeType.Function), args =>
                    {
                        for (int i = 0; i < match.Value.Count; i++)
                        {
                            if (i == match.Value.Count -1)
                            {
                                return match.Value[i](args);
                            }
                            else
                            {
                                try
                                {
                                    return match.Value[i](args);
                                }
                                catch { }
                            }
                        }
                        throw new SystemException("");
                    });
            }

            return newScrope;
        }

        public static BuildInFns.ScriptFnType ToScriptFn(MethodInfo method)
        {
            var returnType = method.ReturnType;
            var parameters = new List<Tuple<string, Type>>();
            foreach (var parameter in method.GetParameters())
            {
                string name = parameter.Name;
                Type type = parameter.ParameterType;
                parameters.Add(Tuple.Create(name, type));
            }

            BuildInFns.ScriptFnType fn = args =>
            {
                if (args.Count != parameters.Count)
                    throw new SystemException($"Required {parameters.Count}, recevied {args.Count}");

                object[] csObjects = new object[parameters.Count];

                for (int i = 0; i < parameters.Count; i++)
                {
                    csObjects[i] = ToCsObject(args[i], parameters[i].Item2);
                }

                object returnObj = method.Invoke(null, csObjects);

                return FromCsObject(returnObj);
            };
            return fn;
        }
    }
}
