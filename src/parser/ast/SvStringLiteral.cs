namespace flashsolve.parser.ast; 

public class SvStringLiteral : SvLiteral {
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