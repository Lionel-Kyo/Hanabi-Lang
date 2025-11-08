using HanabiLangLib.Interprets;
using HanabiLangLib.Interprets.ScriptTypes;
using HanabiLangLib.Lexers;
using HanabiLangLib.Parses;
using System.Diagnostics;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Interpreter.Arguments = args;

HanabiLangTest.TestCases.Run();