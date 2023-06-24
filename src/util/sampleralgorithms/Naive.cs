namespace flashsolve.util.sampleralgorithms;
using System.Diagnostics;
using System.Text;
using Microsoft.Z3;

public class Naive : Base
{
    protected Solver solver;
    protected Context ctx;
    protected Dictionary<string, IntExpr> namesToExprs;
    protected Dictionary<string, List<int>> namesToValues;
    public Naive(Config configs, uint no_outputs) : base(configs, no_outputs)
    {
        var (CTX, constraints, namestoexprs) = get_constraints();
        namesToExprs = namestoexprs;
        ctx = CTX;
        solver = ctx.MkSolver();
        solver.Add(constraints);
        namesToValues = create_output_dictionary(namesToExprs, false);
    }

    protected Status check_with_timer(Solver solver, Stopwatch stopwatch)
    {
        stopwatch.Restart();
        var result = solver.Check();
        stopwatch.Stop();
        namesToValues["duration_in_millis"].Add((int)stopwatch.Elapsed.TotalMilliseconds);

        return result;
    }
    
    
    public void run_naive()
    {
        int numSols = 0;
        var stopwatch = new Stopwatch();
        Status result;

        do
        {
            if (Timer)
                result = check_with_timer(solver, stopwatch);
            else
                result = solver.Check();
            
            if (result != Status.SATISFIABLE)
                break;
            
            var model = solver.Model!;
           
            BoolExpr allVariablesHaveNewValues = null;
            foreach (var con in model.Consts) {
                var constName = con.Key.Name.ToString();
                namesToValues[constName].Add(
                    Int32.Parse(con.Value.ToString())
                );
                var expr = namesToExprs[constName];
                    
                if (allVariablesHaveNewValues == null) {
                    allVariablesHaveNewValues = ctx.MkAnd(ctx.MkEq(expr, con.Value));
                }
                else {
                    allVariablesHaveNewValues = ctx.MkAnd(allVariablesHaveNewValues, ctx.MkEq(expr, con.Value))!;
                }
            }
            allVariablesHaveNewValues = ctx.MkNot(allVariablesHaveNewValues)!;
                
            solver.Add(allVariablesHaveNewValues);

            numSols++;
            if (numSols == NoOutputs) {
                break;
            }
        } while (result == Status.SATISFIABLE);
        
        print_output_dictionary(namesToValues);
    }
    
}