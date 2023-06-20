namespace flashsolve.parser.ast;

public class SvIfElse : SvConstraint.Expr
{
    
    private SvExpr expr;
    private SvConstraintSet _then;
    private SvConstraintSet? _else;
    public SvIfElse(SvExpr expr) {
        Expr = expr;
        _else = null;
    }

    public SvExpr Expr
    {
        get
        {
            return expr;
        }
        set
        {
            expr = value;
        }
    }

    public SvConstraintSet Then {
        get {
            return _then;
        }
        set {
            _then = value;
        }
    }

    public SvConstraintSet Else {
        get {
            return _else;
        }
        set {
            _else = value;
        }
    }
}