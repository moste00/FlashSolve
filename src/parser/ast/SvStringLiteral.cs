namespace flashsolve.parser.ast; 

public class SvStringLiteral : SvPrimaryLiteral {
    private string stringLiteral;
    public SvStringLiteral() { }

    public string StringLiteral {
        get {
            return stringLiteral;
        }
        set {
            stringLiteral = value;
        }
    }
}