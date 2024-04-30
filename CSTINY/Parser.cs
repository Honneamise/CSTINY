namespace CSTINY;

public class Parser
{
    private readonly Lexer lexer;
    private Token token;

    public Parser(Lexer lexer)
    {
        this.lexer = lexer;
        token = lexer.Next();
    }

    private void Consume()
    {
        token = lexer.Next();
    }

    private void Match(TOKENTYPE type)
    {
        if (token.Type == type) { Consume(); }
        else
        {
            string msg = $"[{token.Row},{token.Col}]Expected: <{type}> found: {token}";
            throw new Exception(msg);
        }
    }

    private void Match(TOKENTYPE type, string value)
    {
        if (token.Type == type && token.Value.ToUpper().Equals(value)) { Consume(); }
        else
        {
            string msg = $"[{token.Row},{token.Col}]Expected: <{type},\"{value}\"> found: {token}";
            throw new Exception(msg);
        }
    }

    public Ast Parse()
    {
        return Program();
    }

    private ProgramNode Program()
    {
        List<LineNode> lines = new();

        while (token.Type != TOKENTYPE.EOF)
        {
            if (token.Type == TOKENTYPE.NUMBER)
            {
                lines.Add(Line());
            }

            if(token.Type == TOKENTYPE.EOF) { break; }

            Match(TOKENTYPE.CR);
        }

        return new ProgramNode(lines);
    }

    private LineNode Line()
    {
        short number = Number();

        Ast statement = Statement();

        return new LineNode(number, statement);
    }

    private Ast Statement()
    {
        Token t = token;
        string msg;

        Match(TOKENTYPE.KEYWORD);

        switch (t.Value.ToUpper())
        {
            case "INTEGER":
                return new DeclareNode(SYMBOLTYPE.INTEGER, Identlist());

            case "STRING":
                return new DeclareNode(SYMBOLTYPE.STRING, Identlist());

            case "LET":
                Token ident = token;
                Match(TOKENTYPE.IDENT);
                Match(TOKENTYPE.ASSIGN);
                Ast item = Item();
                return new LetNode(ident.Value, item);

            case "INPUT":
                Token id = token;
                Match(TOKENTYPE.IDENT);
                return new InputNode(id.Value);

            case "PRINT":
                return new PrintNode(Itemlist());
                
            case "IF":
                Ast left = Expr();
                Token relop = token;
                Match(TOKENTYPE.RELOP);
                Ast right = Expr();
                Match(TOKENTYPE.KEYWORD, "THEN");
                Ast statement = Statement();
                return new IfNode(left, relop.Value, right, statement);

            case "GOTO":
                return new GotoNode(Number());

            case "GOSUB":
                return new GosubNode(Number());

            case "RETURN":
                return new ReturnNode();

            case "END":
                return new EndNode();

            default:
                msg = $"[{t.Row},{t.Col}]Unexpected token found: {t}";
                throw new Exception(msg);
        }
    }

    private Ast Item()
    {
        Token t = token;

        switch (t.Type)
        {                
            case TOKENTYPE.STRING:
                Match(TOKENTYPE.STRING);
                return new StringNode(t.Value);

            default: return Expr();
        }
    }

    private List<Ast> Itemlist()
    {
        List<Ast> list = new() { Item() };

        while (token.Type == TOKENTYPE.COMMA)
        {
            Match(TOKENTYPE.COMMA);

            list.Add(Item());
        }

        return list;
    }

    private List<string> Identlist()
    {
        List<string> varlist = new();

        Token t = token;
        Match(TOKENTYPE.IDENT);
        varlist.Add(t.Value);

        while (token.Type == TOKENTYPE.COMMA)
        {
            Match(TOKENTYPE.COMMA);

            t = token;
            Match(TOKENTYPE.IDENT);
            varlist.Add(t.Value);
        }

        return varlist;
    }

    private short Number()
    { 
        Token t = token;

        Match(TOKENTYPE.NUMBER);

        if (!short.TryParse(t.Value, out short number))
        {
            string msg = $"[{t.Row},{t.Col}]Invalid number format: {t.Value}";
            throw new Exception(msg);
        }

        return number;
    }

    //for signed expressions we add a new binary node as : (0,op,term)
    public Ast Expr()
    {
        Ast node;
        bool negate = false;

        if (token.Type == TOKENTYPE.OP && token.Value == "+") 
        { Match(TOKENTYPE.OP); }

        if (token.Type == TOKENTYPE.OP && token.Value == "-")
        {
            Match(TOKENTYPE.OP);
            negate = true;
        }

        if (negate==true) { node = new BinaryNode( new NumberNode(0), "-", Term()); }
        else { node = Term(); }

        while (token.Type == TOKENTYPE.OP && (token.Value.Equals("+") || token.Value.Equals("-")))
        {
            Token op = token;

            Match(TOKENTYPE.OP);
            
            node = new BinaryNode(node, op.Value, Term());
        }

        return node;
    }

    public Ast Term()
    {
        Ast node = Factor();

        while (token.Type == TOKENTYPE.OP && (token.Value.Equals("*") || token.Value.Equals("/")))
        {
            Token op = token;

            Match(TOKENTYPE.OP);
            
            node = new BinaryNode(node, op.Value, Factor());
        }

        return node;
    }

    public Ast Factor()
    {
        Token t = token;
        string msg;

        switch (token.Type)
        {
            case TOKENTYPE.IDENT:
                Match(TOKENTYPE.IDENT);
                return new IdentNode(t.Value);

            case TOKENTYPE.NUMBER:
                return new NumberNode(Number());

            case TOKENTYPE.LPAREN:
                Match(TOKENTYPE.LPAREN);
                Ast node = Expr();
                Match(TOKENTYPE.RPAREN);
                return node;

            default:
                msg = $"[{t.Row},{t.Col}]Unexpected token found: {t}";
                throw new Exception(msg);
        }
    }
}
