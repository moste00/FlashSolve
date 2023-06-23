namespace flashsolve.parser.ast; 

public class SvInsideExpression : SvExpr {
    private SvOpenRange openRange;
    private SvExpr expr;

    public SvInsideExpression(SvExpr expr) {
        Expr = expr;
    }

    public SvExpr Expr {
        get {
            return expr;
        }
        set {
            expr = value;
        }
    }

    public SvOpenRange OpenRange {
        get {
            return openRange;
        }
        set {
            openRange = value;
        }
    }
}