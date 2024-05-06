using System.Net.NetworkInformation;

namespace CSTINY;

public class Nasmer
{
    private readonly int STRING_SIZE = 32;
    private readonly string STRING_BUFFER = "STRING_BUFFER";

    private readonly Ast ast;
    private readonly Scope scope;
    private readonly Dictionary<string,string> pool;//inverted key/value

    public Nasmer(Ast ast, Scope scope)
    {
        this.ast = ast;
        this.scope = scope;
        pool = new();
    }

    public string Generate()
    {
        //headers
        string str = NasmerUtils.Headers;

        //main code generation
        str += Visit((ProgramNode)ast) + "\n";

        //guard for pc out of range
        str += "jmp PC_OUT_OF_RANGE\n\n";

        //variables
        foreach (Symbol symbol in scope.Symbols.Values)
        {
            if (symbol.Type == SYMBOLTYPE.INTEGER)
            {
                str += $"{symbol.Name}: DW 0000h\n";
            }
            else if (symbol.Type == SYMBOLTYPE.STRING)
            {
                str += $"{symbol.Name}: TIMES {STRING_SIZE} DB 00h\n";
            }
        }
        str += "\n";

        //const pool
        foreach (var item in pool)
        {
            str += $"{item.Value}: DB \"{item.Key}\",00h\n";
        }
        str += "\n";

        //builtin buffer
        str += $"{STRING_BUFFER}: TIMES {STRING_SIZE} DB 00h\n";

        //builtin functions
        str += NasmerUtils.BuiltinFunctions;

        return str;
    }

    private string Visit(Ast ast)
    {
        switch (ast)
        {
            case ProgramNode: return Visit((ProgramNode)ast);
            case LineNode: return Visit((LineNode)ast);             
            case DeclareNode: return string.Empty;
            case LetNode: return Visit((LetNode)ast);
            case InputNode: return Visit((InputNode)ast);
            case PrintNode: return Visit((PrintNode)ast);
            case IfNode: return Visit((IfNode)ast);
            case GotoNode: return Visit((GotoNode)ast);
            case GosubNode: return Visit((GosubNode)ast);
            case ReturnNode: return Visit((ReturnNode)ast);
            case EndNode: return Visit((EndNode)ast);
            case BinaryNode: return Visit((BinaryNode)ast);
            case IdentNode: return Visit((IdentNode)ast);
            case NumberNode: return Visit((NumberNode)ast);
            case StringNode: return Visit((StringNode)ast);
        };

        return string.Empty;
    }

    private string Visit(ProgramNode node)
    {
        string str = string.Empty;

        foreach (LineNode Line in node.Lines)
        {
            str += Visit(Line);
        }

        return str;
    }

    private string Visit(LineNode node)
    {
        string str = $"\nLine_{node.Number}:\n";

        return str + Visit(node.Statement) + ".Line_end:\n"; ;
    }

    private string Visit(LetNode node)
    {
        string str = string.Empty;

        switch(node.Item)
        {
            case IdentNode:
                SYMBOLTYPE type = scope[node.Ident];

                if (type == SYMBOLTYPE.INTEGER)
                {
                    str += $"\tmov ax,[{((IdentNode)node.Item).Name}]\n";
                    str += $"\tmov [{node.Ident}],ax\n";
                }
                else if (type == SYMBOLTYPE.STRING)
                {
                    str += $"\tmov si,{((IdentNode)node.Item).Name}\n";
                    str += $"\tmov di,{node.Ident}\n";
                    str += $"\tmov cx,{STRING_SIZE}\n";
                    str += "\trep movsb\n";
                }
                break; ;

            case NumberNode:
                str = $"\tmov [{node.Ident}],word {((NumberNode)node.Item).Value}\n";
                break;

            case StringNode:
                str += $"\tmov si,{Visit((StringNode)node.Item)}\n";
                str += $"\tmov di,{node.Ident}\n";
                str += $"\tmov cx,{STRING_SIZE}\n";
                str += "\trep movsb\n";
                break;

            case BinaryNode:
                str += Visit((BinaryNode)node.Item);
                str += $"\tpop word [{node.Ident}]\n";
                break;
        }

        return str;
    }

    private string Visit(InputNode node)
    {
        string str = string.Empty;

        SYMBOLTYPE type = scope[node.Ident];

        if(type == SYMBOLTYPE.INTEGER)
        {
            str += ".read_number:\n";

            str += $"\tmov cx,{STRING_SIZE}\n";
            str += $"\tmov dx,{STRING_BUFFER}\n";
            str += "\tcall READ_STRING\n";
            str += "\tcall NEW_LINE\n";

            str += $"\tmov cx,{node.Ident}\n";
            str += $"\tmov dx,{STRING_BUFFER}\n";
            str += "\tcall STRING_TO_NUMBER\n";

            str += "\tcmp al,0\n";
            str += "\tje .read_number\n";
        }
        else if (type == SYMBOLTYPE.STRING)
        {
            str += $"\tmov cx,{STRING_SIZE}\n";
            str += $"\tmov dx,{node.Ident}\n";
            str += "\tcall READ_STRING\n";
            str += "\tcall NEW_LINE\n";
        }

        return str;
    }

