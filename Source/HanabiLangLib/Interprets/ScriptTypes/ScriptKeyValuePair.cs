using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    public class ScriptKeyValuePair : ScriptClass
    {
        public ScriptKeyValuePair() :
            base("KeyValuePair", new List<ScriptClass> { BasicTypes.Iterable }, isStatic: false)
        {
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
                long index = ScriptInt.AsCSharp(indexObj);

                int length = 2;
                if ((index >= length) || index < 0 && index < (length * -1))
                    throw new IndexOutOfRangeException();

                long moduloValue = ScriptInt.Modulo(index, 2);
                return index == 0 ? kvValue.Key : kvValue.Value;
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, new KeyValuePair<ScriptValue, ScriptValue>());
        public ScriptObject Create(KeyValuePair<ScriptValue, ScriptValue> value) => new ScriptObject(this, value);

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptKeyValuePair)
            {
                if (_this.Equals(value))
                    return ScriptBool.True;

                var a = AsCSharp(_this);
                var b = AsCSharp(value);
                if (a.Key.Equals(b.Key) && a.Value.Equals(b.Value))
                {
                    return ScriptBool.True;
                }
                return ScriptBool.False;
            }
            return ScriptBool.False;
        }

        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            KeyValuePair<ScriptValue, ScriptValue> kvValue = AsCSharp(_this);
            return $"[{kvValue.Key}, {kvValue.Value}]";
        }

        public override ScriptObject ToStr(ScriptObject _this) => BasicTypes.Str.Create(this.ToJsonString(_this));

        public static KeyValuePair<ScriptValue, ScriptValue> AsCSharp(ScriptObject _this)
        {
            return (KeyValuePair<ScriptValue, ScriptValue>)_this.BuildInObject;
        }
    }
}
