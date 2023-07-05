using flashsolve.compiler;
using flashsolve.util.datastructs;
using Microsoft.Z3;

namespace flashsolve.sampler.algorithms; 

public class SubRand : Base {
    private readonly Solver _solver;
    private readonly Context _ctx;
    private readonly Dictionary<string, BitVecExpr> _namesToExprs;
    private readonly Random _rand;
    
    public SubRand(Config configs, uint noOutputs, RandProblem problem, Random rand) : base(configs, noOutputs, problem) {
        var (ctx,
            constraints,
            namesToExprs) = get_constraints();
        _solver = ctx.MkSolver();
        _ctx = ctx;
        _namesToExprs = namesToExprs;
        _rand = rand;
        
        _solver.Add(constraints);
    }

    public Dictionary<string,List<BitVecExpr>> 
        Run(
            SubRandUtils.ExplorationExploitationPolicy policy, 
            Dictionary<string,SubRandUtils.RangeAwareRandomizer> randomizers
        ) {
        Status stat;
        uint numComputedOutputs = 0;
        StringToIntDict numSuccessfulRandomizations = new StringToIntDict();
        
        BoolExpr[] notEqualPrev = new BoolExpr[_namesToExprs.Count];
        
        while (numComputedOutputs < NoOutputs) {
            stat = _solver.Check();
            if (stat != Status.SATISFIABLE) {
                break;
            }
            
            (Expr[] randomizationAssertions, 
             string[] randomizedVarNames)   = RandomizeSubsetOfVariables(policy, randomizers, numSuccessfulRandomizations);
            stat = _solver.Check(randomizationAssertions);
            if (stat != Status.SATISFIABLE) {
                continue;
            }
            
            var model = _solver.Model!;
            int i = 0;
            foreach (var entry in model.Consts) {
                var variableName = entry.Key.Name.ToString();
                if (variableName[0] == '@') {
                    continue;
                }
                var value = entry.Value;

                notEqualPrev[i++] = _ctx.MkEq(_namesToExprs[variableName], value);
            }

            var notAllExprsEqualFoundValues = _ctx.MkNot(
                _ctx.MkAnd(notEqualPrev)
            )!;
            var autoGenBoolVar = _ctx.MkBoolConst($"@FS_AUTO_GEN_{numComputedOutputs}");
            _solver.Assert(
                _ctx.MkImplies(autoGenBoolVar, notAllExprsEqualFoundValues),
                autoGenBoolVar
            );
            foreach (var varName in randomizedVarNames) {
                numSuccessfulRandomizations.Incr(varName);
            }
            numComputedOutputs++;
        }

        return ExtractSolutions(_solver,_namesToExprs.Keys.ToArray());
    }

    private static Dictionary<string, List<BitVecExpr>> ExtractSolutions(Solver solver, string[] varNames) {
        Dictionary<string, List<BitVecExpr>> sols = new();
        foreach (var varName in varNames) {
            sols[varName] = new List<BitVecExpr>();
        }
        foreach (var assertion in solver.Assertions) {
            if (assertion.IsImplies) {
                var cond = (BoolExpr)assertion.Args[0]!;
                
                if (cond.IsConst && cond.FuncDecl.Name.ToString()[0] == '@') {
                    var consequence = (BoolExpr)assertion.Args[1];
                    
                    if (consequence.IsNot) {
                        var notArg = (BoolExpr)consequence.Args[0]!;
                        if (notArg.IsAnd) {
                            foreach (var andArg in notArg.Args) {
                                var eqArgs = ((BoolExpr)andArg).Args;
                                var e1 = (BitVecExpr)eqArgs[0];
                                var e2 = (BitVecExpr)eqArgs[1];
                                sols[e1.FuncDecl.Name.ToString()].Add(e2);
                            }
                        }
                    }
                }
            }
        }

        return sols;
    }

    private (Expr[],string[]) RandomizeSubsetOfVariables(
                                SubRandUtils.ExplorationExploitationPolicy policy, 
                                Dictionary<string, SubRandUtils.RangeAwareRandomizer> randomizers, 
                                StringToIntDict numSuccessfulRandomizations) {
        while (true) {
            switch (policy.GenAction()) {
                case SubRandUtils.ExplorationExploitationPolicy.PolicyDecision
                    .RandomizeVarWithMaxSuccessfulRandomizations: {
                    var (varName, _) = numSuccessfulRandomizations.GetMax();

                    if (varName != "") {
                        var varExpr = _namesToExprs[varName];
                        var randExpr = randomizers[varName].RandValueFromRange();
                        return (
                            new Expr[] {
                                _ctx.MkEq(varExpr, randExpr)
                            },
                            new string[] {
                                varName
                            }
                        );
                    }
                    break;
                }
                case SubRandUtils.ExplorationExploitationPolicy.PolicyDecision.RandomizeExistingVar: {
                    break;
                }
                case SubRandUtils.ExplorationExploitationPolicy.PolicyDecision.RandomizeNewVar: {
                    foreach (var entry in _namesToExprs) {
                        var varName = entry.Key;
                        if (randomizers.ContainsKey(varName)) {
                            if (!numSuccessfulRandomizations.Contains(varName)) {
                                var varExpr = entry.Value;
                                var randExpr = randomizers[varName].RandValueFromRange();
                                return (
                                    new Expr[] {
                                        _ctx.MkEq(varExpr, randExpr)
                                    },
                                    new string[] {
                                        varName
                                    }
                                );
                            }
                        }
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (numSuccessfulRandomizations.Count == 0) {
                continue;
            }
            int i = _rand.Next(0, numSuccessfulRandomizations.Count);
            var (variableName, _) = numSuccessfulRandomizations.ToArray()[i];
            var variableExpr = _namesToExprs[variableName];
            var randomExpr = randomizers[variableName].RandValueFromRange();
            return (
                new Expr[] {
                    _ctx.MkEq(variableExpr, randomExpr)
                },
                new string[] {
                    variableName
                }
            );
        }
    }
}