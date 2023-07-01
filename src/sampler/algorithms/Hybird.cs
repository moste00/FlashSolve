using flashsolve.compiler;

namespace flashsolve.sampler.algorithms;
using System.Diagnostics;
using Microsoft.Z3;

public class Hybird : Hash
{
    public Hybird(Config configs, uint noOutputs, RandProblem problem) : base(configs, noOutputs, problem)
    {
    }

    

    public void run_hybird_alternate(int type)
    {
        List<List<uint>> hybirdAlgorithms = new List<List<uint>>{ 
            new List<uint> {1, 1},
            new List<uint> {5, 1},
            new List<uint> {20, 1}
        };
        uint currentNumSols = 0;
        bool run_naive = true;
        uint new_currentNumSols;
        
        while (currentNumSols != NoOutputs )
        {
            if(run_naive)
                new_currentNumSols = run_naive_algorithm(currentNumSols + hybirdAlgorithms[type][0], currentNumSols);
            else
                new_currentNumSols = run_hash_algorithm(currentNumSols + hybirdAlgorithms[type][1], currentNumSols);

            run_naive = !run_naive;
            if(new_currentNumSols == currentNumSols)
                break;
            currentNumSols = new_currentNumSols;
        }
        
        print_output_dictionary(NamesToValues);
    }

}