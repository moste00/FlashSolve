using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Collections.Generic;
using Microsoft.Z3;

namespace FlashSolve.main;
public static class Sample {
    public static void main(String[] args) {
        var strategy = args[0];
        Boolean limitSols = false;
        int limit = Int32.MaxValue;
        if (args is [_, "--limit", _, ..]) {
            limitSols = true;
            limit = Int32.Parse(args[2]);
        }
        
        
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

        var namesToValues = new Dictionary<String, List<int>>(){
            {"a",new List<int>()},
            {"b",new List<int>()},
            {"c",new List<int>()},
            {"hash",new List<int>()},
        };
        int numSols = 0;
        var stopwatch = new Stopwatch();
        var otherStopWatch = new Stopwatch();

        if (strategy == "NAIVE") {
            var solver = ctx.MkSolver();
            solver.Add(constraints);
            
            
            otherStopWatch.Start();
            var result = solver.Check();
            while (result == Status.SATISFIABLE) {
                var model = solver.Model!;
           
                BoolExpr allVariablesHaveNewValues = null;
                foreach (var con in model.Consts) {
                    var constName = con.Key.Name.ToString();
                    namesToValues[constName].Add(
                        Int32.Parse(con.Value.ToString())
                    );
                    var expr = namesToExprs[constName];
                    
                    if (allVariablesHaveNewValues == null) {
                        allVariablesHaveNewValues = ctx.MkAnd(ctx.MkEq(expr, con.Value));
                    }
                    else {
                        allVariablesHaveNewValues = ctx.MkAnd(allVariablesHaveNewValues, ctx.MkEq(expr, con.Value))!;
                    }
                }
                allVariablesHaveNewValues = ctx.MkNot(allVariablesHaveNewValues)!;
                
                solver.Add(allVariablesHaveNewValues);
                
                stopwatch.Start();
                result = solver.Check();
                stopwatch.Stop();
                
                numSols++;
                if (limitSols && numSols == limit) {
                    break;
                }
            }
        }
        else if (strategy == "MAXSMT") {
            var optimizer = ctx.MkOptimize()!;
            var random = new Random();
            
            var hardConstraints = new StringBuilder();
            hardConstraints.Append("(declare-const a Int)")
                           .Append("(declare-const b Int)")
                           .Append("(declare-const c Int)");
            foreach (var constr in constraints) {
                hardConstraints.Append("(assert ")
                               .Append(constr)
                               .Append(')');
            }
            optimizer.FromString(hardConstraints.ToString());
            
            otherStopWatch.Start();
            var result = optimizer.Check();
            while (result == Status.SATISFIABLE) {
                hardConstraints.Append("(assert (not (and ");
                
                foreach (var con in optimizer.Model.Consts) {
                    var constName = con.Key.Name.ToString();
                    var constValue = Int32.Parse(con.Value.ToString());
                    
                    namesToValues[constName].Add(constValue);
                    hardConstraints.Append("(= ")
                                   .Append(constName)
                                   .Append(' ')
                                   .Append(constValue)
                                   .Append(") ");
                }
                hardConstraints.Append(")))");
                
                optimizer = ctx.MkOptimize();
                optimizer.FromString(hardConstraints.ToString());
                
                //add soft constraints telling Z3 to prefer random values
                optimizer.AssertSoft(ctx.MkEq(a, ctx.MkInt(random.Next(0,1000))),
                                     1,"G");
                optimizer.AssertSoft(ctx.MkEq(b, ctx.MkInt(random.Next(0,25))),
                                     1,"G");
                optimizer.AssertSoft(ctx.MkEq(c, ctx.MkInt(random.Next(0,1000))),
                                     1,"G");
                
                stopwatch.Start();
                result = optimizer.Check();
                stopwatch.Stop();
                
                numSols++;
                if (limitSols && numSols == limit) {
                    break;
                }
            }
        }
        else if (strategy == "UNI_HASH_XOR") {
            const int hashSize = 13;
            Console.WriteLine("limit = "+ limit+".");
            Console.WriteLine("hash Size = "+ hashSize + " bits.");
            Console.WriteLine("hash groups = "+ Math.Pow(2, hashSize) + " solutions.");
            
            var solver = ctx.MkSolver();
            // adding hash constrains
            var bvInputType = ctx.MkBitVecSort(96);
            var bvHashType = ctx.MkBitVecSort(hashSize);
            var hash = (BitVecExpr)ctx.MkConst("hash", bvHashType);

            var input = (BitVecExpr)ctx.MkConst("input", bvInputType);
            var localConstraints = new[] {
                ctx.MkEq(input,  ctx.MkConcat( ctx.MkConcat(ctx.MkInt2BV(32, a), ctx.MkInt2BV(32, b)), ctx.MkInt2BV(32, c)))
            };
            
            // choosing my hash bits
            BoolExpr hashBitsExprs = add_hash_bits(ctx, input, hash, hashSize);
            // add all constrains
            solver.Add(constraints);
            solver.Add(localConstraints);
            solver.Add(hashBitsExprs);
            

            otherStopWatch.Start();
            var result = solver.Check();
            int count = 0;
            while (result == Status.SATISFIABLE)
            {
                if (count == limit)
                    break;
                else
                    count++;

                var model = solver.Model!;
           
                BoolExpr allVariablesHaveNewValues = null;
                foreach (var con in model.Consts) {
                    var constName = con.Key.Name.ToString();
                    if (constName == "hash")
                    {
                        // add it to the log
                        namesToValues[constName].Add(
                            Int32.Parse(con.Value.ToString())
                        );
                        // execlude it from next
                        allVariablesHaveNewValues = ctx.MkAnd(ctx.MkEq(hash, con.Value));
                    }
                    else if (constName.Contains("hash") || constName == "input")
                    {
                        continue;
                    }
                    else
                    {
                        namesToValues[constName].Add(
                            Int32.Parse(con.Value.ToString())
                        );
                    }
                }
                allVariablesHaveNewValues = ctx.MkNot(allVariablesHaveNewValues)!;
                
                solver.Add(allVariablesHaveNewValues);
                
                stopwatch.Start();
                result = solver.Check();
                stopwatch.Stop();
                
                numSols++;
                if (limitSols && numSols == limit) {
                    break;
                }
            }
        }
        otherStopWatch.Stop();
        Console.WriteLine("Done!");
        Console.WriteLine($"Found {numSols} Solutions in {stopwatch.Elapsed.Seconds} seconds.");
        Console.WriteLine($"Total main loop time is {otherStopWatch.Elapsed.Seconds} seconds.");
        WriteSolsJson(String.Join("",args), namesToValues);
    }

