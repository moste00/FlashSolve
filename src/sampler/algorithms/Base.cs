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
    
    public Base(Config configs, uint noOutputs)
    {
        Configs = configs;
        NoOutputs = noOutputs;
        Timer = configs.SamplerTimer;
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
        // mocking till kamal finishes his class
        var ctx = new Context();
        var bvType = ctx.MkBitVecSort(32);

        var a = (BitVecExpr)ctx.MkConst("a", bvType)!;
        var b = (BitVecExpr)ctx.MkConst("b", bvType)!;
        var c = (BitVecExpr)ctx.MkConst("c", bvType)!;
        var namesToExprs = new Dictionary<string, BitVecExpr>(){
            { "a", a },
            { "b", b },
            { "c", c }
        };
        var zero = ctx.MkBV(0, bvType.Size);
        var thousand = ctx.MkBV(1000, bvType.Size);
        var twentyFive = ctx.MkBV(25, bvType.Size);

        var constraints = new BoolExpr[] {
            ctx.MkBVSGT(a, zero),
            ctx.MkBVSGT(b, zero),
            ctx.MkBVSGT(c, zero),
            ctx.MkBVSLT(a, thousand),
            ctx.MkBVSLT(b, twentyFive),
            ctx.MkBVSLT(c, thousand),
            ctx.MkBVSGT(
                ctx.MkBVSub(
                    ctx.MkBVMul(b, b),
                    ctx.MkBVMul(
                        ctx.MkBVMul(
                            ctx.MkBV(4, bvType.Size),
                            a),
                        c)
                    ),
                zero)
              //  (b * b - 4 * a * c) > 0
        };
        
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