namespace CSTINY;

public enum SYMBOLTYPE
{
    VOID,
    INTEGER,
    STRING
}

public class Symbol
{
    public string Name { get; }
    public SYMBOLTYPE Type { get; }

    public Symbol(string name, SYMBOLTYPE type)
    {
        Name = name;
        Type = type;
    }

    public override string ToString()
    {
        return $"({Name}:{Type})";
    }
}
