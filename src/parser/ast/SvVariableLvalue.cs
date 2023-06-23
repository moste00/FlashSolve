namespace flashsolve.parser.ast; 

public class SvVariableLvalue : SvAstNode {
    private string id;

    public String Id {
        get {
            return id;
        }
        set {
            id = value;
        }
    }
}