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
            {"duration_in_millis",new List<int>()}
        };
        int numSols = 0;
        var stopwatch = new Stopwatch();
        TimeSpan inner_time_elapsed = new TimeSpan();
        var otherStopWatch = new Stopwatch();

        if (strategy == "NAIVE") {
            var solver = ctx.MkSolver();
            solver.Add(constraints);
            
            
            otherStopWatch.Start();
            stopwatch.Start();
            var result = solver.Check();
            stopwatch.Stop();
            inner_time_elapsed = stopwatch.Elapsed;
            while (result == Status.SATISFIABLE) {
                namesToValues["duration_in_millis"].Add((int)stopwatch.Elapsed.TotalMilliseconds);
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
                
                stopwatch.Restart();
                result = solver.Check();
                stopwatch.Stop();
                inner_time_elapsed += stopwatch.Elapsed;
                
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
            stopwatch.Start();
            var result = optimizer.Check();
            stopwatch.Stop();
            inner_time_elapsed = stopwatch.Elapsed;
            while (result == Status.SATISFIABLE) {
                namesToValues["duration_in_millis"].Add((int)stopwatch.Elapsed.TotalMilliseconds);
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
                
                stopwatch.Restart();
                result = optimizer.Check();
                stopwatch.Stop();
                inner_time_elapsed += stopwatch.Elapsed;
                
                numSols++;
                if (limitSols && numSols == limit) {
                    break;
                }
            }
        }
        else if (strategy == "UNI_HASH_XOR") {
            const int hashSize = 12;
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
            stopwatch.Start();
            var result = solver.Check();
            stopwatch.Stop();
            inner_time_elapsed = stopwatch.Elapsed;
            int count = 0;
            while (result == Status.SATISFIABLE)
            {
                namesToValues["duration_in_millis"].Add((int)stopwatch.Elapsed.TotalMilliseconds);
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
                        // exclude it from next
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
                
                stopwatch.Restart();
                result = solver.Check();
                stopwatch.Stop();
                inner_time_elapsed += stopwatch.Elapsed;

                numSols++;
                if (limitSols && numSols == limit) {
                    break;
                }
            }
        }
        otherStopWatch.Stop();
        Console.WriteLine("Done!");
        Console.WriteLine($"Found {numSols} Solutions in {inner_time_elapsed.Minutes} minutes and {inner_time_elapsed.Seconds} seconds.");
        Console.WriteLine($"Total main loop time is {otherStopWatch.Elapsed.Minutes} minutes and {otherStopWatch.Elapsed.Seconds} seconds.");
        WriteSolsJson(String.Join("",args), namesToValues);
    }

    private static void print_list(List<uint> mylist)
    {
        var str_line = "{ ";
        for (int i = 0; i < mylist.Count(); i++)
        {
            if (i > 0)
                str_line += ",";
            str_line += mylist[i] + " ";
        }

        str_line += "}";
        Console.WriteLine("new List<uint> " + str_line);
    }
    private static List<uint> generate_hash_bits_idx(int bitsCount, int min, int max, int current_idx)
    {
        List<List<uint>> memoized_hashBits = new List<List<uint>>{ 
            new List<uint> { 17 ,73 ,31 ,3 ,38 ,53 ,37 ,57 ,91 ,34 ,58 ,66 ,20 ,88 ,86 ,92 ,36 ,6 ,35 ,70 ,83 ,2 ,24 ,41 ,9 ,54 ,45 ,42 ,44 ,10 ,32 ,56 },
            new List<uint> { 8 ,78 ,15 ,26 ,95 ,32 ,65 ,34 ,51 ,1 ,27 ,71 ,76 ,88 ,7 ,54 ,21 ,80 ,92 ,10 ,30 ,77 ,74 ,37 ,25 ,93 ,91 ,61 ,87 ,72 ,11 ,56 },
            new List<uint> { 60 ,28 ,20 ,82 ,71 ,59 ,81 ,3 ,42 ,27 ,7 ,50 ,46 ,10 ,1 ,79 ,58 ,30 ,26 ,19 ,57 ,11 ,88 ,17 ,34 ,80 ,62 ,86 ,43 ,73 ,4 ,6 },
            new List<uint> { 82 ,69 ,47 ,20 ,72 ,32 ,62 ,64 ,68 ,51 ,61 ,58 ,25 ,34 ,16 ,86 ,70 ,22 ,56 ,6 ,76 ,50 ,79 ,18 ,44 ,26 ,90 ,53 ,88 ,8 ,59 ,14 },
            new List<uint> { 22 ,58 ,2 ,66 ,84 ,7 ,12 ,28 ,0 ,95 ,10 ,57 ,80 ,23 ,27 ,59 ,21 ,50 ,39 ,65 ,38 ,30 ,9 ,44 ,60 ,87 ,53 ,37 ,1 ,90 ,47 ,54 },
            new List<uint> { 3 ,8 ,9 ,77 ,78 ,30 ,61 ,59 ,74 ,11 ,2 ,18 ,1 ,71 ,93 ,37 ,35 ,27 ,42 ,49 ,65 ,19 ,48 ,52 ,38 ,56 ,24 ,67 ,72 ,57 ,70 ,64 },
            new List<uint> { 55 ,11 ,93 ,32 ,46 ,43 ,42 ,63 ,71 ,87 ,61 ,5 ,48 ,10 ,36 ,7 ,92 ,49 ,53 ,77 ,83 ,29 ,52 ,95 ,19 ,28 ,76 ,80 ,30 ,67 ,59 ,90 },
            new List<uint> { 69 ,64 ,27 ,93 ,18 ,28 ,74 ,30 ,85 ,41 ,86 ,8 ,61 ,3 ,47 ,70 ,50 ,91 ,71 ,87 ,56 ,24 ,67 ,34 ,94 ,21 ,49 ,42 ,33 ,68 ,82 ,63 },
            new List<uint> { 95 ,44 ,24 ,91 ,87 ,38 ,81 ,76 ,25 ,33 ,14 ,1 ,59 ,93 ,37 ,56 ,21 ,57 ,8 ,90 ,47 ,13 ,43 ,54 ,6 ,12 ,77 ,45 ,69 ,70 ,2 ,55 },
            new List<uint> { 9 ,76 ,43 ,8 ,85 ,31 ,89 ,29 ,4 ,58 ,80 ,13 ,52 ,77 ,30 ,47 ,65 ,33 ,57 ,84 ,59 ,5 ,79 ,66 ,36 ,18 ,81 ,70 ,17 ,63 ,82 ,0 },
            new List<uint> { 60 ,47 ,29 ,74 ,72 ,87 ,4 ,63 ,41 ,21 ,93 ,46 ,9 ,49 ,80 ,82 ,20 ,1 ,30 ,2 ,10 ,59 ,71 ,43 ,0 ,31 ,44 ,16 ,45 ,61 ,17 ,23 },
            new List<uint> { 10 ,68 ,75 ,37 ,31 ,85 ,23 ,82 ,41 ,76 ,7 ,45 ,33 ,83 ,81 ,11 ,57 ,36 ,66 ,30 ,32 ,1 ,40 ,52 ,46 ,14 ,4 ,53 ,62 ,8 ,73 ,0 },
        };

        if (current_idx < 12)
            return memoized_hashBits[current_idx];

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
        print_list(hashBits);
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
            List<uint> hashBits = generate_hash_bits_idx(32, 0, 96, idx);
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

        var path = "out/sample/" + name + ".json";
        File.WriteAllText(path,content.ToString());
    }
}
