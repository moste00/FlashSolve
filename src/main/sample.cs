using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Z3;

namespace FlashSolve.main;
public static class Sample {
    public static void SMain(String[] args) {
        var strategy = args[0];
        var ctx = new Context();
        
        var solver = ctx.MkSolver();

        var syms = new Symbol[] {
            ctx.MkSymbol("a"), ctx.MkSymbol("b"), ctx.MkSymbol("c")
        };
        var a = ctx.MkIntConst(syms[0])!;
        var b = ctx.MkIntConst(syms[1])!;
        var c = ctx.MkIntConst(syms[2])!;
        var namesToExprs = new Dictionary<string, IntExpr>(){
            { "a", a },
            { "b", b },
            { "c", c }
        };
        
        solver.Add(a > 0    , b > 0    , c > 0, 
                                 a < 1000, b < 25, c < 1000,
                                 (b*b - 4*a*c) > 0);

        var namesToValues = new Dictionary<String, List<int>>(){
            {"a",new List<int>()},
            {"b",new List<int>()},
            {"c",new List<int>()}
        };

        if (strategy == "NAIVE") {
            var log = new StringBuilder();
            var stopwatch = new Stopwatch();
            var result = solver.Check();
            
            while (result == Status.SATISFIABLE) {
                var model = solver.Model!;
                log.Append(model);
                log.Append("\n--------------------------------------------------------------------------------------------------------------------------------------\n");
            
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
            }
            Console.WriteLine("Done!");
            Console.WriteLine($"Found {namesToValues["a"].Count} Solutions in {stopwatch.Elapsed.Seconds} seconds.");
            File.WriteAllText("solutions.txt",log.ToString());
        }
        else if (strategy == "MAXSMT") {
            var optimizer = ctx.MkOptimize()!;
            optimizer.FromString(@"
                (declare-const a Int)
                (assert-soft (> a 1))
            ");
            var result = optimizer.Check();
            Console.WriteLine(result);
            Console.WriteLine(optimizer.Objectives);
            Console.WriteLine(optimizer.Model);
        }
    }
}
