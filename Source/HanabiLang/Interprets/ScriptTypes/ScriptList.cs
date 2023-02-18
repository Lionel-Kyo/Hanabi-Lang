using HanabiLang.Parses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLang.Interprets.ScriptTypes
{
    class ScriptList : ScriptClass
    {
        public ScriptList() : 
            base("List", null, new ScriptScope(ScopeType.Class), false, AccessibilityLevels.Public)
        {
            this.AddObjectFn("Length", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                return new ScriptValue(((List<ScriptValue>)_this.BuildInObject).Count);
            });
            this.AddObjectFn("Add", new List<FnParameter>()
            {
                new FnParameter("list", BasicTypes.List)
            }, args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                ((List<ScriptValue>)_this.BuildInObject).Add(args[1]);
                return ScriptValue.Null;
            });
            this.AddObjectFn("GetEnumerator", new List<FnParameter>(), args =>
            {
                ScriptObject _this = (ScriptObject)args[0].Value;
                var result =  BasicTypes.Enumerator.Create();
                result.BuildInObject = (List<ScriptValue>)_this.BuildInObject;
                return new ScriptValue(result);
            });
        }

        public override ScriptObject Create() => new ScriptObject(this, new List<ScriptValue>());
        public ScriptObject Create(List<ScriptValue> value) => new ScriptObject(this, value);

        public override ScriptObject Negative(ScriptObject left)
        {
            var result = ((List<ScriptValue>)left.BuildInObject).ToList();
            result.Reverse();
            return BasicTypes.List.Create(result);
        }

        public override ScriptObject Add(ScriptObject left, ScriptObject right)
        {
            if (right.ClassType is ScriptList)
            {
                var list = ((List<ScriptValue>)left.BuildInObject);
                List<ScriptValue> result = new List<ScriptValue>((List<ScriptValue>)left.BuildInObject);
                result.AddRange(list);
                return BasicTypes.List.Create(result);
            }
            return base.Add(left, right);
        }

        public override ScriptObject Multiply(ScriptObject left, ScriptObject right)
        {
            if (right.ClassType is ScriptInt)
            {
                long value = (long)right.BuildInObject;
                List<ScriptValue> list = new List<ScriptValue>();
                for (long i = 0; i < value; i++)
                {
                    list.AddRange((List<ScriptValue>)left.BuildInObject);
                }
                return BasicTypes.List.Create(list);
            }
            return base.Add(left, right);
        }

        public override ScriptObject Equals(ScriptObject _this, ScriptObject value)
        {
            if (value.ClassType is ScriptList)
            {
                var a = (List<ScriptValue>)_this.BuildInObject;
                var b = (List<ScriptValue>)value.BuildInObject;
                if (a.Equals(b))
                    return ScriptBool.True;
                if (a.Count == b.Count)
                {
                    for (int i = 0; i < a.Count; i++)
                    {
                        if (!a[i].Equals(b[i]))
                            return ScriptBool.False;
                    }
                    return ScriptBool.True;
                }
                return ScriptBool.False;
            }
            return ScriptBool.False;
        }

        public override ScriptObject ToStr(ScriptObject _this)
        {
            StringBuilder result = new StringBuilder();
            result.Append('[');
            foreach (var item in (List<ScriptValue>)_this.BuildInObject)
            {
                if (item.IsObject && ((ScriptObject)item.Value).ClassType is ScriptStr)
                    result.Append($"\"{item}\", ");
                else
                    result.Append($"{item}, ");

            }
            if (result.Length > 1)
                result.Remove(result.Length - 2, 2);
            result.Append(']');
            return BasicTypes.Str.Create(result.ToString());
        }

        public override string ToJsonString(ScriptObject _this, int basicIndent = 2, int currentIndent = 0)
        {
            StringBuilder result = new StringBuilder();
            result.Append('[');
            if (basicIndent != 0)
            {
                result.AppendLine();
                currentIndent += 2;
            }
            int count = 0;
            foreach (var item in (List<ScriptValue>)_this.BuildInObject)
            {
                if (!item.IsObject)
                    throw new SystemException("list item contain not object");

                ScriptObject itemObject = (ScriptObject)item.Value;
                result.Append(' ', currentIndent);
                result.Append($"{itemObject.ClassType.ToJsonString(itemObject, basicIndent, currentIndent)}");

                if (count < ((List<ScriptValue>)_this.BuildInObject).Count - 1)
                {
                    result.Append(", ");
                    if (basicIndent != 0)
                        result.AppendLine();
                }
                count++;
            }
            if (basicIndent != 0)
            {
                currentIndent -= 2;
                result.Append(' ', currentIndent);
                result.AppendLine();
            }
            result.Append(' ', currentIndent);
            result.Append(']');
            return result.ToString();
        }
    }
}
