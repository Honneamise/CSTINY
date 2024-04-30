namespace CSTINY;

public class Scope
{
    public string Name { get; }
    public Scope? Parent { get; }
    public Dictionary<string,Symbol> Symbols { get; }

    public SYMBOLTYPE this[string name] => Symbols[name.ToUpper()].Type;

    public Scope(string name, Scope? parent)
    {
        Name = name;
        Parent = parent;
        Symbols = new();
    }

    public override string ToString()
    {
        string s = $"--{Name} symbols--:";

        foreach (var item in Symbols)
        {
            s += "\n" + item.Value;
        }

        return s;
    }

    public void Define(Symbol symbol)
    {
        Symbols[symbol.Name.ToUpper()] = symbol;
    }

    public Symbol? Resolve(string name) 
    {
        if(Symbols.ContainsKey(name.ToUpper())) return Symbols[name.ToUpper()];

        if(Parent != null) return Parent.Resolve(name.ToUpper());

        return null;
    }
}
