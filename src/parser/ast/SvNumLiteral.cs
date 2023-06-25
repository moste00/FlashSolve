namespace flashsolve.parser.ast; 

public class SvNumLiteral : SvLiteral {
    private string _number;

    public SvNumLiteral() { }

    public string Number {
        get {
            return _number;
        }
        set {
            _number = value;
        }
    }
}