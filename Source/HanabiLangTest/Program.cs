using HanabiLang.Interprets;
using HanabiLang.Interprets.ScriptTypes;
using HanabiLang.Lexers;
using HanabiLang.Parses;
using System.Diagnostics;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Interpreter.Arguments = args;

HanabiLangTest.TestCases.Run();