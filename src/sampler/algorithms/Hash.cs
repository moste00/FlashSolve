namespace flashsolve.sampler.algorithms;
using System.Diagnostics;
using Microsoft.Z3;
using System.Collections.Concurrent;
using flashsolve.compiler;

public class Hash: Naive
{
    //constants
    private const uint InputSize = 32;
    
    private readonly uint _hashSize;
    private readonly uint _bitsCounts;
    private readonly BitVecExpr _input;
    private readonly BitVecExpr _hash;

    public Hash(Config configs, uint noOutputs, RandProblem problem) : base(configs, noOutputs, problem)
    {
        NamesToValues = create_output_dictionary(NamesToExprs, true);
        _hashSize = configs.HashConstantsHashSize;
        _bitsCounts = configs.HashConstantsBitsCounts;
        _input = generate_input_bits();
        _hash = generate_hash_bits();
    }

    private List<List<uint>> get_memoized_hash_bits_idx()
    {
        List<List<uint>> memoizedHashBits = new List<List<uint>>{ 
            new List<uint> { 0, 1, 2, 3, 4, 5, 6, 7},
            new List<uint> { 8, 9, 10, 11, 12, 13, 14, 15},
            new List<uint> {16, 17, 18, 19, 20, 21, 22, 23},
            new List<uint> { 24, 25, 26, 27, 28, 29, 30, 31},
            new List<uint> {0 , 2 , 4 , 6 , 8 , 10 , 12 , 14},
            new List<uint> {16, 18, 20, 22, 24, 26, 28, 30},
            new List<uint> {1 , 4 , 7 , 10 , 13 , 16 , 19 , 22},
            new List<uint> {1 , 5 , 9 , 13 , 17 , 21 , 25 , 29},
            new List<uint> {2 , 5 , 8 , 11 , 14 , 17 , 20 , 23 },
            new List<uint> { 1 , 3 , 5 , 7 , 9 , 11 , 13 , 15},
            new List<uint> { 17 , 19 , 21 , 23 , 25 , 27 ,29 , 31},
            new List<uint> {0, 1, 2, 3, 28, 29, 30, 31},
            new List<uint> {3 , 6 ,9 , 12 , 15 , 18 , 21 , 24},
            new List<uint> {0, 5, 10, 15, 20, 25, 30, 31},
        };

        return memoizedHashBits;
    }
    private BoolExpr extract_hash_bits(BitVecExpr hash)
    {
        if (_bitsCounts != 8)
            throw new Exception("There is no implementation for other than 8 bitCounts yet.");
        var hashBitsIdx = get_memoized_hash_bits_idx();

        BoolExpr allHashBitsExprs = null;
        var bvBitHashType = Ctx.MkBitVecSort(1);

        BitVecExpr totalHashVecExpr = null;

        // create all hash bits expers
        for (int idx = 0; idx < _hashSize; idx++)
        {
            // gen new hashBits idxes
            List<uint> hashBits = hashBitsIdx[idx];
            // create a constant for the new hash
            var hashIdx = (BitVecExpr)Ctx.MkConst("hash" + idx, bvBitHashType);
            // xor first 2-bits
            BitVecExpr hashVecExpr = Ctx.MkBVXOR(Ctx.MkExtract(hashBits[0], hashBits[0], _input),
                Ctx.MkExtract(hashBits[1], hashBits[1], _input));
            // xor the rest of the bits
            for (int i = 2; i < _bitsCounts; i++)
            {
                hashVecExpr = Ctx.MkBVXOR(hashVecExpr, Ctx.MkExtract(hashBits[i], hashBits[i], _input));
            }
            //concat the hashIdx to the total hash constant
            if (totalHashVecExpr == null)
                totalHashVecExpr = hashIdx;
            else
                totalHashVecExpr = Ctx.MkConcat(totalHashVecExpr, hashIdx);

            // create the hash bool expr
            BoolExpr hashBoolExpr = Ctx.MkEq(hashIdx, hashVecExpr);
            // add it to the all hash bits expr
            if (allHashBitsExprs == null)
            {
                allHashBitsExprs = hashBoolExpr;
            }
            else
            {
                allHashBitsExprs = Ctx.MkAnd(allHashBitsExprs, hashBoolExpr)!;
            }
        }
        // make the total hash BoolExpr
        BoolExpr totalHashBoolExpr = Ctx.MkEq(hash, totalHashVecExpr);
        allHashBitsExprs = Ctx.MkAnd(allHashBitsExprs, totalHashBoolExpr);
        
        return allHashBitsExprs;
    }
    
    private BitVecExpr generate_hash_bits()
    {
        var bvHashType = Ctx.MkBitVecSort(_hashSize);
        var hash = (BitVecExpr)Ctx.MkConst("hash", bvHashType);
        var hashConstraint = new[] {
            extract_hash_bits(hash)
        };
        Solver.Add(hashConstraint);
        return hash;
    }
    private BitVecExpr generate_input_bits()
    {
        var bvInputType = Ctx.MkBitVecSort(InputSize);
        var input = (BitVecExpr)Ctx.MkConst("input", bvInputType);
        var exprs = NamesToExprs.Values.ToList();
        Helper.shuffle_expr_list(ref exprs);
        
        
        var inputConstraint = Helper.extract_input_constraint(input, exprs, Ctx);
        Solver.Add(inputConstraint);
        return input;
    }
    
    protected uint run_hash_algorithm(uint thresh = 1, uint currentNumSols = 0)
    {
        var stopwatch = new Stopwatch();
        Status result;

        do
        {
            if (Timer)
                result = check_with_timer(stopwatch);
            else
                result = Solver.Check();
            
            if (result != Status.SATISFIABLE)
                break;
            
            var model = Solver.Model!;

            BoolExpr allVariablesHaveNewValues = null;
            foreach (var con in model.Consts) {
                var constName = con.Key.Name.ToString();
                if (constName == "hash")
                {
                    NamesToValues[constName].Add(
                        con.Value
                    );

                    allVariablesHaveNewValues = Ctx.MkAnd(Ctx.MkEq(_hash, con.Value));
                }
                else if (!constName.Contains("hash") && constName != "input")
                {
                    NamesToValues[constName].Add(
                        con.Value
                    );
                }
            }
            allVariablesHaveNewValues = Ctx.MkNot(allVariablesHaveNewValues)!;
                
            Solver.Add(allVariablesHaveNewValues);

            currentNumSols++;
            if (currentNumSols == NoOutputs || currentNumSols == thresh) {
                break;
            }
        } while (result == Status.SATISFIABLE);

        return currentNumSols;
    }
    
    public override void run_algorithm()
    {
        run_hash_algorithm(NoOutputs);
        Helper.print_output_dictionary(NamesToValues);
    }
    
    public override void test_algorithm(ConcurrentDictionary<string, Dictionary<string, List<object>>> results)
    {
        run_hash_algorithm(TestingNoOutputs);
        var added = results.TryAdd("Hash", NamesToValues);
        if(!added)
            throw new Exception("test_algorithm of (Hash) could not add it's results to the ConcurrentDictionary");
    }
}