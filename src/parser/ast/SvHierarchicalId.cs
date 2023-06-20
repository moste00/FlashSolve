namespace flashsolve.parser.ast; 

public class SvHierarchicalId {
    private List<string> _hierarchicalIds;

    public SvHierarchicalId() {
        _hierarchicalIds = new List<string>();
    }

    public void Add(string id) {
        _hierarchicalIds.Add(id);
    }
    public List<string> HierarchicalIds => _hierarchicalIds;
}