    private string Visit(PrintNode node)
    {
        string str = string.Empty;

        foreach (Ast ast in node.Itemlist)
        {
            switch(ast)
            {
                case IdentNode:
                    SYMBOLTYPE type = scope[((IdentNode)ast).Name];
                    if (type == SYMBOLTYPE.INTEGER)
                    {
                        str += $"\tmov dx,[{((IdentNode)ast).Name}]\n";
                        str += "\tcall PRINT_NUMBER\n";
                    }
                    else if (type == SYMBOLTYPE.STRING)
                    {
                        str += $"\tmov dx,{((IdentNode)ast).Name}\n";
                        str += "\tcall PRINT_STRING\n";
                    }
                    break; ;

                case NumberNode:
                    str = $"\tmov dx,{((NumberNode)ast).Value}\n";
                    str += "\tcall PRINT_NUMBER\n";
                    break;

                case StringNode:
                    str += $"\tmov dx,{Visit((StringNode)ast)}\n";
                    str += "\tcall PRINT_STRING\n";
                    break;

                case BinaryNode:
                    str += Visit((BinaryNode)ast);
                    str += "\tpop dx\n";
                    str += "\tcall PRINT_NUMBER\n";
                    break;
            }
        }

        //new Line at end
        str += "\tcall NEW_LINE\n";

        return str;
    }

    private string Visit(IfNode node)
    {
        string str = string.Empty;

        str += Visit(node.Left);

        str += Visit(node.Right);

        str += "\tpop bx\n";
        str += "\tpop ax\n";
        str += "\tcmp ax,bx\n";

        switch (node.Relop)
        {
            case "<": str += "\tjnl .Line_end\n"; break;
            case "<=": str += "\tjnle .Line_end\n"; break;
            case ">": str += "\tjng .Line_end\n"; break;
            case ">=": str += "\tjnge .Line_end\n"; break;
            case "==": str += "\tjne .Line_end\n"; break;
            case "<>" or "!=": str += "\tje .Line_end\n"; break;
        }

        str += Visit(node.Statement);

        return str;
    }

    private static string Visit(GotoNode node)
    {
        return $"\tjmp Line_{node.Number}\n";
    }

    private static string Visit(GosubNode node)
    {
        return $"\tcall Line_{node.Number}\n";
    }

    private static string Visit(ReturnNode _)
    {
        return "\tret\n";
    }

    private static string Visit(EndNode _)
    {
        return "\tcall TERMINATE_PROGRAM\n";
    }

    private string Visit(BinaryNode node)
    {
        string str = string.Empty;

        str += Visit(node.Left);

        str += Visit(node.Right);

        str += "\tpop bx\n";
        str += "\tpop ax\n";

        switch (node.Op)
        {
            case "+": str += "\tadd ax,bx\n"; break;
            case "-": str += "\tsub ax,bx\n"; break;
            case "*": str += "\timul bx\n"; break;

            case "/": str += "\txor dx,dx\n";
                      str += "\tcwd\n";
                      str += "\tcmp bx,00h\n";
                      str += "\tje DIVISION_BY_ZERO\n";
                      str += "\tidiv bx\n"; 
                      break;
        }

        str += "\tjo ARITHEMETIC_OVERFLOW\n";

        str += "\tpush ax\n";

        return str;
    }

    //push ident value on the stack
    private string Visit(IdentNode node)
    {
        string str = string.Empty;

        SYMBOLTYPE type = scope[node.Name];

        if (type == SYMBOLTYPE.INTEGER)
        {
            str = $"\tmov ax,[{node.Name}]\n";
        }
        else if (type == SYMBOLTYPE.STRING)
        {
            str = $"\tmov ax,{node.Name}\n";
        }

        str += "\tpush ax\n";

        return str;
    }

    //push the number on the stack
    private static string Visit(NumberNode node)
    {
        string str = $"\tmov ax,{node.Value}\n";

        str += "\tpush ax\n";

        return str;
    }

    //return the address of the string
    private string Visit(StringNode node)
    {
        if (!pool.ContainsKey(node.Value))
        {
            string name = "Pool_" + pool.Count;

            pool[node.Value] = name;
        }

        return pool[node.Value];
    }
}
