using flashsolve.compiler;

namespace flashsolve.sampler.algorithms;
using System.Diagnostics;
using Microsoft.Z3;
using System.Collections.Concurrent;

public class Naive : Base
{
    protected readonly Solver Solver;
    protected readonly Context Ctx;
    protected readonly Dictionary<string, BitVecExpr>NamesToExprs;
    protected Dictionary<string, List<object>> NamesToValues;
    public Naive(Config configs, uint noOutputs, RandProblem problem) : base(configs, noOutputs, problem)
    {
        var (ctx, constraints, namestoexprs) = get_constraints();
        NamesToExprs = namestoexprs;
        Ctx = ctx;
        Solver = Ctx.MkSolver();
        Solver.Add(constraints);
        NamesToValues = create_output_dictionary(NamesToExprs, false);
    }

    protected Status check_with_timer(Stopwatch stopwatch)
    {
        stopwatch.Restart();
        var result = Solver.Check();
        stopwatch.Stop();
        NamesToValues["duration_in_millis"].Add(stopwatch.Elapsed.TotalMilliseconds);

        return result;
    }
    
    
    protected uint run_naive_algorithm(uint thresh = 1, uint currentNumSols = 0)
    {
        var stopwatch = new Stopwatch();
        Status result;

        do
        {
            if (Timer)
                result = check_with_timer(stopwatch);
            else
                result = Solver.Check();
            
            if (result != Status.SATISFIABLE)
                break;
            
            var model = Solver.Model!;

            BoolExpr allVariablesHaveNewValues = null;
            foreach (var con in model.Consts) {
                var constName = con.Key.Name.ToString();
                if (constName.Contains("hash") || constName == "input")
                {
                    continue;
                }
                NamesToValues[constName].Add(
                    con.Value
                );
                var expr = NamesToExprs[constName];
                    
                if (allVariablesHaveNewValues == null) {
                    allVariablesHaveNewValues = Ctx.MkAnd(Ctx.MkEq(expr, con.Value))!;
                }
                else {
                    allVariablesHaveNewValues = Ctx.MkAnd(allVariablesHaveNewValues, Ctx.MkEq(expr, con.Value))!;
                }
            }
            allVariablesHaveNewValues = Ctx.MkNot(allVariablesHaveNewValues)!;
                
            Solver.Add(allVariablesHaveNewValues);

            currentNumSols++;
            if (currentNumSols == NoOutputs || currentNumSols == thresh) {
                break;
            }
        } while (result == Status.SATISFIABLE);
        
        return currentNumSols;
    }

    public void run_naive()
    {
        run_naive_algorithm(NoOutputs);
        print_output_dictionary(NamesToValues);
    }

    public override void test_algorithm(ConcurrentDictionary<string, Dictionary<string, List<object>>> results)
    {
        run_naive_algorithm(TestingNoOutputs);
        var added = results.TryAdd("Naive", NamesToValues);
        throw new Exception("test_algorithm of (Naive) could not add it's results to the ConcurrentDictionary");
    }
}