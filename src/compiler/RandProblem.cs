namespace flashsolve.compiler; 

using Microsoft.Z3;

public class RandProblem {
    //the main store for constraints 
    private List<BoolExpr> _constraints;
    //for each variables, what constraints involve it ? and what Z3 object represent it ?
    //constraints are referenced by their z3 IDs
    private Dictionary<string,
                       (HashSet<uint>,BitVecExpr)> _vars;
    //maps z3 identifiers to indices into constraints
    //also serves as a quick-lookup set to detect duplicate constraints
    private Dictionary<uint, int> _idsToIndices;

    private Context _generatingCtx;

    public RandProblem() {
        _constraints = new List<BoolExpr>();
        _vars = new Dictionary<string, (HashSet<uint>,BitVecExpr)>();
        _idsToIndices = new Dictionary<uint, int>();
    }

    public Context Context {
        get => _generatingCtx;
        set => _generatingCtx = value;
    }
    public BoolExpr[] Constraints => _constraints.ToArray();

    public Dictionary<string, BitVecExpr> Vars => _vars.ToDictionary(
        (entry) => entry.Key,
        (entry) => entry.Value.Item2
    );
    public void Merge(RandProblem other) {
        foreach (var constraint in other._constraints) {
            AddConstraint(constraint);
        }

        foreach (var entry in other._vars) {
            string var = entry.Key;
            var (ids, z3Expr) = entry.Value;
            AddVar(var,z3Expr);
            
            foreach (var id in ids) {
                int index = other._idsToIndices[id];
                var constraint = other._constraints[index];
                AssociateConstraintWithVar(constraint,var);
            }
        }
    }
    public void AddConstraint(BoolExpr constraint) {
        if (!_idsToIndices.ContainsKey(constraint.Id)) {
            _constraints.Add(constraint);
            _idsToIndices[constraint.Id] = _constraints.Count - 1;
        }
    }
    public void AddVar(string var, BitVecExpr e) {
        if (!_vars.ContainsKey(var)) {
            _vars[var] = (new HashSet<uint>(),e);
        }
    }
    public void AssociateConstraintWithVar(BoolExpr constraint, string var) {
        _vars[var].Item1.Add(constraint.Id);
    }

    public BitVecExpr LookupVar(string v) {
        return _vars[v].Item2;
    }
}