﻿using HanabiLang.Interprets;
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

namespace HanabiLang
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
                if (!interpreter.CurrentScope.Variables.TryGetValue(value, out ScriptVariable variable))
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
            CheckEquals(values["result2"], new ScriptValue("Super2 Middle2"));
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
    }
}

