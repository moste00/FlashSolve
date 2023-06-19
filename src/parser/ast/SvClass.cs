namespace flashsolve.parser.ast;

public class SvClass : SvAstRoot {
    private CstRef cstnode;
    private List<SvBitData> _members;
    private List<SvConstraint> _constraints;

    public SvClass(CstRef cstnode) {
        this.cstnode = cstnode;
        _members = new List<SvBitData>();
        _constraints = new List<SvConstraint>();
    }

    public void Add(SvBitData membr) => _members.Add(membr);
    public void Add(SvConstraint constr) => _constraints.Add(constr);

    public List<SvConstraint> Constraints => _constraints;
    public List<SvBitData> Members => _members;
    public string Name => cstnode.Name;
}