namespace flashsolve.compiler; 

using Microsoft.Z3;

public class RandProblem {
    //the main store for constraints 
    private List<BoolExpr> constraints;
    //for each variables, what constraints involve it ?
    //constraints are referenced by their z3 IDs
    private Dictionary<string,HashSet<uint>> vars;
    //maps z3 identifiers to indices into constraints
    //also serves as a quick-lookup set to detect duplicate constraints
    private Dictionary<uint, int> idsToIndices;

    public RandProblem() {
        constraints = new List<BoolExpr>();
        vars = new Dictionary<string, HashSet<uint>>();
        idsToIndices = new Dictionary<uint, int>();
    }
    public void Merge(RandProblem other) {
        foreach (var constraint in other.constraints) {
            AddConstraint(constraint);
        }

        foreach (var entry in other.vars) {
            string var = entry.Key;
            AddVar(var);
            
            foreach (var id in entry.Value) {
                int index = other.idsToIndices[id];
                var constraint = other.constraints[index];
                AssociateConstraintWithVar(constraint,var);
            }
        }
    }
    public void AddConstraint(BoolExpr constraint) {
        if (!idsToIndices.ContainsKey(constraint.Id)) {
            constraints.Add(constraint);
            idsToIndices[constraint.Id] = constraints.Count - 1;
        }
    }
    public void AddVar(string var) {
        if (!vars.ContainsKey(var)) {
            vars[var] = new HashSet<uint>();
        }
    }
    public void AssociateConstraintWithVar(BoolExpr constraint, string var) {
        vars[var].Add(constraint.Id);
    }
}