using flashsolve.compiler;

namespace flashsolve.sampler.algorithms;
using System.Diagnostics;
using System.Text;
using Microsoft.Z3;
using flashsolve.sampler;
using System.Numerics;

public class Base
{
    //constants
    private const string OutputHashKey = "hash";
    private const string OutputDurationKey = "duration_in_millis";

    // members
    private Config _configs;
    protected readonly uint NoOutputs;
    protected readonly bool Timer;
    protected bool Paralization;

    protected RandProblem _problem;
    // Constructor
    // SHOULD TAKE THE CONSTRAINS STRUCT
    public Base(Config configs, uint noOutputs, RandProblem problem)
    {
        _configs = configs;
        NoOutputs = noOutputs;
        Timer = configs.SamplerTimer;
        Paralization = configs.SamplerParalization;
        _problem = problem;
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
    
    protected void print_output_dictionary(Dictionary<string, List<object>> namesToValues)
    {
        int maxLength = namesToValues.Values.Max(list => list.Count);

        for (int i = 0; i < maxLength; i++)
        {
            foreach (var kvp in namesToValues)
            {
                var key = kvp.Key;
                var values = kvp.Value;

                if (i < values.Count)
                {
                    if(key == OutputDurationKey)
                        Console.Write("   | " + values[i]+ " ms");
                    else
                        Console.Write("0x" + BigInteger.Parse(values[i].ToString()).ToString("x")+ " ");
                }
                else
                {
                    Console.Write(" - ");
                }
            }
            Console.WriteLine();
        }
    }

    protected (Context context, BoolExpr[] constraints, Dictionary<string, BitVecExpr> namesToExprs) get_constraints()
    {
        // mocking till kamal finishes his class
        var ctx = _problem.Context;

        var constraints = _problem.Constraints;
        var namesToExprs = _problem.Vars;
        
        return (ctx, constraints, namesToExprs);
    }

}