namespace flashsolve.main;

using System.Diagnostics;
using System.Text;
using Microsoft.Z3;
using flashsolve.sampler.algorithms;
using flashsolve.sampler;

public class SampleV1
{
    //constants
    private const string ConfigFilePath = "src/main/config.json";
    
    // members
    private Config _configs;
    private uint _numOfOutputs;
    // note: missing: cadidate algorithm, constraints variable
    
    // Constructor
    public SampleV1(uint numOfOutputs)
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
        var sampler = new Hybird(_configs, 1000);
        Console.WriteLine("************************************************************************************");
        sampler.run_hybird_alternate(2);
        Console.WriteLine("************************************************************************************");
    }


}
