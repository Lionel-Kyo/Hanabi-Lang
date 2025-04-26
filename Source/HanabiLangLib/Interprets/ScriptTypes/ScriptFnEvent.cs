using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Interprets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptFnEvent : ScriptClass
    {
        public ScriptFnEvent() :
            base("FnEvent", null, isStatic: false)
        {
            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("args", (ScriptClass)null, null, true),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                var _args = ScriptList.AsCSharp(args[1].TryObject);

                var values = new HashSet<ScriptFns>();
                foreach (var arg in _args)
                {
                    var fn = arg.TryFunction;
                    if (fn == null)
                        throw new ArgumentException("FnEvent only accept function");
                    values.Add(fn);
                }
                _this.BuildInObject = values;
                return ScriptValue.Null;
            });

            this.AddVariable("Length", args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(AsCSharp(_this).Count);
            }, null, false, null);

            this.AddFunction("Add", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("function", (ScriptClass)null, null, false),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                var fn = args[1].TryFunction;
                if (fn == null)
                    throw new ArgumentException("FnEvent only can add function");
                int result = AddFn(_this, fn);
                return new ScriptValue(result);
            });

            this.AddFunction("Remove", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("function", (ScriptClass)null, null, false),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                int result;
                var fn = args[1].TryFunction;
                if (fn == null)
                {
                    var idObj = args[1].TryObject;
                    if (idObj == null || !idObj.IsTypeOrSubOf(BasicTypes.Int))
                        throw new ArgumentException("FnEvent only can remove by function or id");
                    int hashCode = ScriptInt.ValidateToInt32(ScriptInt.AsCSharp(idObj));
                    result = RemoveFn(_this, hashCode);
                }
                else 
                {
                    result = RemoveFn(_this, fn);
                }
                return new ScriptValue(result);
            });

            this.AddFunction("Call", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("args", (ScriptClass)null, null, true),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                var _args = ScriptList.AsCSharp(args[1].TryObject);
                Call(_this, false, _args.ToArray());
                return ScriptValue.Null;
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, new HashSet<ScriptFns>());

        public ScriptObject Create(IEnumerable<ScriptFns> fns) => new ScriptObject(this, new HashSet<ScriptFns>(fns));

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create($"<object: {_this.ClassType.Name}>");

        public static int Length(ScriptObject _this)
        {
            return AsCSharp(_this).Count;
        }

        public static int AddFn(ScriptObject _this, ScriptFns fn)
        {
            var fns = AsCSharp(_this);
            if (fns.Add(fn))
                return fn.GetHashCode();
            return 0;
        }

        public static int RemoveFn(ScriptObject _this, ScriptFns fn)
        {
            var fns = AsCSharp(_this);
            if (fns.Remove(fn))
                return fn.GetHashCode();
            return 0;
        }

        public static int RemoveFn(ScriptObject _this, int hashCode)
        {
            var fns = AsCSharp(_this);
            if (fns.RemoveWhere(fn => fn.GetHashCode() == hashCode) > 0)
                return hashCode;
            return 0;
        }


        public static void Call(ScriptObject _this, bool skipEachFnError, params ScriptValue[] args)
        {
            var fns = AsCSharp(_this);
            foreach (var fn in fns)
            {
                try
                {
                    fn.Call(null, args);
                }
                catch (Exception ex)
                {
                    if (!skipEachFnError)
                        throw ex;
                }
            }
        }

        public static HashSet<ScriptFns> AsCSharp(ScriptObject _this)
        {
            return (HashSet<ScriptFns>)_this.BuildInObject;
        }
    }
}
