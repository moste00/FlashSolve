namespace flashsolve.parser.ast; 

public class SvIncOrDecExpr : SvExpr {
    private SvVariableLvalue variableId;
    private bool isPrefix;

    public SvIncOrDecExpr(SvVariableLvalue variableId, bool isPrefix) {
        VariableId = variableId;
        IsPrefix = isPrefix;
    }

    public SvVariableLvalue VariableId {
        get {
            return variableId;
        }
        set {
            variableId = value;
        }
    }

    public bool IsPrefix {
        get {
            return isPrefix;
        }
        set {
            isPrefix = value;
        }
    }
}