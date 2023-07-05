using flashsolve.compiler;
using flashsolve.parser.ast;
using flashsolve.parser.invoker.antlrinvoker;
using flashsolve.util.datastructs;

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
    private Config _configs;
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

    public void run() {
        // should run the candidate algorithm with respect to the output configs
        // then writes the output results
        var inv = new AntlrInvoker(); 
        inv.add_file("F:/7.GP/FlashSolve/Tests/uniqueness1.txt");
        var compiler = new Sv2Z3Compiler();
        var problem = compiler.Compile((SvConstraintProgram)inv.Ast[0]);

        var sampler = 
            new Hash(_configs, _numOfOutputs, problem); // new SubRand(_configs, _numOfOutputs, problem, new Random())
        sampler.run_hash();
        Console.WriteLine("Sample Done Successfully");
        
    // var randomizers = new Dictionary<string, SubRandUtils.RangeAwareRandomizer>();
    // foreach (var entry in problem.NonOverconstrainedVars()) {
    //     randomizers[entry.Key] = new SubRandUtils.BlindRandomizer(entry.Value.SortSize,problem.Context);
    // }
    // sampler.Run(
    //     new SubRandUtils.EpsilonGreedy(),
    //     randomizers
    // );
    
    }
}
