namespace flashsolve.parser.ast;

public class SvImplication : SvConstraint.Expr
{
    private SvExpr expr;
    private List<SvConstraint.Expr> _constraintSet;

    public SvImplication(SvExpr expr)
    {
        Expr = expr;
        _constraintSet = new List<SvConstraint.Expr>();
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

    public void Add(SvConstraint.Expr constraintExpr)
    {
        _constraintSet.Add(constraintExpr);
    }
}