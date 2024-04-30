namespace CSTINY;

public class Symtab
{
    private readonly Ast ast;
    private readonly Scope scope;
    private short currentLine;

    public Symtab(Ast ast)
    {
        this.ast = ast;

        scope = new("GLOBAL", null);
    }

    public Scope Build()
    {
        Visit(ast);

        return scope;
    }

    private SYMBOLTYPE Visit(Ast ast)
    {
        switch(ast)
        {
            case ProgramNode: Visit((ProgramNode)ast); break;
            case LineNode: break;
            case DeclareNode: Visit((DeclareNode)ast); break;
            case LetNode: Visit((LetNode)ast); break;
            case InputNode: Visit((InputNode)ast); break;
            case PrintNode: Visit((PrintNode)ast); break;
            case IfNode: Visit((IfNode)ast); break;
            case GotoNode: break;
            case GosubNode: break;
            case ReturnNode: break;
            case EndNode: break;
            case BinaryNode: return Visit((BinaryNode)ast);
            case IdentNode: return Visit((IdentNode)ast);
            case NumberNode: return SYMBOLTYPE.INTEGER;
            case StringNode: return SYMBOLTYPE.STRING;

            default: break;
        };

        return SYMBOLTYPE.VOID;
    }

    private void Visit(ProgramNode node)
    {
        foreach (LineNode line in node.Lines)
        {
            currentLine = line.Number;
            Visit(line.Statement);
        }
    }

    private void Visit(DeclareNode node)
    {
        //define variables
        foreach (string name in node.Identlist)
        {
            Symbol? s = scope.Resolve(name);

            if (s != null)
            {
                string msg = $"[Line {currentLine}]Found already declared variable: {name}\n";
                throw new Exception(msg);
            }

            s = new(name, node.Type);

            scope.Define(s);
        }
    }

    private void Visit(LetNode node)
    {
        //reference 
        Symbol? symbol = scope.Resolve(node.Ident);

        if (symbol == null)
        {
            string msg = $"[Line {currentLine}]Found undeclared variable: {node.Ident}\n";
            throw new Exception(msg);
        }

        SYMBOLTYPE itemType = Visit(node.Item);

        if (symbol.Type != itemType)
        {
            string msg = $"[Line {currentLine}]Cannot assign {itemType} to {symbol.Type}\n";
            throw new Exception(msg);
        }
    }

    private void Visit(InputNode node)
    {
        //reference 
        Symbol? symbol = scope.Resolve(node.Ident);

        if (symbol == null)
        {
            string msg = $"[Line {currentLine}]Found undeclared variable: {node.Ident}\n";
            throw new Exception(msg);
        }
    }

    private void Visit(PrintNode node)
    {
        foreach (Ast ast in node.Itemlist)
        {
            Visit(ast);
        }
    }

    private void Visit(IfNode node)
    {
        SYMBOLTYPE leftType = Visit(node.Left);
        SYMBOLTYPE rightType = Visit(node.Right);

        if (leftType != rightType)
        {
            string msg = $"[Line {currentLine}]Cannot compare {leftType} with {rightType}\n";
            throw new Exception(msg);
        }

        Visit(node.Statement);
    }

    private SYMBOLTYPE Visit(BinaryNode node)
    {
        SYMBOLTYPE leftType = Visit(node.Left);

        SYMBOLTYPE rightType = Visit(node.Right);

        if (leftType != SYMBOLTYPE.INTEGER || rightType != SYMBOLTYPE.INTEGER)
        {
            string msg = $"[Line {currentLine}]Invalid arithmetic operand types: {leftType} vs {rightType}\n";
            throw new Exception(msg);
        }

        return SYMBOLTYPE.INTEGER;
    }

    private SYMBOLTYPE Visit(IdentNode node)
    {
        //reference ident 
        Symbol? symbol = scope.Resolve(node.Name);

        if (symbol == null)
        {
            string msg = $"[Line {currentLine}]Found undeclared variable: {node.Name}\n";
            throw new Exception(msg);
        }

        return symbol.Type;
    }

}
