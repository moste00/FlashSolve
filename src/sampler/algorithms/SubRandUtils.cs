using flashsolve.compiler;
using Microsoft.Z3;

namespace flashsolve.sampler.algorithms; 

public static class SubRandUtils {
    
    public interface ExplorationExploitationPolicy {
        PolicyDecision GenAction();
        PolicyDecision GenAction(bool lastRandomizationWasSuccessful);
        enum PolicyDecision {
            RandomizeVarWithMaxSuccessfulRandomizations,
            RandomizeExistingVar,
            RandomizeNewVar
        }
        public class LambdaWrapper<T> : ExplorationExploitationPolicy {
            private Func<T,PolicyDecision> _lambda;
            private Func<bool, T, PolicyDecision> _anotherLambda;
            private T _state;
            public LambdaWrapper(T initialState, 
                                 Func<T,PolicyDecision> lambda,
                                 Func<bool,T,PolicyDecision> anotherLambda) {
                _lambda = lambda;
                _state = initialState;
                _anotherLambda = anotherLambda;
            }

            public PolicyDecision GenAction() {
                return _lambda(_state);
            }

            public PolicyDecision GenAction(bool lastRandomizationWasSuccessful) {
                return _anotherLambda(lastRandomizationWasSuccessful,_state);
            }
        }
        public class LambdaWrapperNoState : ExplorationExploitationPolicy {
            private Func<PolicyDecision> _lambda;
            private Func<bool, PolicyDecision> _anotherLambda;
            public LambdaWrapperNoState(Func<PolicyDecision> lambda,
                                      Func<bool,PolicyDecision> anotherLambda) {
                _lambda = lambda;
                _anotherLambda = anotherLambda;
            }

            public PolicyDecision GenAction() {
                return _lambda();
            }

            public PolicyDecision GenAction(bool lastRandomizationWasSuccessful) {
                return _anotherLambda(lastRandomizationWasSuccessful);
            }
        }
        public static ExplorationExploitationPolicy From(Func<PolicyDecision> function,
                                                         Func<bool,PolicyDecision> functionBoolArg) {
            return new LambdaWrapperNoState(function,functionBoolArg);
        }
    }
    //In the steady state, behaves as follows : with a small probability epsilon, chooses exploration
    //Otherwise, mostly chooses exploitation
    //Except, EpsilonGreedy policies doesn't start in the steady state, they start "adventurous", meaning they 
    //prefer exploration much more when young, then gradually they start leaning more and more into exploitation
    public class EpsilonGreedy : ExplorationExploitationPolicy {
        private double _epsilon;
        private double _decreaseBy = 0.05;  //the probability _epsilon will decrease by every certain number of iterations
        private int _every;                 //the number of iterations _epsilon will decrease after
        private double _until;              //the final probability that _epsilon will not decrease after 
        private int _numIterationsSoFar;
        private Random _rand;
        private ExplorationExploitationPolicy.PolicyDecision _adventurousAction;
        private ExplorationExploitationPolicy.PolicyDecision _lastAction;
        public EpsilonGreedy(double decreaseBy = 0.05,
                             int every = 10,           
                             double until = 0.05) {
            if (decreaseBy <= 0.0 || decreaseBy >= 1.0) {
                decreaseBy = 0.05;
            }
            if (every <= 0) {
                every = 10;
            }

            if (until <= 0.0 || until >= 1.0) {
                until = 0.05;
            }
            _epsilon = 1.0;
            _decreaseBy = decreaseBy;
            _every = every;
            _until = until;
            _numIterationsSoFar = 0;
            _rand = new Random();
            _adventurousAction = ExplorationExploitationPolicy.PolicyDecision.RandomizeNewVar;
        }
        public ExplorationExploitationPolicy.PolicyDecision GenAction() {
            ExplorationExploitationPolicy.PolicyDecision decision = (_rand.NextDouble() <= _epsilon) ? 
                                                                    _adventurousAction: 
                                                                    ExplorationExploitationPolicy.PolicyDecision.RandomizeVarWithMaxSuccessfulRandomizations;
            Iter();
            _lastAction = decision;
            return decision;
        }

        public ExplorationExploitationPolicy.PolicyDecision GenAction(bool lastRandomizationWasSuccessful) {
            ExplorationExploitationPolicy.PolicyDecision decision;
            if (lastRandomizationWasSuccessful) {
                decision = (_rand.NextDouble() <= _epsilon) ? 
                            _adventurousAction: 
                            _lastAction;
            } else {
                decision = (_rand.NextDouble() <= _epsilon) ? 
                            _adventurousAction: 
                            ExplorationExploitationPolicy.PolicyDecision.RandomizeVarWithMaxSuccessfulRandomizations; 
            }
            
            Iter();
            _lastAction = decision;
            return decision;
        }

        private void Iter() {
            _numIterationsSoFar++;
            if (_numIterationsSoFar % _every == 0) {
                if (_epsilon - _decreaseBy >= _until) {
                    _epsilon -= _decreaseBy;
                }

                if (_adventurousAction == ExplorationExploitationPolicy.PolicyDecision.RandomizeNewVar) {
                    _adventurousAction = ExplorationExploitationPolicy.PolicyDecision.RandomizeExistingVar;
                } else {
                    _adventurousAction = ExplorationExploitationPolicy.PolicyDecision.RandomizeNewVar;
                }
            }
        }
    }
    //------------------------------------------------------------------------------------------------------------------
    public interface RangeAwareRandomizer {
        BitVecExpr RandValueFromRange();
    }
    
