namespace flashsolve.parser.ast; 

public class SvConditionalExpression : SvExpr {
    private List<SvExpr> _exprs;
    private SvExpr then;
    private SvExpr els;

    public SvConditionalExpression(SvExpr then, SvExpr els) {
        Then = then;
        Else = els;
        _exprs = new List<SvExpr>();
    }

    public SvExpr Then {
        get {
            return then;
        }
        set {
            then = value;
        }
    }

    public SvExpr Else {
        get {
            return els;
        }
        set {
            els = value;
        }
    }

    public void Add(SvExpr expr) {
        _exprs.Add(expr);
    }
    
    /// <summary>
    /// Retrieves the list of conditional expressions.
    /// </summary>
    /// <returns>The list of conditional expressions.</returns>
    public List<SvExpr> ConditionalExpressions() {
        return _exprs;
    }
}