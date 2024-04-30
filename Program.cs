using CSTINY;
class Program
{
    private static void Main(string[] args)
    {
        Core.Init();

        if ((args.Length == 2 && (args[0] is "-t" or "-i"))
           || (args.Length == 3 && (args[0] is "-n")))
        {
            string input = File.ReadAllText(args[1]);

            Lexer lexer = new(input);
            Parser parser = new(lexer);
            Ast ast = parser.Parse();
            Analyzer anal = new(ast);
            anal.Scan();
            Symtab symtab = new(ast);
            Scope scope = symtab.Build();

            if (args[0] == "-t")
            {
                Printer printer = new(ast);
                printer.Display();
                return;
            }
            else if (args[0] == "-i")
            {
                Interpreter intp = new(ast, scope);
                intp.Exec();
                return;
            }
            else if (args[0] == "-n")
            {
                Nasmer nasmer = new(ast, scope);
                string str = nasmer.Generate();
                File.WriteAllText(args[2], str);
                return;
            }
        }
        else
        {
            Core.Usage();
        }        
    }
}