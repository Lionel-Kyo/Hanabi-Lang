using System.Text;
using HanabiLang.Lexers;
using HanabiLang.Parses;
using HanabiLang.Interprets;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Interpreter.Arguments = args;

//TestCases.Run();
if (args.Length <= 0)
    Start();
else
    ExecuteFile(args);

void ExecuteFile(string[] args)
{
    string path;
    if (args.Length > 0 && File.Exists(args[0]))
        path = args[0];
    else
        return;
    string[] lines = File.ReadAllLines(path);
    var tokens = Lexer.Tokenize(lines);
    //Console.WriteLine(string.Join("\n", tokens));
    var parser = new Parser(tokens);
    var ast = parser.Parse();
    //Console.WriteLine(string.Join("\n", ast.Nodes));

    path = Path.GetFullPath(path).Replace("\\", "/");
    DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
    Interpreter interpreter = new Interpreter(ast: ast, existedScope: null, predefinedScope: null, path: path, isMain: true);
    ImportedItems.Files[path] = Tuple.Create(lastWriteTimeUtc, interpreter);
    interpreter.Interpret(false, false);
}

void Start()
{
    Interpreter interpreter = new Interpreter(ast: null, existedScope: null, predefinedScope: null, path: "", isMain: true);
    List<string> lines = new List<string>();
    while (true)
    {
        if (lines.Count <= 0)
            Console.Write(">>> ");
        else
            Console.Write("... ");
        string line = Console.ReadLine() ?? "";
        lines.Add(line);
        List<Token>? tokens = null;
        try
        {
            tokens = Lexer.Tokenize(lines);
            //Console.WriteLine(string.Join("\n", tokens));
        }
        catch (Exception ex)
        {
            lines = new List<string>();
            Console.WriteLine(Parser.ExceptionToString(ex));
        }
        if (tokens == null)
            continue;

        var parser = new Parser(tokens);
        AbstractSyntaxTree? ast = null;
        try
        {
            ast = parser.Parse();
            Console.WriteLine(string.Join("\n", ast.Nodes));
        }
        catch (ParseFormatNotCompleteException ex)
        {
            if (lines.Count > 1 && line.Length <= 0)
            {
                lines = new List<string>();
                Console.WriteLine(Parser.ExceptionToString(ex));
            }
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