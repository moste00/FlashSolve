namespace flashsolve.util.sampleralgorithms;
using System.Diagnostics;
using System.Text;
using Microsoft.Z3;
using flashsolve.util;

public class Base
{
    //constants
    private const string OutputHashKey = "hash";
    private const string OutputDurationKey = "duration_in_millis";

    // members
    protected Config Configs;
    protected uint NoOutputs;
    protected bool Timer;
    protected bool Paralization;

    // Constructor
    // SHOULD TAKE THE CONSTRAINS STRUCT
    public Base(Config configs, uint no_outputs)
    {
        Configs = configs;
        NoOutputs = no_outputs;
        Timer = configs.sampler_timer;
        Paralization = configs.sampler_paralization;
    }

    protected Dictionary<string, List<int>> create_output_dictionary(Dictionary<string, IntExpr> namesToExprs, bool hash)
    {
        var namesToValues = new Dictionary<string, List<int>>();
        foreach (var key in namesToExprs.Keys)
        {
            namesToValues[key] = new List<int>();
        }
        if(Timer)
            namesToValues[OutputDurationKey] = new List<int>();
        if(hash)
            namesToValues[OutputHashKey] = new List<int>();   

        return namesToValues;
    }
    
    protected void print_output_dictionary(Dictionary<string, List<int>> namesToValues)
    {
        int maxLength = namesToValues.Values.Max(list => list.Count);

        for (int i = 0; i < maxLength; i++)
        {
            foreach (var kvp in namesToValues)
            {
                string key = kvp.Key;
                List<int> values = kvp.Value;

                if (i < values.Count)
                {
                    Console.Write(values[i] + " ");
                }
                else
                {
                    Console.Write(" - ");
                }
            }
            Console.WriteLine();
        }
    }

    protected (Context context, BoolExpr[] constraints, Dictionary<string, IntExpr> namesToExprs) get_constraints()
    {
        // mocking till kamal finishes his class
        var ctx = new Context();
        var a = ctx.MkIntConst("a")!;
        var b = ctx.MkIntConst("b")!;
        var c = ctx.MkIntConst("c")!;
        var namesToExprs = new Dictionary<string, IntExpr>(){
            { "a", a },
            { "b", b },
            { "c", c }
        };

        var constraints = new[] {
            a > 0,
            b > 0,
            c > 0,
            a < 1000,
            b < 25,
            c < 1000,
            (b * b - 4 * a * c) > 0
        };
        
        return (ctx, constraints, namesToExprs);
    }

}