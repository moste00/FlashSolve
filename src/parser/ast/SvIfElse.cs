namespace flashsolve.parser.ast;

public class SvIfElse : SvConstraint.Expr
{
    private SvExpr expr;
    private List<SvConstraint.Expr> _then;
    private List<SvConstraint.Expr> _else;
    public SvIfElse(SvExpr expr)
    {
        Expr = expr;
        _then = new List<SvConstraint.Expr>();
        _else = new List<SvConstraint.Expr>();
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

    public void AddThen(SvConstraint.Expr thenConstraintExpr)
    {
        _then.Add(thenConstraintExpr);
    }
    public void AddElse(SvConstraint.Expr elseConstraintExpr)
    {
        _else.Add(elseConstraintExpr);
    }
    
}