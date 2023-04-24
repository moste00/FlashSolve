using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Microsoft.Z3;

namespace flashsolve.main;
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
            {"c",new List<int>()}
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
            var namesToRandomSoftValues = new Dictionary<String, List<int>>(){
                {"a",new List<int>()},
                {"b",new List<int>()},
                {"c",new List<int>()}
            };
            namesToRandomSoftValues["a"].Add(-1);
            namesToRandomSoftValues["b"].Add(-1);
            namesToRandomSoftValues["c"].Add(-1);
            int numSoftAssertion = 0;
            
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
                
                //pick which variables to randomize : 1 means all variables, 2,3 or 4 means either a,b, or c respectively
                var randomziedVariable = 4;//random.Next(1, 5);
                switch (randomziedVariable) {
                    //All variables
                    case 1: {
                        var softRanda = random.Next(0, 1000);
                        namesToRandomSoftValues["a"].Add(softRanda);
                        optimizer.AssertSoft(ctx.MkEq(a, ctx.MkInt(softRanda)),
                            1, "G");
                        
                        var softRandb = random.Next(0, 25);
                        namesToRandomSoftValues["b"].Add(softRandb);
                        optimizer.AssertSoft(ctx.MkEq(b, ctx.MkInt(softRandb)),
                            1, "G");
                        
                        var softRandc = random.Next(0, 1000);
                        namesToRandomSoftValues["c"].Add(softRandc);
                        optimizer.AssertSoft(ctx.MkEq(c, ctx.MkInt(softRandc)),
                            1, "G");

                        numSoftAssertion += 3;
                        break;
                    }
                    //just a 
                    case 2: {
                        var softRanda = random.Next(0, 1000);
                        namesToRandomSoftValues["a"].Add(softRanda);
                        namesToRandomSoftValues["b"].Add(-1);
                        namesToRandomSoftValues["c"].Add(-1);
                        optimizer.AssertSoft(ctx.MkEq(a, ctx.MkInt(softRanda)),
                            1, "G");
                        numSoftAssertion++;
                        break;
                    }
                    //just b
                    case 3: {
                        var softRandb = random.Next(0, 25);
                        namesToRandomSoftValues["a"].Add(-1);
                        namesToRandomSoftValues["b"].Add(softRandb);
                        namesToRandomSoftValues["c"].Add(-1);
                        optimizer.AssertSoft(ctx.MkEq(b, ctx.MkInt(softRandb)),
                            1, "G");
                        numSoftAssertion++;
                        break;
                    }
                    //just c
                    case 4: {
                        var softRandc = random.Next(0, 1000);
                        namesToRandomSoftValues["a"].Add(-1);
                        namesToRandomSoftValues["b"].Add(-1);
                        namesToRandomSoftValues["c"].Add(softRandc);

                        optimizer.AssertSoft(ctx.MkEq(c, ctx.MkInt(softRandc)),
                            1, "G");
                        numSoftAssertion++;
                        break;
                    }
                }

                stopwatch.Start();
                result = optimizer.Check();
                stopwatch.Stop();
                
                numSols++;
                if (limitSols && numSols == limit) {
                    break;
                }
            }

            Console.WriteLine($"Number of random sols {namesToValues["a"].Count} and number of soft random values {namesToRandomSoftValues["a"].Count}");
            Console.WriteLine($"Number of random sols {namesToValues["b"].Count} and number of soft random values {namesToRandomSoftValues["b"].Count}");
            Console.WriteLine($"Number of random sols {namesToValues["c"].Count} and number of soft random values {namesToRandomSoftValues["c"].Count}");

            Console.WriteLine($"Number of times solver violated soft assertions {CountViolatedSoftAssertions(namesToValues,namesToRandomSoftValues)} out of a total of {numSoftAssertion}");
        }
        otherStopWatch.Stop();
        Console.WriteLine("Done!");
        Console.WriteLine($"Found {numSols} Solutions in {stopwatch.Elapsed.Seconds} seconds.");
        Console.WriteLine($"Total main loop time is {otherStopWatch.Elapsed.Seconds} seconds.");
        WriteSolsJson(String.Join('!',args), namesToValues);
    }

    private static int CountViolatedSoftAssertions(Dictionary<string, List<int>> namesToValues, Dictionary<string, List<int>> namesToRandomSoftValues) {
        int count = 0; 
        
        foreach (var entry in namesToValues) {
            string name = entry.Key;
            var values = entry.Value;
            for (int i = 0; i < values.Count; i++) {
                int softRandValue = namesToRandomSoftValues[name][i];
                if (softRandValue == -1) continue;
                
                if (values[i] != softRandValue) {
                    count++;
                }
            }
        }

        return count;
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
                content.Append(entry.Value[i]);
                content.Append(" ");
            }
            content.Append("]\n\t");
        }
        
        content.Append("\n]"); //end values
        content.Append("\n}");//end root dict
        
        File.WriteAllText(name,content.ToString());
    }
}
