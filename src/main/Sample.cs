namespace flashsolve.main;

using System.Diagnostics;
using System.Text;
using Microsoft.Z3;
using flashsolve.sampler.algorithms;
using flashsolve.sampler;

public class Sample
{
    //constants
    private const string ConfigFilePath = "src/main/config.json";
    
    // members
    private readonly Config _configs;
    private uint _numOfOutputs;
    // note: missing: cadidate algorithm, constraints variable
    
    // Constructor
    public Sample(uint numOfOutputs)
    {
        _configs = new Config(ConfigFilePath);
        _numOfOutputs = numOfOutputs;
    }

    public void run_test()
    {
        // runs the algorithms in the config
        // then choose the best one

    }

    public void run()
    {
        // should run the candidate algorithm with respect to the output configs
        // then writes the output results
        var sampler = new Hybrid(_configs, _numOfOutputs,2);
        Console.WriteLine("************************************************************************************");
        sampler.run_hybrid_alternate();
        Console.WriteLine("************************************************************************************");
    }


}
