using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HanabiLang.Lexers;
using HanabiLang.Parses;
using HanabiLang.Interprets;
using System.Collections;
using System.Threading;

namespace HanabiLang
{
    internal class Program
    {
        public static void ExecuteFile(string[] args)
        {
            //string path = "./Test.txt";
            //string path = "./Test2.txt";
            //string path = "./Test3.txt";
            //string path = "./BubbleSort.txt";
            //string path = "./CsImport.txt";
            //string path = "./CsImport2.txt";
            //string path = "./FnInClass.txt";
            //string path = "./InterpolatedString.txt";
            //string path = "./ImportJson.txt";
            //string path = "./ChristmasTree.txt";
            //string path = "./ForLoopTest.txt";
            //string path = "./ForLoopFuncBug.txt";
            //string path = "./switchTest.txt";
            string path = "H:/HanabiScriptTest/ImportJson.txt";
            if (args.Length > 0 && File.Exists(args[0]))
            {
                path = args[0];
            }
            string[] lines = File.ReadAllLines(path);
            var tokens = Lexer.Tokenize(lines);
            foreach (var token in tokens)
            {
                //Console.WriteLine(token);
            }
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            foreach (var item in ast.Nodes)
            {
                Console.WriteLine(item);
            }
            //Console.WriteLine();
            path = Path.GetFullPath(path).Replace("\\", "/");
            DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
            Interpreter.Arguments = args;
            Interpreter interpreter = new Interpreter(ast: ast, existedScope: null, predefinedScope: null, path: "", isMain: true);
            ImportedItems.Files[path] = Tuple.Create(lastWriteTimeUtc, interpreter);
            interpreter.Interpret(false, false);
        }

        public static void Start(string[] args)
        {
            Interpreter.Arguments = args;
            Interpreter interpreter = new Interpreter(ast: null, existedScope: null, predefinedScope: null, path: "", isMain: true);
            List<string> lines = new List<string>();
            while (true)
            {
                if (lines.Count <= 0)
                    Console.Write(">>> ");
                else
                    Console.Write("... ");
                string line = Console.ReadLine();
                lines.Add(line);
                List<Token> tokens = null;
                try
                {
                    tokens = Lexer.Tokenize(lines);
                }
                catch (Exception ex)
                {
                    lines = new List<string>();
                    Console.WriteLine(Parser.ExceptionToString(ex));
                }
                if (tokens == null)
                    continue;

                var parser = new Parser(tokens);
                AbstractSyntaxTree ast = null;
                try
                {
                    ast = parser.Parse();
                }
                catch (ParseFormatNotCompleteException ex)
                {
                }
                catch (Exception ex)
                {
                    lines = new List<string>();
                    Console.WriteLine(Parser.ExceptionToString(ex));
                }
                if (ast == null)
                    continue;

                Interpreter tempInterpreter = new Interpreter(ast: ast, existedScope: interpreter.CurrentScope,
                    predefinedScope: interpreter.PredefinedScope, path: interpreter.Path, isMain: true);
                try
                {
                    tempInterpreter.Interpret(false, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(Interpreter.ExceptionToString(ex));
                }
                lines = new List<string>();
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length <= 0)
                Start(args);
            else
                ExecuteFile(args);
        }
    }
}
