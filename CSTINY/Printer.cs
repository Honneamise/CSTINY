namespace CSTINY;

public class Printer
{
    private readonly Ast ast;
    private int level;

    public Printer(Ast ast)
    {
        
        this.ast = ast;
        level = 1;
    }

    public void Display()
    {
        Visit((ProgramNode)ast);
    }

    private void PrintLn(object o)
    {
        for (int i = 1; i<level; i++)
        { Console.Write("\t"); }

        Console.WriteLine(o);
    }

    private void Visit(Ast ast)
    {
        switch (ast)
        {
            case ProgramNode: Visit((ProgramNode)ast); break;
            case LineNode: Visit((LineNode)ast); break;
            case DeclareNode: Visit((DeclareNode)ast); break;
            case LetNode: Visit((LetNode)ast); break;
            case InputNode: Visit((InputNode)ast); break;
            case PrintNode: Visit((PrintNode)ast); break;
            case IfNode: Visit((IfNode)ast); break;
            case GotoNode: Visit((GotoNode)ast); break;
            case GosubNode: Visit((GosubNode)ast); break;
            case ReturnNode: Visit((ReturnNode)ast); break;
            case EndNode: Visit((EndNode)ast); break;
            case BinaryNode: Visit((BinaryNode)ast); break;
            case IdentNode: Visit((IdentNode)ast); break;
            case NumberNode: Visit((NumberNode)ast); break;
            case StringNode: Visit((StringNode)ast); break;

            default: break;
        };
    }

    private void Visit(ProgramNode node)
    {
        PrintLn("[PROGRAM]");

        level++;
        foreach (LineNode line in node.Lines)
        {
            Visit(line);
        }
        level--;

    }

    private void Visit(LineNode node)
    {
        PrintLn($"[LINE {node.Number}]");

        level++;
        Visit(node.Statement);
        level--;
    }

    private void Visit(DeclareNode node)
    {
        PrintLn("[DECLARATION STMT]");

        level++;
        foreach (string ident in node.Identlist)
        {
            PrintLn($"[{node.Type}: {ident}]");
        }
        level--;
    }

    private void Visit(LetNode node)
    {
        PrintLn("[LET STMT]");

        level++;
        PrintLn($"[IDENT: {node.Ident}]");
        Visit(node.Item);
        level--;
    }

    private void Visit(InputNode node)
    {
        PrintLn($"[INPUT STMT ({node.Ident})]");
    }

    private void Visit(PrintNode node)
    {
        PrintLn("[PRINT STMT]");

        level++;
        foreach (Ast ast in node.Itemlist)
        {
            Visit(ast);
        }
        level--;
    }

    private void Visit(IfNode node)
    {
        PrintLn("[IF STMT]");

        level++;
        PrintLn($"[RELOP: {node.Relop}]");
        Visit(node.Left);
        Visit(node.Right);
        Visit(node.Statement);
        level--;
    }

    private void Visit(GotoNode node)
    {
        PrintLn($"[GOTO STMT ({node.Number})]");
    }

    private void Visit(GosubNode node)
    {
        PrintLn($"[GOSUB STMT ({node.Number})]");
    }

    private void Visit(ReturnNode _)
    {
        PrintLn($"[RETURN STMT]");
    }

    private void Visit(EndNode _)
    {
        PrintLn($"[END STMT]");
    }

    private void Visit(BinaryNode node)
    {
        PrintLn("[EXPRESSION]");

        level++;
        PrintLn($"[OP: {node.Op}]");
        Visit(node.Left);
        Visit(node.Right);
        level--;
    }

    private void Visit(IdentNode node)
    {
        PrintLn($"[IDENT: {node.Name}]");
    }

    private void Visit(NumberNode node)
    {
        PrintLn($"[NUMBER: {node.Value}]");
    }

    private void Visit(StringNode node)
    {
        PrintLn($"[STRING: \"{node.Value}\"]");
    }

}
