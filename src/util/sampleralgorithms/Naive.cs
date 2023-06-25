namespace flashsolve.util.sampleralgorithms;
using System.Diagnostics;
using Microsoft.Z3;

public class Naive : Base
{
    protected Solver solver;
    protected Context ctx;
    protected Dictionary<string, BitVecExpr>namesToExprs;
    protected Dictionary<string, List<object>> namesToValues;
    public Naive(Config configs, uint no_outputs) : base(configs, no_outputs)
    {
        var (CTX, constraints, namestoexprs) = get_constraints();
        namesToExprs = namestoexprs;
        ctx = CTX;
        solver = ctx.MkSolver();
        solver.Add(constraints);
        namesToValues = create_output_dictionary(namesToExprs, false);
    }

    protected Status check_with_timer(Stopwatch stopwatch)
    {
        stopwatch.Restart();
        var result = solver.Check();
        stopwatch.Stop();
        namesToValues["duration_in_millis"].Add(stopwatch.Elapsed.TotalMilliseconds);

        return result;
    }
    
    
    protected void run_naive_algorithm(uint thresh = 1, uint currentNumSols = 0)
    {
        var stopwatch = new Stopwatch();
        Status result;

        do
        {
            if (Timer)
                result = check_with_timer(stopwatch);
            else
                result = solver.Check();
            
            if (result != Status.SATISFIABLE)
                break;
            
            var model = solver.Model!;

            BoolExpr allVariablesHaveNewValues = null;
            foreach (var con in model.Consts) {
                var constName = con.Key.Name.ToString();
                if (constName.Contains("hash") || constName == "input")
                {
                    continue;
                }
                namesToValues[constName].Add(
                    con.Value
                );
                var expr = namesToExprs[constName];
                    
                if (allVariablesHaveNewValues == null) {
                    allVariablesHaveNewValues = ctx.MkAnd(ctx.MkEq(expr, con.Value))!;
                }
                else {
                    allVariablesHaveNewValues = ctx.MkAnd(allVariablesHaveNewValues, ctx.MkEq(expr, con.Value))!;
                }
            }
            allVariablesHaveNewValues = ctx.MkNot(allVariablesHaveNewValues)!;
                
            solver.Add(allVariablesHaveNewValues);

            currentNumSols++;
            if (currentNumSols == NoOutputs || currentNumSols == thresh) {
                break;
            }
        } while (result == Status.SATISFIABLE);
    }

    public void run_naive()
    {
        run_naive_algorithm(NoOutputs);
        print_output_dictionary(namesToValues);
    }

}