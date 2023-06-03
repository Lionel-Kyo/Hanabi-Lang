using HanabiLang.Interprets;
using HanabiLang.Interprets.ScriptTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    internal static class Test
    {
        public static ScriptValue TestFn(ScriptValue scriptValue)
        {
            if (scriptValue.IsFunction)
            {
                var fns = (ScriptFns)scriptValue.Value;
                fns.Call(null);
            }
            return new ScriptValue(BasicTypes.List.Create(new List<ScriptValue>() { ScriptValue.Null, new ScriptValue("Fake"), new ScriptValue(3.14) }));
        }

        public static List<List<int>> ListList()
        {
            return new List<List<int>> { new List<int> { 1, 2, 3 }, new List<int> { 4, 5, 6 } };
        }

        public static int[] PrintLn2(params int[] arg)
        {
            foreach (int line in arg)
            {
                Console.WriteLine(line);
            }
            return arg;
        }
        public static object[] PrintLn2(params object[] arg)
        {
            DateTime dt = DateTime.Now;
            object[] lines = (object[])arg;
            foreach (object line in lines)
            {
                Console.WriteLine(line);
            }
            return lines;
        }

        public static object[] PrintLn(object[] arg)
        {
            DateTime dt = DateTime.Now;
            object[] lines = (object[])arg;
            foreach (object line in lines)
            {
                Console.WriteLine(line);
            }
            return lines;
        }

        public static Dictionary<object, object> PrintDict(object arg)
        {
            Dictionary<object, object> lines = (Dictionary<object, object>)arg;
            foreach (var kv in lines)
            {
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            }
            return lines;
        }

        public static bool? Nullable(bool? fake)
        {
            bool? nul = null;
            return nul;
        }
    }

    public class Faker
    {
        private int a;
        public int B => 12345;

        public int BA { get; set; }
        public int BB { get; private set; }

        public int c;
        public const string Hi = "Hi";
        public static string Hello => "Hello";
        public Faker()
        {

        }
        public Faker(int a)
        {
            this.a = a;
        }

        public int GetA()
        {
            return this.a;
        }

        public static string Nyan() => "Nyan";
    }

    public class TestClass2
    {
        public string Message { get; private set; }
        public TestClass2(string message)
        {
            this.Message = message;
        }

        public TestClass2 Add(TestClass2 value)
        {
            return new TestClass2(this.Message + value.Message);
        }

        public override string ToString()
        {
            return this.Message;
        }
    }

    public enum ENUMMM
    {

    }
}
