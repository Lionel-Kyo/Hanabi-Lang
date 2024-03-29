﻿using System;
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
        public static void Main(string[] args)
        {
            //string path = "./Test.txt";
            //string path = "./Test2.txt";
            //string path = "./Test3.txt";
            string path = "./Test4.txt";
            //string path = "./BubbleSort.txt";
            //string path = "./CsImport.txt";
            //string path = "./CsImport2.txt";
            //string path = "./FnInClass.txt";
            //string path = "./InterpolatedString.txt";
            //string path = "./ImportJson.txt";
            //string path = "./ChristmasTree.txt";
            // string path = "./ForLoopTest.txt";
            //string path = "./switchTest.txt";
            path = "H:/HanabiScriptTest/ImportJson.txt";
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
            Interpreter interpreter = new Interpreter(ast, null, path, true);
            ImportedItems.Files[path] = Tuple.Create(lastWriteTimeUtc, interpreter);
            interpreter.Interpret(false);
        }
    }
}
