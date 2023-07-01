using flashsolve.compiler;
using flashsolve.parser.ast;
using flashsolve.parser.invoker.antlrinvoker;

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
        string svSource = @"""
        class Pkt;
	        rand bit [7:0] addr;
	        rand bit [7:0] data;
	        
	        constraint addr_limit { 
		        addr*data <= 8'hB; 
		        addr == 'h4;
	        }
        endclass
        """;
        var inv = new AntlrInvoker(); 
        inv.add_file("./grammar-runners/input/1.txt");
        var compiler = new Sv2Z3Compiler();
        var problem = compiler.Compile((SvConstraintProgram)inv.Ast[0]);
        
        var sampler = new Hybird(_configs, 1000, problem);
        Console.WriteLine("************************************************************************************");
        sampler.run_hybird_alternate(2);
        Console.WriteLine("************************************************************************************");
    }


}
