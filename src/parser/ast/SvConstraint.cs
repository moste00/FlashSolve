namespace flashsolve.parser.ast;

public class SvConstraint : SvAstNode {
    public interface BlockItem : SvAstNode {}
    public interface Expr : BlockItem {}

    public record Block(List<SvConstraint.BlockItem> items) : SvAstNode;


    private CstRef cstnode;
    private SvConstraint.Block _items;

    public SvConstraint(CstRef cstnode) {
        this.cstnode = cstnode;
        _items = null;
    }

    public SvConstraint.Block Items {
            get => _items;
            set => _items = value;
    }
}