    private static List<uint> generate_hash_bits_idx(int bitsCount, int min, int max)
    {
        List<uint> hashBits = new List<uint>();
        if (bitsCount > (max - min))
        {
            Console.WriteLine("Error in <generate_hash_bits> : wrong bitscount .... bitscount should be < (max-min)");
            return hashBits;
        }
        Random rnd = new Random();
        for (int i = 0; i < bitsCount; i++)
        {
            uint newNumber = 0;
            newNumber = (uint)rnd.Next(0, 96);
            if (!hashBits.Contains(newNumber))
                hashBits.Add(newNumber);
            else
                i--;
        }
        return hashBits;
    }

    private static BoolExpr add_hash_bits(Context ctx, BitVecExpr input, BitVecExpr hash, int hashSize)
    {
        BoolExpr allHashBitsExprs = null;
        var bvbitHashType = ctx.MkBitVecSort(1);
        
        BitVecExpr totalHashVecExpr = null;

        // create all hash bits expers
        for (int idx = 0; idx < hashSize; idx++)
        {
            // gen new hashBits idxes
            List<uint> hashBits = generate_hash_bits_idx(32, 0, 96);
            // create a constant for the new hash
            var hash_i = (BitVecExpr)ctx.MkConst("hash"+idx, bvbitHashType);
            // xor first 2-bits
            BitVecExpr hashVecExpr = ctx.MkBVXOR(ctx.MkExtract(hashBits[0], hashBits[0], input), ctx.MkExtract(hashBits[1], hashBits[1], input));
            // xor the rest of the bits
            for (int i = 2; i < 32; i++)
            {
                hashVecExpr = ctx.MkBVXOR(hashVecExpr, ctx.MkExtract(hashBits[i], hashBits[i], input));
            }
            //concat the hash_i to the total hash constant
            if (totalHashVecExpr == null)
                totalHashVecExpr = hash_i;
            else
                totalHashVecExpr = ctx.MkConcat(totalHashVecExpr, hash_i);

            // create the hash bool exper
            BoolExpr hashBoolExpr = ctx.MkEq(hash_i, hashVecExpr);
            // add it to the all hash bits expr
            if (allHashBitsExprs == null)
            {
                allHashBitsExprs = hashBoolExpr;
            }
            else
            {
                allHashBitsExprs = ctx.MkAnd(allHashBitsExprs, hashBoolExpr)!;
            }
        }
        // make the total hash BoolExpr
        BoolExpr totalHashBoolExpr = ctx.MkEq(hash, totalHashVecExpr);
        allHashBitsExprs = ctx.MkAnd(allHashBitsExprs, totalHashBoolExpr);

        return allHashBitsExprs;
    }
    private static void WriteSolsJson(string name,Dictionary<string, List<int>> namesToValues) {
        var content = new StringBuilder();

        content.Append("{\n\"variables\": ");
        content.Append('"');

        int numSols = 0;
        foreach (var entry in namesToValues) {
            if (numSols == 0) numSols = entry.Value.Count(); 
                
            content.Append(entry.Key);
            content.Append(' ');
        }

        content.Append("\",\n");

        content.Append("\"values\": [\n\t");
        
        Boolean commaNecessary = false;
        for (int i = 0; i < numSols; i++) {
            if (commaNecessary) {
                content.Append(',');
            }
            else {
                commaNecessary = true;
            }
            content.Append("[");
            Boolean anotherCommaNecessary = false;
            foreach (var entry in namesToValues) {
                if (anotherCommaNecessary) {
                    content.Append(',');
                }
                else {
                    anotherCommaNecessary = true;
                }
                if (!name.Contains("UNI_HASH_XOR") && entry.Key == "hash")
                {
                    content.Append(0);
                    content.Append(" ");
                    continue;   
                }
                content.Append(entry.Value[i]);
                content.Append(" ");
            }
            content.Append("]\n\t");
        }
        
        content.Append("\n]"); //end values
        content.Append("\n}");//end root dict
        
        File.WriteAllText(name+".json",content.ToString());
    }
}
