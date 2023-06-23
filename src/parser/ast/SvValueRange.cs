namespace flashsolve.parser.ast; 

public class SvValueRange : Tuple<SvExpr, SvExpr?>, SvAstNode {
    public SvValueRange(SvExpr item1, SvExpr? item2) : base(item1, item2) { }
}