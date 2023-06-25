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
            list[k] = list[n];
            list[n] = value;
        }
    }
}