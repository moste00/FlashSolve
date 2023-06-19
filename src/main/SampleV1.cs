namespace flashsolve.main;

using System.Diagnostics;
using System.Text;
using Microsoft.Z3;
using flashsolve.util;

public class SampleV1
{
    //constants
    private const string ConfigFilePath = "src/main/config.json";
    
    // members
    private Config Configs;
    private int NumOfOutputs;
    // note: missing: cadidate algorithm, constraints variable
    
    // Constructor
    public SampleV1(int numOfOutputs)
    {
        Configs = new Config(ConfigFilePath);
        NumOfOutputs = numOfOutputs;
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
    }


}
