using flashsolve.compiler;

namespace flashsolve.sampler.algorithms;
using Microsoft.Z3;
using flashsolve.sampler;
using System.Collections.Concurrent;

public class Base
{
    //constants
    private const string OutputHashKey = "hash";
    private const string OutputDurationKey = "duration_in_millis";

    // members
    protected readonly Config Configs;
    protected readonly uint TestingNoOutputs;
    protected readonly uint NoOutputs;
    protected readonly bool Timer;
    protected RandProblem _problem;
    // Constructor
    // SHOULD TAKE THE CONSTRAINS STRUCT
    public Base(Config configs, uint noOutputs, RandProblem problem)
    {
        Configs = configs;
        NoOutputs = noOutputs;
        Timer = configs.SamplerTimer;
        _problem = problem;
        TestingNoOutputs = Math.Min(Configs.TestingSampleSize, NoOutputs);
    }

    protected Dictionary<string, List<object>> create_output_dictionary(Dictionary<string, BitVecExpr> namesToExprs, bool hash)
    {
        var namesToValues = new Dictionary<string, List<object>>();
        foreach (var key in namesToExprs.Keys)
        {
            namesToValues[key] = new List<object>();
        }
        if(hash)
            namesToValues[OutputHashKey] = new List<object>();   
        
        if(Timer)
            namesToValues[OutputDurationKey] = new List<object>();

        return namesToValues;
    }

    protected (Context context, BoolExpr[] constraints, Dictionary<string, BitVecExpr> namesToExprs) get_constraints()
    {
        var ctx = _problem.Context;

        var constraints = _problem.Constraints;
        var namesToExprs = _problem.Vars;
        
        return (ctx, constraints, namesToExprs);
    }
    
    public virtual void run_algorithm()
    {
    }

    public virtual void test_algorithm(ConcurrentDictionary<string, Dictionary<string, List<object>>> results)
    {
        Console.WriteLine("Warning: empty.....u called the base test function");
    }
}