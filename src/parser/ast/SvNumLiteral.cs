namespace flashsolve.parser.ast; 

public class SvNumLiteral : SvLiteral {
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