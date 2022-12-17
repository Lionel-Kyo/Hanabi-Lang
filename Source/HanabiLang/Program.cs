using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HanabiLang.Lexers;
using HanabiLang.Parses;
using HanabiLang.Interprets;

namespace HanabiLang
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            BuildInFns.AddBasicFunctions();
            //string path = "./Test3.txt";
            //string path = "./Test4.txt";
            //string path = "./BubbleSort.txt";
            string path = "./CsImport.txt";
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
                //Console.WriteLine(item);
            }
            //Console.WriteLine();
            Interpreter interpreter = new Interpreter(ast, path, true, args);
            ImportedFiles.Files[System.IO.Path.GetFullPath(path)] = interpreter;
            interpreter.Interpret();
        }
    }
}
