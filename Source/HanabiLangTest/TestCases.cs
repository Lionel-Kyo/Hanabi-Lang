using HanabiLang.Interprets;
using HanabiLang.Interprets.Exceptions;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Lexers;
using HanabiLang.Parses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HanabiLangTest
{
    public static class TestCases
    {
        private static string GetExceptionMsg(Exception ex)
        {
            StringBuilder result = new StringBuilder();
            for (Exception exception = ex; exception != null; exception = exception.InnerException)
            {
                result.Append(exception.Message);
                result.Append(", ");
            }
            if (result.Length > 2)
                result.Remove(result.Length - 2, 2);
            return result.ToString();
        }
        public static void Run()
        {
            var methods = typeof(TestCases).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(method => method.Name != "Run").ToArray();
            int passed = 0;
            foreach (var method in methods)
            {
                Console.Write($"{method.Name}: ");
                try
                {
                    method.Invoke(null, null);
                    Console.WriteLine("Passed");
                    passed++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed: {GetExceptionMsg(ex)}");
                }
            }

            Console.WriteLine($"\nPassed Test: {passed}/{methods.Length}\n");
        }
        private static void CheckEquals(ScriptValue a, ScriptValue b)
        {
            if (!a.Equals(b))
                throw new SystemException($"{a} != {b}");
        }

        private static Dictionary<string, ScriptValue> Interpret(string code, out Interpreter interpreter, params string[] values)
        {
            var tokens = Lexer.Tokenize(code.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
            var parser = new Parser(tokens);
            var ast = parser.Parse();

            interpreter = new Interpreter(ast: ast, existedScope: null, predefinedScope: null, path: "", isMain: true);
            interpreter.Interpret(false, false);
            Dictionary<string, ScriptValue> result = new Dictionary<string, ScriptValue>();
            foreach (var value in values)
            {
                if (!interpreter.CurrentScope.Variables.TryGetValue(value, out ScriptVariable? variable))
                    throw new KeyNotFoundException(value);
                result[value] = variable.Value;
            }
            return result;
        }

        public static void TryCatch1()
        {
            string sourceCode = @"
var value1 = null;
var value2 = null;
var value3 = null;
const list: List = [1, 2, 3];
            
try {
    value1 = list[3];
} catch {
    value2 = ""Passed"";
}
finally {
    value3 = ""End"";
}
";
            var values = Interpret(sourceCode, out _, "value1", "value2", "value3");
            CheckEquals(values["value1"], new ScriptValue());
            CheckEquals(values["value2"], new ScriptValue("Passed"));
            CheckEquals(values["value3"], new ScriptValue("End"));
        }

        public static void TryCatch2()
        {
            string sourceCode = @"
class MyException1 : Exception
{
    fn MyException1() {
        super(""My Message1"");
    }
}

class MyException2 : Exception
{
    fn MyException2() {
        super(""My Message2"");
    }
}

var ex1 = null;
var ex2 = null;

try {
    throw MyException2();
} catch (e: MyException1 | MyException2) {
    ex1 = e;
} catch e: MyException2 {
    ex2 = e;
}
";

            var values = Interpret(sourceCode, out var interpreter, "ex1", "ex2");
            CheckEquals(new ScriptValue(values["ex1"].TryObject.ClassType), new ScriptValue(interpreter.CurrentScope.Classes["MyException2"]));
            CheckEquals(values["ex2"], new ScriptValue());
        }

        public static void ArrowFnTest()
        {
            string sourceCode = @"
const Test = () => {
	const lists = [""good"", 3.14, """", 123, ""string"", 0.002, ""testing""]
	var count = 0
	var haveBreak = false
	for item in lists {
		count = count + 1
		if item == ""testing"" {
            haveBreak = true
			break
		}
        haveBreak = !haveBreak
	}
	if haveBreak {
		return ""Have Break""
	} else {
		return ""No Break""
	}
};

var result = Test();
";

            var values = Interpret(sourceCode, out var interpreter, "result");
            CheckEquals(values["result"], new ScriptValue("Have Break"));
        }

        public static void BubbleSortTest()
        {
            string sourceCode = @"
fn bubbleSort(list, compareFn = (x, y) => x > y) {
    for i in range(list.Length) {
        for j in range(list.Length - i - 1) {
         if compareFn(list[j], list[j + 1]) {
           var temp = list[j];
           list[j] = list[j + 1];
           list[j + 1] = temp;
         }
       }
    }
}

const list1 = [8, 7, 9, 4, 5, 2, 1, 0, 3, 5, 1, 2];
bubbleSort(list1);
const list2 = [8, 7, 9, 4, 5, 2, 1, 0, 3, 5, 1, 2];
bubbleSort(list2, (x, y) => x < y);
";

            var verify = (new int[] { 8, 7, 9, 4, 5, 2, 1, 0, 3, 5, 1, 2 }).Select(i => new ScriptValue(i));
            var values = Interpret(sourceCode, out var interpreter, "list1", "list2");
            CheckEquals(values["list1"], new ScriptValue(verify.OrderBy(i => (long)i.TryObject.BuildInObject).ToList()));
            CheckEquals(values["list2"], new ScriptValue(verify.OrderByDescending(i => (long)i.TryObject.BuildInObject).ToList()));
        }

        public static void FactorialTest()
        {
            string sourceCode = @"
fn factorial(x) {

    if x == 0 {
        return 1;
    } else {
        return x * factorial(x - 1);
    }
}

const result = factorial(12);
";

            var values = Interpret(sourceCode, out var interpreter, "result");
            CheckEquals(values["result"], new ScriptValue(479001600));
        }

        public static void ClassTest1()
        {
            string sourceCode = @"
class TestClass
{
	var value = null;

	fn TestClass(value) {
		this.value = value;
	}

	fn ToStr() {
		return ""I'm testing: "" + value;
	}
}

const test = TestClass(3.14);
const result1 = str(test);
const result2 = test.ToStr();
";

            var values = Interpret(sourceCode, out var interpreter, "result1", "result2");
            CheckEquals(values["result1"], new ScriptValue($"I'm testing: 3.14"));
            CheckEquals(values["result2"], new ScriptValue($"I'm testing: 3.14"));
        }

        public static void ClassTest2()
        {
            string sourceCode = @"
class Super1 {
	fn ToStr() => ""Super1"";
}

class Super2 { 
	fn ToStr() => ""Super2"";
}

class Super3 { }

class Middle1: Super1 {
	fn ToStr() => super.ToStr() + "" Middle1"";
}

class Middle2: Super2 {
	fn ToStr() => super.ToStr() + "" Middle2"";
}

class Middle3: Super3 {
	fn ToStr() => ""Middle3"";

	fn GetText() => this.Name + "" is Middle3"";
}

class TestClass: Middle1, Middle3, Middle2 {
	var Name: str { get; private set; }
	fn TestClass(name: str) {
		this.Name = name;
	}

	fn GetText() {
		return super.GetText();
	}
}
const result1 = TestClass(""Hello"").GetText();
const result2 = TestClass(""Hello"").ToStr();
";

            var values = Interpret(sourceCode, out var interpreter, "result1", "result2");
            CheckEquals(values["result1"], new ScriptValue("Hello is Middle3"));
            CheckEquals(values["result2"], new ScriptValue("Super1 Middle1"));
        }

        public static void ClassTest3()
        {
            string sourceCode = @"
class Test1 {
    var a = 3.14; 
    var b = str(this.a); 
    fn GetA() {
        return () => () => this.a; 
    }
}

const a = Test1().GetA;
const b = a();
const result1 = a()()();
const result2 = b()();
";

            var values = Interpret(sourceCode, out var interpreter, "result1", "result2");
            CheckEquals(values["result1"], new ScriptValue(3.14));
            CheckEquals(values["result2"], new ScriptValue(3.14));
        }

        public static void ClassTest4()
        {
            string sourceCode = @"
class Super1 {
    fn A() => ""Super1""; 
}

class Middle1 : Super1 {
    fn A() => super.A() + "" "" + ""Middle1""; 
}

class Test1 : Middle1 {
    fn A() => super.A() + "" "" + ""Test1""; 
    fn B() => () => () => () => () => super.A();
}

const a = Test1().A;
const b = Test1().B;
const result1 = a();
const result2 = b()()()()();
";

            var values = Interpret(sourceCode, out var interpreter, "result1", "result2");
            CheckEquals(values["result1"], new ScriptValue("Super1 Middle1 Test1"));
            CheckEquals(values["result2"], new ScriptValue("Super1 Middle1"));
        }

        public static void ClassTest5()
        {
            string sourceCode = @"
class Super1 {
    fn A() => ""Super1""; 
}

class Middle1 : Super1 {
    fn A() => super.A() + "" "" + ""Middle1""; 
}

class Test1 : Middle1 { 
    var a = () => super.A;
}

class Test2 : Middle1 { 
    var b = () => super;
}

const a = Test1().a;
const b = Test2().b;
const result1 = a()();
const result2 = b().A();
";

            var values = Interpret(sourceCode, out var interpreter, "result1", "result2");
            CheckEquals(values["result1"], new ScriptValue("Super1 Middle1"));
            CheckEquals(values["result2"], new ScriptValue("Super1 Middle1"));
        }

        public static void ClassTest6()
        {
            string sourceCode = @"
class Test1 { 
    public var A { get; set; }
    public var B { get; private set; }
    public var C { private get; set; }
    private var D { get; set; }

    static fn A() {
        return ""A"";
    }

    public fn SetB(value) => this.B = value;
    public fn GetC() => this.C;
    public fn GetD() => this.D;
    public fn SetD(value) => this.D = value;
}
const a = Test1.A;
const result1 = a();
const test1 = Test1();

test1.A = 1234;
const result2 = test1.A;
test1.SetB(""Hello, World"");
const result3 = test1.B;
test1.C = 3.14;
const result4 = test1.GetC();
test1.SetD(0.1);
const result5 = test1.GetD();
";

            var values = Interpret(sourceCode, out var interpreter, "result1", "result2", "result3", "result4", "result5");
            CheckEquals(values["result1"], new ScriptValue("A"));
            CheckEquals(values["result2"], new ScriptValue(1234));
            CheckEquals(values["result3"], new ScriptValue("Hello, World"));
            CheckEquals(values["result4"], new ScriptValue(3.14));
            CheckEquals(values["result5"], new ScriptValue(0.1));
        }

        public static void ForloopTest()
        {
            string sourceCode = @"
fn ForLoop(a) {
    const list = [];
	for i in range(2) {
		for item in a {
            list.Add(item * i);
			fn WhileLoop(start, end) {
				while start < end {
					list.Add([start, end]);
					start = start + 1;
				}
			}
			WhileLoop(0, 2);
		}
	}
    return list;
}

const result = str(ForLoop(""Hello, World!""));
";
            
            var values = Interpret(sourceCode, out var interpreter, "result");
            CheckEquals(values["result"], new ScriptValue(@"[""H"", [0, 2], [1, 2], ""e"", [0, 2], [1, 2], ""l"", [0, 2], [1, 2], ""l"", [0, 2], [1, 2], ""o"", [0, 2], [1, 2], "","", [0, 2], [1, 2], "" "", [0, 2], [1, 2], ""W"", [0, 2], [1, 2], ""o"", [0, 2], [1, 2], ""r"", [0, 2], [1, 2], ""l"", [0, 2], [1, 2], ""d"", [0, 2], [1, 2], ""!"", [0, 2], [1, 2], ""H"", [0, 2], [1, 2], ""e"", [0, 2], [1, 2], ""l"", [0, 2], [1, 2], ""l"", [0, 2], [1, 2], ""o"", [0, 2], [1, 2], "","", [0, 2], [1, 2], "" "", [0, 2], [1, 2], ""W"", [0, 2], [1, 2], ""o"", [0, 2], [1, 2], ""r"", [0, 2], [1, 2], ""l"", [0, 2], [1, 2], ""d"", [0, 2], [1, 2], ""!"", [0, 2], [1, 2]]"));
        }

        public static void FnCallArgsWithName()
        {
            string sourceCode = @"
var result1 = null;
var result2 = null;
var result3 = null;
var result4 = null;
fn Test(a, b, c, d) {
    result1 = d;
    result2 = c;
    result3 = b;
    result4 = a;
}

Test(12, d:34, b:56, c:78)
";
            var values = Interpret(sourceCode, out var interpreter, "result1", "result2", "result3", "result4");
            CheckEquals(values["result1"], new ScriptValue(34));
            CheckEquals(values["result2"], new ScriptValue(78));
            CheckEquals(values["result3"], new ScriptValue(56));
            CheckEquals(values["result4"], new ScriptValue(12));
        }

        public static void JsonTest()
        {
            string sourceCode = @"
class Test {
    public var t1 { get; set; }
    public var t2: decimal { get; set; }
    public var t3 { get; set; }
}

class A {
    public var a { get; set; }
    public var b { get; set; }
    public var c { get; set; }
    public var d: Test { get; set; }
}

var a = A();
a.a = ""Hello"";
a.b = 658654383;

var result1 = Json.Serialize(a);

var j2 = ""{ \""a\"": \""Hello world\"", \""b\"": 3.14, \""c\"": 12345,  \""d\"": { \""t1\"": \""Test1\"", \""t2\"": 12345, \""t3\"": 3.14 } }""

var b = Json.Deserialize(j2, Type(A));
var result2 = Json.Serialize(b.d);
var result3 = Json.Serialize(b);
";
            var values = Interpret(sourceCode, out var interpreter, "result1", "result2", "result3");
            CheckEquals(values["result1"], new ScriptValue("{\"a\": \"Hello\", \"b\": 658654383, \"c\": null, \"d\": null}"));
            CheckEquals(values["result2"], new ScriptValue("{\"t1\": \"Test1\", \"t2\": 12345, \"t3\": 3.14}"));
            CheckEquals(values["result3"], new ScriptValue("{\"a\": \"Hello world\", \"b\": 3.14, \"c\": 12345, \"d\": {\"t1\": \"Test1\", \"t2\": 12345, \"t3\": 3.14}}"));
        }

        public static void DictTest()
        {
            string sourceCode = @"
const a = {
    ""Test"": 12345
};

var result1 = a.GetValue(""Test"");
";
            var values = Interpret(sourceCode, out var interpreter, "result1");
            CheckEquals(values["result1"], new ScriptValue(12345));
        }


        public static void IteratorTest()
        {
            string sourceCode = @"
class Test: Iterator {
    private let values = null;
    public let Iter => TestIterator.Create(this.values);

    public fn Test(values: Iterator) {
        this.values = values
    }

    private class TestIterator {
        private let values = null;
        private let index = -1;

        public fn TestIterator(values) {
            this.values = values;
        }

        private fn GetCurrent() {
            return this.values[this.index];
        }

        private fn MoveNext() {
            if this.index + 1 >= this.values.Length {
                return false;
            }
            this.index++;
            return true;
        }

        private fn Reset() {
            this.index = -1;
        }

        public static fn Create(values) {
            let result = TestIterator(values);
            return Iterator(result.GetCurrent, result.MoveNext, result.Reset);
        }
    }
}

const test = Test([*range(10, 20)]);
const result1 = test.ToList();
const result2 = test.Iter.ToList();
";
            var values = Interpret(sourceCode, out var interpreter, "result1", "result2");
            var result = Enumerable.Range(10, 10).Select(i => new ScriptValue(i)).ToList();
            CheckEquals(values["result1"], new ScriptValue(result));
            CheckEquals(values["result2"], new ScriptValue(result));
        }

        public static void IteratorTest2()
        {
            string sourceCode = @"
const result1 = range(25, 50).SelectWithIndex((value, index) => *[value, index]).Where((value, index) => index >= 10).Select((value, index) => $""{index}: {value}"").ToList();
const result2 = range(25, 50).Iter.SelectWithIndex((value, index) => *[value, index]).Where((value, index) => index >= 10).Select((value, index) => $""{index}: {value}"").ToList();
";
            var values = Interpret(sourceCode, out var interpreter, "result1", "result2");
            var result = Enumerable.Range(25, 25).Select((value, index) => Tuple.Create(value, index)).Where(vi => vi.Item2 >= 10).Select(vi => new ScriptValue($"{vi.Item2}: {vi.Item1}")).ToList();
            CheckEquals(values["result1"], new ScriptValue(result));
            CheckEquals(values["result2"], new ScriptValue(result));
        }
    }
}

