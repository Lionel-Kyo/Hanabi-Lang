using HanabiLang.Interprets;
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
                    Console.WriteLine($"Failed: {ex.Message}");
                }
            }

            Console.WriteLine($"\nPassed Test: {passed}/{methods.Length}\n");
        }
        private static void CheckEquals(ScriptValue a, ScriptValue b)
        {
            if (!a.Equals(b))
                throw new SystemException($"{a} != {b}");
        }

        private static Interpreter Interpret(string code)
        {
            var tokens = Lexer.Tokenize(code.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
            var parser = new Parser(tokens);
            var ast = parser.Parse();

            Interpreter interpreter = new Interpreter(ast: ast, existedScope: null, predefinedScope: null, path: "", isMain: true);
            interpreter.Interpret(false, false);
            return interpreter;
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
            Interpreter interpreter = Interpret(sourceCode);
            var value1 = interpreter.CurrentScope.Variables["value1"].Value;
            var value2 = interpreter.CurrentScope.Variables["value2"].Value;
            var value3 = interpreter.CurrentScope.Variables["value3"].Value;
            CheckEquals(value1, new ScriptValue());
            CheckEquals(value2, new ScriptValue("Passed"));
            CheckEquals(value3, new ScriptValue("End"));
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
} catch (e: MyException2) {
    ex2 = e;
}
";

            Interpreter interpreter = Interpret(sourceCode);
            var ex1 = interpreter.CurrentScope.Variables["ex1"].Value;
            var ex2 = interpreter.CurrentScope.Variables["ex2"].Value;
            CheckEquals(new ScriptValue(ex1.TryObject.ClassType), new ScriptValue(interpreter.CurrentScope.Classes["MyException2"]));
            CheckEquals(ex2, new ScriptValue());
        }
    }
}
