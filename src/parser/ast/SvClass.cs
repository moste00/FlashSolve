namespace flashsolve.parser.ast;

public class SvClass {
    private CstRef cstnode;
    private List<SvBitData> _members;
    private List<SvConstraint> _constraints;

    public SvClass(CstRef cstnode) {
        this.cstnode = cstnode;
        _members = new List<SvBitData>();
        _constraints = new List<SvConstraint>();
    }

    public void AddMember(SvBitData membr) => _members.Add(membr);
    public void AddConstraint(SvConstraint constr) => _constraints.Add(constr);
}