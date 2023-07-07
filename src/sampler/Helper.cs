namespace flashsolve.sampler;
using Microsoft.Z3;
using System.Numerics;

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

    public static double EuclideanDistance(List<object> point1, List<object> point2)
    {
        double sumOfSquares = 0;
        for (int i = 0; i < point1.Count ; i++)
        {
            double p1 = double.Parse(point1[i].ToString()!);
            double p2 = double.Parse(point2[i].ToString()!);
            sumOfSquares += Math.Pow(p1 - p2, 2);
        }
        return Math.Sqrt(sumOfSquares);
    }
    public static double calculate_spread(Dictionary<string, List<object>> namesToValues)
    {
        string outputHashKey = "hash";
        string outputDurationKey = "duration_in_millis";
        var keys = namesToValues.Keys.ToList();
        var numPoints = namesToValues[keys[0]].Count;

        var dist = 0.0;
        for (int i = 0; i < numPoints; i++)
            for (int j = i+1; j < numPoints; j++)
            {
                List<object> p1 = new List<object>();
                List<object> p2 = new List<object>();
                foreach (var pi in keys)
                {
                    if(pi == outputHashKey || pi == outputDurationKey)
                        continue;
                    p1.Add(namesToValues[pi][i]);
                }
                foreach (var pi in keys)
                {
                    if(pi == outputHashKey || pi == outputDurationKey)
                        continue;
                    p2.Add(namesToValues[pi][j]);
                }

                dist += EuclideanDistance(p1, p2);
            }

        return dist/(numPoints*(numPoints-1)/2);
    }
    
    public static (double, double) CalcTimePerSolution(Dictionary<string, List<object>> namesToValues)
    {
        string outputDurationKey = "duration_in_millis";
        double accTime = 0.0;
        double maxTime = 0.0;
        int numPoints = namesToValues[outputDurationKey].Count;
        List<object> times = namesToValues[outputDurationKey];

        for (int i = 0; i < numPoints; i++)
        {
            var t = double.Parse(times[i].ToString()!);
            double currentTime = t / 1000.0;
            accTime += currentTime;
            if (currentTime > maxTime)
            {
                maxTime = currentTime;
            }
        }

        return (accTime, maxTime);
    }
    
    public static void print_output_dictionary(Dictionary<string, List<object>> namesToValues)
    {
        int maxLength = namesToValues.Values.Max(list => list.Count);

        foreach (var key in namesToValues.Keys)
        {
            if(key == "duration_in_millis")
                Console.Write("   | "+key+"  ");
            else
                Console.Write(key+ "  ");
        }

        Console.Write("\n");

        for (int i = 0; i < maxLength; i++)
        {
            foreach (var kvp in namesToValues)
            {
                var key = kvp.Key;
                var values = kvp.Value;

                if (i < values.Count)
                {
                    if(key == "duration_in_millis")
                        Console.Write("   | " + values[i]+ " ms");
                    else
                        Console.Write(values[i]+ " ");
                        //Console.Write("0x" + BigInteger.Parse(values[i].ToString()).ToString("x")+ " ");
                }
                else
                {
                    Console.Write(" - ");
                }
            }
            Console.WriteLine();
        }
    }
}