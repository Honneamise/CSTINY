namespace CSTINY;

public class Analyzer
{
    private readonly Ast ast;
    private readonly List<short> lineNumbers;
    private short currentLine;

    public Analyzer(Ast ast)
    {
        this.ast = ast;
        lineNumbers = new();
    }

    public void Scan()
    {
        Visit((ProgramNode)ast);
    }

    private void Visit(Ast ast)
    {
        switch (ast)
        {
            case ProgramNode: Visit((ProgramNode)ast); break;
            case LineNode: Visit((LineNode)ast); break;
            case GotoNode: Visit((GotoNode)ast); break;
            case GosubNode: Visit((GosubNode)ast); break;

            default: break;
        };
    }

    private void Visit(ProgramNode node)
    {
        //for each line check the line number
        foreach (LineNode line in node.Lines)
        {
            currentLine = line.Number;

            //is duplicate ?
            if (lineNumbers.Contains(line.Number))
            {
                string msg = $"[Line {currentLine}]Duplicated line number found";
                throw new Exception(msg);
            }

            //is unordered ?
            if (lineNumbers.Count > 0 && line.Number < lineNumbers[^1])
            {
                string msg = $"[Line {currentLine}]Line number not progressive";
                throw new Exception(msg);
            }

            lineNumbers.Add(line.Number);
        }

        foreach (LineNode line in node.Lines)
        {
            Visit(line.Statement);
        }
    }

    private void Visit(GotoNode node)
    {
        if (!lineNumbers.Contains(node.Number))
        {
            string msg = $"[Line {currentLine}]Invalid jump found: {node.Number}";
            throw new Exception(msg);
        }
    }

    private void Visit(GosubNode node)
    {
        if (!lineNumbers.Contains(node.Number))
        {
            string msg = $"[Line {currentLine}]Invalid subroutine jump found: {node.Number}";
            throw new Exception(msg);
        }
    }

}
