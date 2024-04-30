namespace CSTINY;

public enum KEYWORD
{
    INTEGER,
    STRING,
    LET,
    INPUT,
    PRINT,
    IF,
    THEN,
    GOTO,
    GOSUB,
    RETURN,
    END
}

public enum TOKENTYPE
{
    COMMA,
    LPAREN,
    RPAREN,
    CR,
    EOF,
    OP,
    RELOP,
    ASSIGN,
    KEYWORD,
    NUMBER,
    STRING,
    IDENT
}

public class Token
{
    public TOKENTYPE Type { get; }
    public string Value { get; }
    public uint Row { get; }
    public uint Col { get; }

    public Token(TOKENTYPE type, uint row, uint col)
    {
        Type = type;
        Value = string.Empty;
        Row = row; 
        Col = col;
    }

    public Token(TOKENTYPE type, string value, uint row, uint col)
    {
        Type = type;
        Value = value;
        Row = row;
        Col = col;
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Value)) { return $"<{Type}>"; }

        return $"<{Type},\"{Value}\">"; 
    }
}
