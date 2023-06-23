namespace flashsolve.parser.ast;

public class SvUniqueness : SvConstraint.Expr
{
    private SvOpenRange openRange;
    
    public SvUniqueness(SvOpenRange openRange) {
        OpenRange = openRange;
    }
    public SvOpenRange OpenRange {
        get;
        set;
    }
}