    //Is not actually "aware" of anything except the length of the variable being randomized
    //This is the bare minimum, it doesn't take into account any range constraints that might vastly narrow down the 
    //valid range
    //e.g. for a 8-bit variable x where there is a constraint "x == 1", it will still generate any value in [0,255]
    public class BlindRandomizer : RangeAwareRandomizer {
        private uint _lengthBits;
        private Context _ctx;

        private Random _rand;

        //used if and only if _lengthBits is >= 64, because the generated value won't fit in any native types
        //(technically strings and string builders can be used, but the Z3 API won't accept anything except decimal 
        //and that makes things harder, so the only alternative left is bool[]) 
        private bool[]? _bigBitVec;

        public BlindRandomizer(uint lenbits, Context ctx) {
            _lengthBits = lenbits;
            _ctx = ctx;
            _rand = new Random();
            _bigBitVec = _lengthBits >= 64 ? 
                        new bool[_lengthBits] : 
                        null;
        }

        public BitVecExpr RandValueFromRange() {
            if (_lengthBits < 64) {
                return _ctx.MkBV(
                    _rand.Next(0, 1 << ((int)_lengthBits)),
                    _lengthBits
                );
            }

            for (int i = 0; i < _lengthBits - 1; i++) {
                _bigBitVec[i] = _rand.NextBool();
            }
            return _ctx.MkBV(_bigBitVec);
        }
    }

    public static bool NextBool(this Random rand, double trueProbability = 0.5) {
        return rand.NextDouble() < trueProbability;
    }

    public static Dictionary<string, BitVecExpr> NonOverconstrainedVars(this RandProblem problem) {
        Dictionary<string, BitVecExpr> nonOverconstrainedVars = new();
        
        var overConstrainedVars = problem.OverconstrainedVars();
        foreach (var (varName,varExpr) in problem.Vars) {
            if (!overConstrainedVars.Contains(varName)) {
                nonOverconstrainedVars[varName] = varExpr;
            }
        }

        return nonOverconstrainedVars;
    }

    //Overconstrained variables are those who have constraints of the form "var == some_value" at the top level
    //The "Top Level" means a constraint appears it's own named block, either alone or among other constraints
    //This is significant because all top levels has to be satisfied (they are basically one ANDed together in one big AND)
    //So expressions of the form "var == some_value" in the top level means the variable must have some_value in any solution
    //Which, by definition, makes it "overconstrained", it's effectively incapable of being randomized
    public static HashSet<string> OverconstrainedVars(this RandProblem problem) {
        return problem.Constraints.OverconstrainedVars();
    }
    public static HashSet<string> OverconstrainedVars(this BoolExpr[] constraints) {
        HashSet<string> overconstrainedVarNames = new();
        foreach (var toplevelBlock in constraints) {
            var blockOverconstrainedVars = toplevelBlock.OverconstrainedVars();
            overconstrainedVarNames.UnionWith(blockOverconstrainedVars);
        }
        return overconstrainedVarNames;
    }
    
    public static HashSet<string> OverconstrainedVars(this BoolExpr constraint) {
        if (constraint.IsEq) {
            var exprs = constraint.Args!;
            
            if (exprs.Length == 2) {
                var e1 = exprs[0]!;
                var e2 = exprs[1]!;
                    
                if (e1.IsBV && e2.IsBV) {
                    if ((     e1.IsConst 
                         || ((e1.IsBVSignExtension || e1.IsBVZeroExtension) && e1.Args[0].IsBV && e1.Args[0].IsConst))
                    &&   e2.IsNumeral) {
                        var e1Bv = (BitVecExpr)e1;
                        return new HashSet<string>() {
                            (e1Bv.IsConst)? e1Bv.FuncDecl.Name.ToString():
                                            e1Bv.Args[0].FuncDecl.Name.ToString()
                        };
                    }
                    if (e1.IsNumeral 
                    && (      e2.IsConst 
                         || ((e2.IsBVSignExtension || e2.IsBVZeroExtension) && e2.Args[0].IsBV && e2.Args[0].IsConst))) {
                        var e2Bv = (BitVecExpr)e2;
                        return new HashSet<string>() {
                            (e2Bv.IsConst)? e2Bv.FuncDecl.Name.ToString():
                                            e2Bv.Args[0].FuncDecl.Name.ToString()
                        };
                    }
                }
            }
        }
        else if (constraint.IsAnd) {
            return (constraint.Args!).OverconstrainedVars() ;
        }
        //empty set
        return new HashSet<string>();
    }
    public static HashSet<string> OverconstrainedVars(this Expr[] constraints) {
        HashSet<string> overconstrainedVarNames = new();
        foreach (var toplevelBlock in constraints) {
            if (toplevelBlock.IsBool) {
                var b = (BoolExpr)toplevelBlock;
                var blockOverconstrainedVars = b.OverconstrainedVars();
                overconstrainedVarNames.UnionWith(blockOverconstrainedVars);
            }
            
        }
        return overconstrainedVarNames;
    }
}