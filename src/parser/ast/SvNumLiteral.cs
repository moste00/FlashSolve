namespace flashsolve.parser.ast; 

public class SvNumLiteral : SvPrimaryLiteral {
    private string number;

    public SvNumLiteral() { }

    public string Number {
        get {
            return number;
        }
        set {
            number = value;
        }
    }
}