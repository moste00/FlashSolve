namespace flashsolve.util.sampleralgorithms;
using System.Diagnostics;
using Microsoft.Z3;

public class Hash: Naive
{
    //constants
    private const uint InputSize = 32;
    
    protected uint hashSize;
    protected uint bitsCounts;
    private BitVecExpr input;

    public Hash(Config configs, uint no_outputs) : base(configs, no_outputs)
    {
        hashSize = configs.hashConstants_hashSize;
        bitsCounts = configs.hashConstants_bitsCounts;
        input = generate_input_bits();
    }
    
    private BitVecExpr generate_input_bits()
    {
        var bvInputType = ctx.MkBitVecSort(InputSize);
        var input = (BitVecExpr)ctx.MkConst("input", bvInputType);
        var exprs = namesToExprs.Values.ToList();
        Helper.shuffle_expr_list(ref exprs);

        // loop over the exprs list and get the first n-bits from all of them till reach 32-bits
        // or finish the vars.....
        
        return input;
    }
}