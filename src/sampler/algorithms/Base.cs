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

    // Constructor
    // SHOULD TAKE THE CONSTRAINS STRUCT
    public Base(Config configs, uint noOutputs)
    {
        _configs = configs;
        NoOutputs = noOutputs;
        Timer = configs.SamplerTimer;
        Paralization = configs.SamplerParalization;
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

}