namespace FlashSolve.parser.ast;

public class SvConstraint {
    private CstRef cstnode;
    private List<SvExpr> _items;

    public SvConstraint(CstRef cstnode) {
        this.cstnode = cstnode;
        _items = new List<SvExpr>();
    }

    public void AddItems(SvExpr itm) => _items.Add(itm);
}