namespace flashsolve.parser.ast;

public class SvUniqueness : SvConstraint.Expr
{
    private List<Tuple<SvExpr,SvExpr?>> _expr;
    
    public SvUniqueness()
    {
        _expr = new List<Tuple<SvExpr,SvExpr?>>();
    }
    
    public void Add(Tuple<SvExpr,SvExpr?> record)
    {
        _expr.Add(record);
    }
}