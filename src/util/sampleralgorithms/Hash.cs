namespace flashsolve.util.sampleralgorithms;
using System.Diagnostics;
using Microsoft.Z3;

public class Hash: Naive
{
    //constants
    private const uint InputSize = 32;
    
    protected uint hashSize;
    protected uint bitsCounts;
    private BitVecExpr input;
    private BitVecExpr hash;

    public Hash(Config configs, uint no_outputs) : base(configs, no_outputs)
    {
        namesToValues = create_output_dictionary(namesToExprs, true);
        hashSize = configs.hashConstants_hashSize;
        bitsCounts = configs.hashConstants_bitsCounts;
        input = generate_input_bits();
        hash = generate_hash_bits();
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
        if (bitsCounts != 8)
            throw new Exception("There is no implementation for other than 8 bitcounts yet.");
        var hashbits_idx = get_memoized_hash_bits_idx();

        BoolExpr allHashBitsExprs = null;
        var bvbitHashType = ctx.MkBitVecSort(1);

        BitVecExpr totalHashVecExpr = null;

        // create all hash bits expers
        for (int idx = 0; idx < hashSize; idx++)
        {
            // gen new hashBits idxes
            List<uint> hashBits = hashbits_idx[idx];
            // create a constant for the new hash
            var hash_i = (BitVecExpr)ctx.MkConst("hash" + idx, bvbitHashType);
            // xor first 2-bits
            BitVecExpr hashVecExpr = ctx.MkBVXOR(ctx.MkExtract(hashBits[0], hashBits[0], input),
                ctx.MkExtract(hashBits[1], hashBits[1], input));
            // xor the rest of the bits
            for (int i = 2; i < bitsCounts; i++)
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

        // BoolExpr [] mergedExpr = {totalHashBoolExpr, allHashBitsExprs} ;
        return allHashBitsExprs;
    }
    
    private BitVecExpr generate_hash_bits()
    {
        var bvHashType = ctx.MkBitVecSort(hashSize);
        var hash = (BitVecExpr)ctx.MkConst("hash", bvHashType);
        var hashConstraint = new[] {
            extract_hash_bits(hash)
        };
        solver.Add(hashConstraint);
        return hash;
    }
    private BitVecExpr generate_input_bits()
    {
        var bvInputType = ctx.MkBitVecSort(InputSize);
        var input = (BitVecExpr)ctx.MkConst("input", bvInputType);
        var exprs = namesToExprs.Values.ToList();
        Helper.shuffle_expr_list(ref exprs);
        
        
        var inputConstraint = Helper.extract_input_constraint(input, exprs, ctx);
        solver.Add(inputConstraint);
        return input;
    }
}