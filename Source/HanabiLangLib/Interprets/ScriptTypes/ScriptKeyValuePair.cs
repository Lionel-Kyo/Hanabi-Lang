using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptKeyValuePair : ScriptClass
    {
        public ScriptKeyValuePair() :
            base("KeyValuePair", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
            this.InitializeOperators();

            AddVariable("Key", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return AsCSharp(_this).Key;
            }, null, false, null);

            AddVariable("Value", args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return AsCSharp(_this).Value;
            }, null, false, null);

            AddVariable("Iter", args =>
            {
                var keyValue = AsCSharp(args[0].TryObject);
                var result = BasicTypes.Iterable.Create(new List<ScriptValue>() { keyValue.Key, keyValue.Value });
                return new ScriptValue(result);
            }, null, false, null);

            this.AddFunction("__GetIndexer__", new List<FnParameter> { new FnParameter("this") , new FnParameter("indexes", BasicTypes.List) }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                var kvValue = AsCSharp(_this);

                List<ScriptValue> indexes = ScriptList.AsCSharp(args[1].TryObject);
                if (indexes.Count > 1)
                    throw new ArgumentException("Only 1 indexer is allowed for List");

                var indexObj = indexes[0].TryObject;

                if (indexObj?.ClassType == BasicTypes.Slice)
                {
                    var slice = ScriptSlice.AsCSharp(indexObj);
                    return new ScriptValue(ScriptIterable.Slice(new List<ScriptValue> { kvValue.Key, kvValue.Value }, slice.Start, slice.End, slice.Step));
                }

                if (indexObj?.ClassType != BasicTypes.Int)
                    throw new ArgumentException("Only int/slice is allowed for KeyValuePair indexer");

                long index = ScriptRange.GetModuloIndex(ScriptInt.AsCSharp(indexObj), 2);

                return index == 0 ? kvValue.Key : kvValue.Value;
            });

            this.AddFunction(TO_STR, new List<FnParameter>()
            {
                new FnParameter("this")
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue($"[{AsCSharp(_this).Key}, {AsCSharp(_this).Value}]");
            });
        }

        private void InitializeOperators()
        {
            this.AddFunction(OPEARTOR_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                return new ScriptValue(OperatorEquals(args[0], args[1]));
            });
            this.AddFunction(OPEARTOR_NOT_EQUALS, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("other"),
            }, args =>
            {
                return new ScriptValue(!OperatorEquals(args[0], args[1]));
            });
        }

        private bool OperatorEquals(ScriptValue value1, ScriptValue value2)
        {
            ScriptObject _this = value1.TryObject;
            ScriptObject _other = value2.TryObject;
            if (_other.IsTypeOrSubOf(BasicTypes.KeyValuePair))
            {
                if (object.ReferenceEquals(_this, _other))
                    return true;

                var a = AsCSharp(_this);
                var b = AsCSharp(_other);

                if (a.Key.Equals(b.Key) && a.Value.Equals(b.Value))
                    return true;
            }
            return false;
        }

        public override ScriptObject Create() => new ScriptObject(this, new KeyValuePair<ScriptValue, ScriptValue>());
        public ScriptObject Create(KeyValuePair<ScriptValue, ScriptValue> value) => new ScriptObject(this, value);

        public static KeyValuePair<ScriptValue, ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (KeyValuePair<ScriptValue, ScriptValue>)_this.BuildInObject;
        }
    }
}
