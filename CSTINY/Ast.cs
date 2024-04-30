namespace CSTINY;

public class Ast
{
    public Ast() { }

    public override string ToString()
    {
        string s = "{" + GetType().Name + "}";

        return s;
    }
}

public class ProgramNode : Ast
{
    public List<LineNode> Lines { get; }

    public ProgramNode(List<LineNode> lines) : base()
    {
        Lines = lines;
    }
}

public class LineNode : Ast
{
    public short Number { get; }
    public Ast Statement { get; }

    public LineNode(short number, Ast statement) : base()
    {
        Number = number;
        Statement = statement;
    }
}

public class DeclareNode : Ast
{
    public SYMBOLTYPE Type { get; }
    public List<string> Identlist { get; }
    
    public DeclareNode (SYMBOLTYPE type, List<string> identlist) : base()
    {
        Type = type;

        Identlist = new();

        foreach (string s in identlist)
        {
            Identlist.Add(s);
        }
    }
}

public class LetNode : Ast
{
    public string Ident { get; }
    public Ast Item { get; }

    public LetNode(string ident, Ast item) : base()
    {
        Ident = ident;
        Item = item;
    }
}

public class InputNode : Ast
{
    public string Ident { get; }

    public InputNode(string ident) : base()
    {
        Ident = ident;
    }
}

public class PrintNode : Ast
{
    public List<Ast> Itemlist { get; }

    public PrintNode(List<Ast> itemlist) : base()
    {
        Itemlist = itemlist;
    }
}

public class IfNode : Ast
{
    public Ast Left { get; }
    public string Relop { get; }
    public Ast Right { get; }
    public Ast Statement { get; }

    public IfNode(Ast left, string relop, Ast right, Ast statement) : base()
    { 
        Left = left;
        Relop = relop;
        Right = right;
        Statement = statement;
    }
}

public class GotoNode : Ast
{
    public short Number { get; }

    public GotoNode(short number) : base()
    {
        Number = number;
    }
}

public class GosubNode : Ast
{
    public short Number { get; }

    public GosubNode(short number) : base()
    {
        Number = number;
    }
}

public class ReturnNode : Ast
{
    public ReturnNode() : base() { }
}

public class EndNode : Ast
{
    public EndNode() : base() { }
}

public class BinaryNode : Ast
{
    public Ast Left { get; }
    public string Op { get; }
    public Ast Right { get; }

    public BinaryNode(Ast left, string op, Ast right) : base()
    {
        Left = left;
        Op = op;
        Right = right;
    }
}

public class IdentNode : Ast
{
    public string Name { get; }

    public IdentNode(string name) : base()
    { 
        Name = name;
    }
}

public class NumberNode : Ast
{
    public short Value { get; }

    public NumberNode(short value) : base()
    { 
        Value = value;
    }
}

public class StringNode : Ast
{
    public string Value { get; }

    public StringNode(string value) : base()
    { 
        Value = value;
    }
}


