using HanabiLangLib.Interprets;
using HanabiLangLib.Interprets.Exceptions;
using HanabiLangLib.Interprets.ScriptTypes;
using HanabiLangLib.Lexers;
using HanabiLangLib.Parses;
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
            for (Exception? exception = ex; exception != null; exception = exception.InnerException)
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
            if (ScriptBool.AsCSharp(ScriptValue.NotEquals(a, b).TryObject))
                throw new SystemException($"{a} != {b}");
        }

        private static void CheckNotEquals(ScriptValue a, ScriptValue b)
        {
            if (ScriptBool.AsCSharp(ScriptValue.Equals(a, b).TryObject))
                throw new SystemException($"{a} == {b}");
        }

        private static Dictionary<string, ScriptValue> Interpret(string code, out Interpreter interpreter, params string[] values)
        {
            // var _tokens = Lexer.Tokenize(code.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
            var tokens = new NewLexer(code).Tokenize();
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

        public static void StringTest()
        {
            string sourceCode = @"
let a = null;

const result1 = $""{(a == null ? ""Hi"" : 1234)}""
const result2 = @$""{(a == null ? ""Hi"" : 1234)}""
a = true;
const result3 = $""{(a == null ? ""Hi"" : 1234)}""
const result4 = $@""{(a == null ? ""Hi"" : 1234)}""
const result5 = @""{a == null ? """" : 1234}""
";
            var values = Interpret(sourceCode, out _, "result1", "result2", "result3", "result4", "result5");
            bool? a = null;
            CheckEquals(values["result1"], new ScriptValue($"{(a == null ? "Hi" : 1234)}"));
            CheckEquals(values["result2"], new ScriptValue(@$"{(a == null ? "Hi" : 1234)}"));
            a = true;
            CheckEquals(values["result3"], new ScriptValue($"{(a == null ? "Hi" : 1234)}"));
            CheckEquals(values["result4"], new ScriptValue($@"{(a == null ? "Hi" : 1234)}"));
            CheckEquals(values["result5"], new ScriptValue(@"{a == null ? "" : 1234}"));
        }

        public static void TryCatch1()
        {
            string sourceCode = @"
let value1 = null;
let value2 = null;
let value3 = null;
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
class MyException1 : Exception {
    fn MyException1(this) {
        super(""My Message1"");
    }
}

class MyException2 : Exception {
    fn MyException2(this) {
        super(""My Message2"");
    }
}

let ex1 = null;
let ex2 = null;

try {
    throw MyException2();
} catch (e: MyException1 | MyException2) {
    ex1 = e;
} catch e: MyException2 {
    ex2 = e;
}
";

            var values = Interpret(sourceCode, out var interpreter, "ex1", "ex2");
            CheckEquals(new ScriptValue(values["ex1"].TryObject.ClassType), interpreter.CurrentScope.Variables["MyException2"].Value);
            CheckEquals(values["ex2"], new ScriptValue());
        }

        public static void ArrowFnTest()
        {
            string sourceCode = @"
const Test = () => {
	const lists = [""good"", 3.14, """", 123, ""string"", 0.002, ""testing""]
	let count = 0
	let hasBreak = false
	for item in lists {
		count = count + 1
		if item == ""testing"" {
            hasBreak = true
			break
		}
        hasBreak = !hasBreak
	}
	if hasBreak {
		return ""Has Break""
	} else {
		return ""No Break""
	}
};

const Add = (a, b) => {
    return a + b;
};

const CallArg = (arg) => {
    return arg();
};

const result1 = Test();
const result2 = ((((()=>()=>()=>()=>""Hello, World""))))()()()();
const result3 = CallArg(() => Add(1, 2),);
";

            var values = Interpret(sourceCode, out var interpreter, "result1", "result2", "result3");
            CheckEquals(values["result1"], new ScriptValue("Has Break"));
            CheckEquals(values["result2"], new ScriptValue("Hello, World"));
            CheckEquals(values["result3"], new ScriptValue(3));
        }

        public static void BubbleSortTest()
        {
            string sourceCode = @"
fn bubbleSort(list, compareFn = (x, y) => x > y) {
    for i in range(list.Length) {
        for j in range(list.Length - i - 1) {
            if compareFn(list[j], list[j + 1]) {
                list[j], list[j + 1] = list[j + 1], list[j]
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
class TestClass {
	let value = null;

	fn TestClass(this, value) {
		this.value = value;
	}

	fn ToStr(this) {
		return ""I'm testing: "" + this.value;
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
	fn ToStr(this) => ""Super1"";
}

class Super2 { 
	fn ToStr(this) => ""Super2"";
}

class Super3 { }

class Middle1: Super1 {
	fn ToStr(this) => super.ToStr() + "" Middle1"";
}

class Middle2: Super2 {
	fn ToStr(this) => super.ToStr() + "" Middle2"";
}

class Middle3: Super3 {
	fn ToStr(this) => ""Middle3"";

	fn GetText(this) => this.Name + "" is Middle3"";
}

class TestClass: Middle1, Middle3, Middle2 {
	let Name: str { get; private set; }
	fn TestClass(this, name: str) {
		this.Name = name;
	}

	fn GetText(this) {
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
    let a = 3.14; 
    let b = str(this.a); 
    fn GetA(this) {
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
    fn A(this) => ""Super1""; 
}

class Middle1 : Super1 {
    fn A(this) => super.A() + "" "" + ""Middle1""; 
}

class Test1 : Middle1 {
    fn A(this) => super.A() + "" "" + ""Test1""; 
    fn B(this) => () => () => () => () => super.A();
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
    fn A(this) => ""Super1""; 
}

class Middle1 : Super1 {
    fn A(this) => super.A() + "" "" + ""Middle1""; 
}

class Test1 : Middle1 { 
    let a = () => super.A;
}

class Test2 : Middle1 { 
    let b = () => super;
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
    public let A { get; set; }
    public let B { get; private set; }
    public let C { private get; set; }
    private let D { get; set; }

    static fn A() {
        return ""A"";
    }

    public fn SetB(this, value) => this.B = value;
    public fn GetC(this) => this.C;
    public fn GetD(this) => this.D;
    public fn SetD(this, value) => this.D = value;
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

        public static void ClassTest7()
        {
            string sourceCode = @"
class Test {
    fn Test(this) { }
    const a = () => ""Hello a""
    let b = () => ""Hello b""
    const c => () => ""Hello c""
    let d => () => ""Hello d""
}

const result1 = catch(Test.a())
const result2 = catch(Test.b())
const result3 = catch(Test.c())
const result4 = catch(Test.d())
const test = Test()
const result5 = catch(test.a())
const result6 = catch(test.b())
const result7 = catch(test.c())
const result8 = catch(test.d())
";

            var values = Interpret(sourceCode, out var interpreter, "result1", "result2", "result3", "result4", "result5", "result6", "result7", "result8");
            CheckEquals(values["result1"], new ScriptValue()); // No Error
            CheckNotEquals(values["result2"], new ScriptValue()); // Error
            CheckEquals(values["result3"], new ScriptValue()); // No Error
            CheckNotEquals(values["result4"], new ScriptValue()); // Error
            CheckEquals(values["result5"], new ScriptValue()); // No Error
            CheckEquals(values["result6"], new ScriptValue()); // No Error
            CheckEquals(values["result7"], new ScriptValue()); // No Error
            CheckEquals(values["result8"], new ScriptValue()); // No Error
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
            CheckEquals(values["result"], new ScriptValue(@"["""", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], """", [0, 2], [1, 2], ""H"", [0, 2], [1, 2], ""e"", [0, 2], [1, 2], ""l"", [0, 2], [1, 2], ""l"", [0, 2], [1, 2], ""o"", [0, 2], [1, 2], "","", [0, 2], [1, 2], "" "", [0, 2], [1, 2], ""W"", [0, 2], [1, 2], ""o"", [0, 2], [1, 2], ""r"", [0, 2], [1, 2], ""l"", [0, 2], [1, 2], ""d"", [0, 2], [1, 2], ""!"", [0, 2], [1, 2]]"));
        }

        public static void FnCallArgsWithName()
        {
            string sourceCode = @"
let result1 = null;
let result2 = null;
let result3 = null;
let result4 = null;
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
import ""Type""
import ""Json5"" as Json
import ""decimal""

class Test {
    public let t1 { get; set; }
    public let t2: decimal { get; set; }
    public let t3 { get; set; }
}

class A {
    public let a { get; set; }
    public let b { get; set; }
    public let c { get; set; }
    public let d: Test { get; set; }
}

let a = A();
a.a = ""Hello"";
a.b = 658654383;

let result1 = Json.Serialize(a);

let j2 = ""{ \""a\"": \""Hello world\"", \""b\"": 3.14, \""c\"": 12345,  \""d\"": { \""t1\"": \""Test1\"", \""t2\"": 12345, \""t3\"": 3.14 } }""

let b = Json.Deserialize(j2, Type(A));
let result2 = Json.Serialize(b.d);
let result3 = Json.Serialize(b);
";
            var values = Interpret(sourceCode, out var interpreter, "result1", "result2", "result3");
            CheckEquals(values["result1"], new ScriptValue("{\"a\": \"Hello\",\"b\": 658654383,\"c\": null,\"d\": null}"));
            CheckEquals(values["result2"], new ScriptValue("{\"t1\": \"Test1\",\"t2\": 12345.0,\"t3\": 3.14}"));
            CheckEquals(values["result3"], new ScriptValue("{\"a\": \"Hello world\",\"b\": 3.14,\"c\": 12345,\"d\": {\"t1\": \"Test1\",\"t2\": 12345.0,\"t3\": 3.14}}"));
        }

        public static void DictTest()
        {
            string sourceCode = @"
const a = {
    ""Test"": 12345
};

let result1 = a.GetValue(""Test"");
";
            var values = Interpret(sourceCode, out var interpreter, "result1");
            CheckEquals(values["result1"], new ScriptValue(12345));
        }


        public static void IterableTest()
        {
            string sourceCode = @"
import ""Iterable""

class Test: Iterable {
    private let values = null;
    public let Iter => TestIterator.Create(this.values);

    public fn Test(this, values: Iterable) {
        this.values = values;
    }

    private class TestIterator {
        private let values = null;
        private let index = -1;

        public fn TestIterator(this, values) {
            this.values = values;
        }

        private fn GetCurrent(this) {
            return this.values[this.index];
        }

        private fn MoveNext(this) {
            if this.index + 1 >= this.values.Length {
                return false;
            }
            this.index++;
            return true;
        }

        private fn Reset(this) {
            this.index = -1;
        }

        public static fn Create(values) {
            let result = TestIterator(values);
            return Iterable.Iterator(result.GetCurrent, result.MoveNext, result.Reset);
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

        public static void OperaterTest1()
        {
            string sourceCode = @"
const step = 1;
const x = 0;
const stop = 0;
const result1 = (step < 0 && x <= stop) || (step > 0 && x >= stop);
const result2 = step < 0 && x <= stop || step > 0 && x >= stop;
";
            var values = Interpret(sourceCode, out var interpreter, "result1", "result2");
            CheckEquals(values["result1"], new ScriptValue(true));
            CheckEquals(values["result2"], new ScriptValue(true));
        }

        public static void OperaterTest2()
        {
            string sourceCode = @"
let a = range(10).ToList();
const result1 = a?.Where(x => x > 5)?.ToList() ?? [];
a = null;
const result2 = a?.Where(x => x > 5)?.ToList() ?? [];
";
            var values = Interpret(sourceCode, out var interpreter, "result1", "result2");
            CheckEquals(values["result1"], new ScriptValue(Enumerable.Range(6, 4).Select(i => new ScriptValue(i)).ToList()));
            CheckEquals(values["result2"], new ScriptValue(new List<ScriptValue>()));
        }

        public static void CatchExpressionTest()
        {
            string sourceCode = @"
const d = { ""A"": 1234, ""B"": [1, 2, 3, 4, 5] }
const result1, result1Err = catch(d[""A""])
const result2, result2Err = catch(d[12345])
const result3a, result3b, result3c, result3d, result3e, result3Err = catch(d[""B""])
const result4a, result4b, result4c, result4Err = catch(d[""B""])
const result5a, result5b, result5c, result5d, result5e, result5f, result5Err = catch(d[""B""])
";
            var values = Interpret(sourceCode, out var interpreter, "result1", "result1Err", "result2", "result2Err",
                "result3a", "result3b", "result3c", "result3d", "result3e", "result3Err",
                "result4a", "result4b", "result4c", "result4Err",
                "result5a", "result5b", "result5c", "result5d", "result5e", "result5f", "result5Err");
            CheckEquals(values["result1"], new ScriptValue(1234));
            CheckEquals(values["result1Err"], new ScriptValue());
            CheckEquals(values["result2"], new ScriptValue());
            CheckNotEquals(values["result2Err"], new ScriptValue());
            CheckEquals(values["result3a"], new ScriptValue(1));
            CheckEquals(values["result3b"], new ScriptValue(2));
            CheckEquals(values["result3c"], new ScriptValue(3));
            CheckEquals(values["result3d"], new ScriptValue(4));
            CheckEquals(values["result3e"], new ScriptValue(5));
            CheckEquals(values["result3Err"], new ScriptValue());
            CheckEquals(values["result4a"], new ScriptValue());
            CheckEquals(values["result4b"], new ScriptValue());
            CheckEquals(values["result4c"], new ScriptValue());
            CheckNotEquals(values["result4Err"], new ScriptValue());
            CheckEquals(values["result5a"], new ScriptValue());
            CheckEquals(values["result5b"], new ScriptValue());
            CheckEquals(values["result5c"], new ScriptValue());
            CheckEquals(values["result5d"], new ScriptValue());
            CheckEquals(values["result5e"], new ScriptValue());
            CheckEquals(values["result5f"], new ScriptValue());
            CheckNotEquals(values["result5Err"], new ScriptValue());
        }

        public static void SlicerTest1()
        {
            string sourceCode = @"
const result1 = ""Hello, World""[2:8:2]
const result2 = ""Hello, World""[2::-1]
const result3 = ""Hello, World""[::-1]
const result4 = ""Hello, World""[2:7:2]
const result5 = ""Hello, World""[-1:-9:-1]
const result6 = ""Hello, World""[:]
const result7 = ""Hello, World""[-1:-8:-2]
const result8 = ""Hello, World""[-2:-1]
const result9 = ""Hello, World""[0:9:3]
const result10 = ""Hello, World""[9:3]
const result11 = ""Hello, World""[2:6:-1]
const result12 = catch(""Hello, World""[1:5:0])
const result13 = ""Hello, World""[0:1]
const result14 = ""Hello, World""[2:2]
";
            var values = Interpret(sourceCode, out var interpreter, "result1", "result2", "result3", "result4", "result5", "result6",
                "result7", "result8", "result9", "result10", "result11", "result12", "result13", "result14");
            CheckEquals(values["result1"], new ScriptValue("lo "));
            CheckEquals(values["result2"], new ScriptValue("leH"));
            CheckEquals(values["result3"], new ScriptValue("dlroW ,olleH"));
            CheckEquals(values["result4"], new ScriptValue("lo "));
            CheckEquals(values["result5"], new ScriptValue("dlroW ,o"));
            CheckEquals(values["result6"], new ScriptValue("Hello, World"));
            CheckEquals(values["result7"], new ScriptValue("drW,"));
            CheckEquals(values["result8"], new ScriptValue("l"));
            CheckEquals(values["result9"], new ScriptValue("Hl "));
            CheckEquals(values["result10"], new ScriptValue(""));
            CheckEquals(values["result11"], new ScriptValue(""));
            CheckNotEquals(values["result12"], new ScriptValue());
            CheckEquals(values["result13"], new ScriptValue("H"));
            CheckEquals(values["result14"], new ScriptValue(""));
        }
    }
}

