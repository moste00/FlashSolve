namespace flashsolve.sampler.algorithms;
using System.Diagnostics;
using Microsoft.Z3;
using System.Collections.Concurrent;

public class Hybrid : Hash
{
    private int _type;
    public Hybrid(Config configs, uint noOutputs, int type) : base(configs, noOutputs)
    {
        _type = type;
    }

    public void set_type(int val)
    {
        _type = val;
    }
    
    public int get_type()
    {
        return _type;
    }
    
    private void run_hybrid_alternate_algorithm(uint thresh)
    {
        List<List<uint>> hybridAlgorithms = new List<List<uint>>{ 
            new List<uint> {1, 1},
            new List<uint> {5, 1},
            new List<uint> {20, 1}
        };
        uint currentNumSols = 0;
        var runNaive = true;

        while (currentNumSols < thresh )
        {
            uint newCurrentNumSols;
            if (runNaive)
            {
                var algoThresh = Math.Min(currentNumSols + hybridAlgorithms[_type][0], thresh);
                newCurrentNumSols = run_naive_algorithm(algoThresh, currentNumSols);
            }
            else
            {
                var algoThresh = Math.Min(currentNumSols + hybridAlgorithms[_type][1], thresh);
                newCurrentNumSols = run_hash_algorithm(algoThresh, currentNumSols);
            }

            runNaive = !runNaive;
            if(newCurrentNumSols == currentNumSols)
                break;
            currentNumSols = newCurrentNumSols;
        }
    }
    
    public override void run_algorithm()
    {
        run_hybrid_alternate_algorithm(NoOutputs);
        Helper.print_output_dictionary(NamesToValues);
    }
    public override void test_algorithm(ConcurrentDictionary<string, Dictionary<string, List<object>>> results)
    {
        run_hybrid_alternate_algorithm(TestingNoOutputs);
        var added = results.TryAdd($"Hybrid{_type}", NamesToValues);
        if(!added)
            throw new Exception("test_algorithm of (Hybrid) could not add it's results to the ConcurrentDictionary");
    }

}