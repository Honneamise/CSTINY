namespace CSTINY;

public class Interpreter
{
    private readonly ProgramNode root;
    private readonly Scope scope;
    private readonly Dictionary<string, object> memory;
    private int pc;
    private readonly Stack<int> stack;
    private LineNode currentLine;

    public Interpreter(Ast ast, Scope scope)
    {
        root = (ProgramNode)ast;
        this.scope = scope;
        memory = new();
        pc = 0;
        stack = new();

        currentLine = root.Lines[pc];
    }

    public void Exec()
    {
        Visit(root);
    }

    private static string ReadString()
    {
        string? s;

        do { s = Console.ReadLine(); }
        while (s == null);
    
        return s;
    }

    private static short ReadNumber()
    {
        short s;

        while (!short.TryParse(Console.ReadLine(), out s)){ }

        return s;
    }
    
    private object Visit(Ast ast)
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
            case BinaryNode: return Visit((BinaryNode)ast);
            case IdentNode: return Visit((IdentNode)ast);
            case NumberNode: return ((NumberNode)ast).Value;
            case StringNode: return ((StringNode)ast).Value;

            default: break;
        };

        return new();
    }

    private void Visit(ProgramNode node)
    {
        while (pc >= 0)
        {
            if (pc >= node.Lines.Count)
            {
                string msg = $"[Line {currentLine.Number}]PC out of range";
                throw new Exception(msg);
            }

            currentLine = node.Lines[pc];

            pc++;

            Visit(currentLine);
        }
    }

    private void Visit(LineNode node)
    {
        Visit(node.Statement);
    }

    private void Visit(DeclareNode node)
    {
        object value = new();
        
        switch (node.Type)
        {
            case SYMBOLTYPE.INTEGER: value = (short)0; break;
            case SYMBOLTYPE.STRING: value = string.Empty; break;
        }

        foreach (string name in node.Identlist)
        {
            memory[name] = value;
        }
    }

    private void Visit(LetNode node)
    {
        memory[node.Ident] = Visit(node.Item);
    }

    private void Visit(InputNode node)
    {
        SYMBOLTYPE type = scope[node.Ident];

        switch (type)
        {
            case SYMBOLTYPE.INTEGER:
                memory[node.Ident] = ReadNumber();
                break;

            case SYMBOLTYPE.STRING:
                memory[node.Ident] = ReadString();
                break;

            default: break;
        }
    }

    private void Visit(PrintNode node)
    {
        string s = string.Empty;

        foreach (Ast ast in node.Itemlist)
        {
            s += Visit(ast);
        }

        Console.WriteLine(s);
    }

    private void Visit(IfNode node)
    {
        short left = (short)Visit(node.Left);
        
        short right = (short)Visit(node.Right);

        bool thenEnabled = false;

        switch (node.Relop)
        {
            case "<": thenEnabled = (left < right); break;
            case "<=": thenEnabled = (left <= right); break;
            case ">": thenEnabled = (left > right); break;
            case ">=": thenEnabled = (left >= right); break;
            case "==": thenEnabled = (left == right); break;
            case "!=": thenEnabled = (left != right); break;
        }

        if (thenEnabled) { Visit(node.Statement); }
    }

    private void Visit(GotoNode node)
    {
        for (int i = 0; i < root.Lines.Count; i++)
        {
            LineNode line = root.Lines[i];

            if (line.Number == node.Number) { pc = i; return; }
        }
    }

    private void Visit(GosubNode node)
    {
        for (int i = 0; i < root.Lines.Count; i++)
        {
            LineNode line = root.Lines[i];

            if (line.Number == node.Number)
            {
                stack.Push(pc);
                pc = i;
                return;
            }
        }
    }

    private void Visit(ReturnNode _)
    {
        if (stack.Count != 0) { pc = stack.Pop(); }
        else 
        {
            string msg = $"[Line {currentLine.Number}]Stack empty";

            throw new Exception(msg);
        }
    }

    private void Visit(EndNode _)
    {
        pc = -1;
    }

    private object Visit(BinaryNode node)
    {
        short left = (short)Visit(node.Left);

        short right = (short)Visit(node.Right);

        return node.Op switch
        {
            "+" => checked((short)(left + right)),
            "-" => checked((short)(left - right)),
            "*" => checked((short)(left * right)),
            "/" => checked((short)(left / right)),
            _ => new(),
        };
    }

    //return the value of the variable stored in memory as object
    private object Visit(IdentNode node)
    {
        return memory[node.Name];
    }

}
