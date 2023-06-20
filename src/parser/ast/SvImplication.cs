namespace flashsolve.parser.ast;

public class SvImplication : SvConstraint.Expr {
    private SvExpr expr;
    private SvConstraintSet _constraintSet;

    public SvImplication(SvExpr expr) {
        Expr = expr;
        _constraintSet = new SvConstraintSet();
    }

    public SvExpr Expr {
        get { return expr; }
        set { expr = value; }
    }

    public SvConstraintSet ConstraintSet {
        get { return _constraintSet; }
        set { _constraintSet = value; }
    }
}