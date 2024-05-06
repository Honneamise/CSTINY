namespace CSTINY;

public class Lexer
{
    private const char EOF = (char)0;

    private readonly string text;
    private int pos;
    private uint row;
    private uint col;
    private char c;

    public Lexer(string text)
    {
        this.text = text;
        pos = 0;
        row = 1;
        col = 1;
        c = text[pos];
    }

    private void Comment()
    {
        while (c != '\r' && c!=EOF)
        {
            Consume();
        }
    }

    private string Number()
    {  
        string str = string.Empty;

        while(char.IsNumber(c))
        {
            str += c;
            Consume();
        }

        return str; 
    }

    private string Ident()
    {
        string ident = string.Empty;

        while (c != EOF && char.IsLetterOrDigit(c))
        {
            ident += c;
            Consume();
        }

        return ident;
    }

    private string Str()
    {
        string str = string.Empty;

        while (c != EOF && c != '"')
        {
            str += c;
            Consume();
        }

        return str;
    }

    private void Consume()
    {
        pos++;
        col++;

        if (c == '\n') { row++; col = 1; }

        if (pos >= text.Length) { c = EOF; }
        else { c = text[pos]; }
    }

    private void Match(char c)
    {
        if (this.c == c) { Consume(); }
        else
        { 
            string msg = $"[{row}:{col}]Expected: ({c}) found: ({this.c})"; 
            throw new Exception(msg); 
        }
    }

    public Token Next()
    {
        while (c != EOF)
        {
            uint rw = row;
            uint cl = col;

            switch(c) 
            {
                case '#':
                    Comment();
                    continue;

                case ' ' or '\t': 
                    Consume(); 
                    continue;

                case ',':
                    Consume();
                    return new Token(TOKENTYPE.COMMA, rw, cl);

                case '(':
                    Consume();
                    return new Token(TOKENTYPE.LPAREN, rw, cl);

                case ')':
                    Consume();
                    return new Token(TOKENTYPE.RPAREN, rw, cl);

                case '\r':
                    Consume();
                    Match('\n');
                    return new Token(TOKENTYPE.CR, rw, cl);

                case '+' or '-' or '*' or '/':
                    string s = c.ToString();
                    Consume();
                    return new Token(TOKENTYPE.OP, s, rw, cl);

                case '=':
                    Consume();
                    try { Match('='); return new Token(TOKENTYPE.RELOP, "==", rw, cl); }
                    catch (Exception) { return new Token(TOKENTYPE.ASSIGN, rw, cl); }

                case '<':
                    Consume();
                    try { Match('>'); return new Token(TOKENTYPE.RELOP, "<>", rw, cl); } catch { }
                    try { Match('='); return new Token(TOKENTYPE.RELOP, "<=", rw, cl); }
                    catch (Exception) { return new Token(TOKENTYPE.RELOP, "<", rw, cl); }

                case '>':
                    Consume();
                    try { Match('='); return new Token(TOKENTYPE.RELOP, ">=", rw, cl); }
                    catch (Exception) { return new Token(TOKENTYPE.RELOP, ">", rw, cl); }

                case '!':
                    Consume();
                    Match('=');
                    return new Token(TOKENTYPE.RELOP, "!=", rw, cl);

                case '"':
                    Consume();
                    string str = Str();
                    Match('"');
                    return new Token(TOKENTYPE.STRING, str, rw, cl);

                default:
                    if(char.IsDigit(c)) { return new Token(TOKENTYPE.NUMBER, Number(), rw, cl); }

                    if (char.IsLetter(c))
                    {
                        string ident = Ident();

                        if(Enum.GetNames(typeof(KEYWORD)).Contains(ident.ToUpper()))
                        { 
                            return new Token(TOKENTYPE.KEYWORD, ident, rw, cl); 
                        }
                        else 
                        { 
                            return new Token(TOKENTYPE.IDENT, ident, rw, cl); 
                        }
                    }

                    string msg = $"[{row}:{col}]Invalid character found: ({c})";
                    throw new Exception(msg);
            }
        }

        return new Token(TOKENTYPE.EOF, row, col);
    }

}
