namespace flashsolve.util;
using Microsoft.Z3;

public static class Helper
{
    public static void shuffle_expr_list( ref List<BitVecExpr> list)
    {
        // Create a random number generator
        Random random = new Random();

        // Perform Fisher-Yates shuffle
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            var value = list[k];
            (list[k], list[n]) = (list[n], value);
        }
    }

    private static BitVecExpr extract_bv_bites(uint inputSize, List<BitVecExpr> exprs, Context ctx)
    {
        BitVecExpr concatenatedInputBits = null;
        uint currentIdx = 0;
        uint currentExprIdx = 0;
        while (true)
        {
            bool nothingChanged = true;
            foreach (var expr in exprs)
            {
                if(currentExprIdx >= expr.SortSize)
                    continue;
                nothingChanged = false;

                if (concatenatedInputBits == null)
                {
                    concatenatedInputBits = ctx.MkExtract(currentExprIdx,
                        currentExprIdx, 
                        expr)!;
                }
                else
                {
                    concatenatedInputBits = ctx.MkConcat(concatenatedInputBits, 
                        ctx.MkExtract(currentExprIdx,
                            currentExprIdx,
                            expr)
                        )!;
                }

                currentIdx++;
                if (currentIdx == inputSize)
                    return concatenatedInputBits;
            }

            currentExprIdx++;
            if (nothingChanged)
                currentExprIdx = 0;
        }
        
    }
    
    public static  BoolExpr [] extract_input_constraint(BitVecExpr input, List<BitVecExpr> exprs, Context ctx)
    {
        BitVecExpr concatenatedInputBits = extract_bv_bites(input.SortSize, exprs, ctx);
        var inputConstraint = new[] {
            ctx.MkEq(input,  concatenatedInputBits)
        };
        return inputConstraint;
    }
    
}