using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptUnzipable : ScriptClass
    {
        public ScriptUnzipable() :
            base("Unzipable", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("value")
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                ScriptObject value = args[1].TryObject;

                if (!ScriptIterable.TryGetIterable(value, out var iter))
                    throw new SystemException("Create List failed, variable is not enumerable");

                _this.BuildInObject = iter;
                return ScriptValue.Null;
            });

            AddVariable("Iter", args =>
            {
                ScriptObject _this = args[0].TryObject;
                return new ScriptValue(BasicTypes.Iterable.Create(AsCSharp(_this)));
            }, null, false, null);

            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue($"<object: {_this.ClassType.Name}>");
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, (IEnumerable<ScriptValue>)new List<ScriptValue>());
        public ScriptObject Create(IEnumerable<ScriptValue> value) => new ScriptObject(this, value);

        public static IEnumerable<ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (IEnumerable<ScriptValue>)_this.BuildInObject;
        }
    }
}
