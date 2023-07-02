namespace flashsolve.main;

using System.Diagnostics;
using System.Text;
using Microsoft.Z3;
using flashsolve.sampler.algorithms;
using flashsolve.sampler;
using System.Collections.Concurrent;
using System.Threading;

public class Sample
{
    //constants
    private const string ConfigFilePath = "src/main/config.json";
    
    // members
    private readonly Config _configs;
    private readonly uint _numOfOutputs;
    private Dictionary<string, bool> testAlgorithms;
    private Base canditidateAlgorithm;
    
    // Constructor
    public Sample(uint numOfOutputs)
    {
        _configs = new Config(ConfigFilePath);
        _numOfOutputs = numOfOutputs;
        initialize_test_algorithms();
    }

    public void run_test()
    {
        if (testAlgorithms["singleAlgorithm"])
        {
            canditidateAlgorithm = get_sampler_algorithms(false).First();
            if (canditidateAlgorithm == null)
                throw new Exception("Exception: there is no valid algorithm selected");
        }
        else
        {
            var algorithms = get_sampler_algorithms(testAlgorithms["All"]);
            ConcurrentDictionary<string, Dictionary<string, List<object>>> results = new  ConcurrentDictionary<string, Dictionary<string, List<object>>>();
            List<Thread> threads = new List<Thread>();
            foreach (var algo in algorithms)
            {
                Thread childThread = new Thread(() => algo.test_algorithm(results));
                childThread.Start();
                threads.Add(childThread);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            foreach (var res in results)
            {
                Console.WriteLine("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
                Console.WriteLine(res.Key);
                Helper.print_output_dictionary(res.Value);
                Console.WriteLine("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
            }
        }
    }

    public void run()
    {
        if (canditidateAlgorithm == null)
        {
            Console.WriteLine("Warning: Started testing the selected algorithms");
            run_test();
            Console.WriteLine("Warning: Done testing the selected algorithms");
        }
        Console.WriteLine("************************************************************************************");
        Console.WriteLine("Info: running cadidate algorithm" + canditidateAlgorithm.GetType());
        canditidateAlgorithm.run_algorithm();
        Console.WriteLine("************************************************************************************");
    }


    private void initialize_test_algorithms()
    {
        var selected = _configs.TestingAlgorithmsNaive
                       + _configs.TestingAlgorithmsHash
                       + _configs.TestingAlgorithmsMaxsmt
                       + _configs.TestingAlgorithmsHybrid0
                       + _configs.TestingAlgorithmsHybrid1
                       + _configs.TestingAlgorithmsHybrid2;
        
        testAlgorithms = new Dictionary<string, bool>()
        {
            { "Naive", _configs.TestingAlgorithmsNaive == 1},
            { "Hash", _configs.TestingAlgorithmsHash == 1},
            { "Maxsmt", _configs.TestingAlgorithmsMaxsmt == 1},
            { "Hybrid0", _configs.TestingAlgorithmsHybrid0 == 1},
            { "Hybrid1", _configs.TestingAlgorithmsHybrid1 == 1},
            { "Hybrid2", _configs.TestingAlgorithmsHybrid2 == 1},
            {"All", selected == 0},
            {"singleAlgorithm", selected == 1}
        };
    }

    private Base map_sampler_algorithms_names(string algorithm)
    {
        Base result = null;
        switch (algorithm)
        {
            case "Naive":
                result = new Naive(_configs, _numOfOutputs);
                break;
            case "Hash":
                result = new Hash(_configs, _numOfOutputs);
                break;
            case "Maxsmt":
                Console.WriteLine("Warning: Maxsmt is still under development...... expecting the code to break.");
                break;
            case "Hybrid0":
                result = new Hybrid(_configs, _numOfOutputs, 0);
                break;
            case "Hybrid1":
                result = new Hybrid(_configs, _numOfOutputs, 1);
                break;
            case "Hybrid2":
                result = new Hybrid(_configs, _numOfOutputs ,2);
                break;
            default:
                Console.WriteLine("Warning: bug in the algorithm name");
                break;
        }

        return result;
    }
    private List<Base> get_sampler_algorithms(bool all)
    {
        List<Base> results = new List<Base>();

        foreach (var item in testAlgorithms)
        {
            if(item.Key == "All" || item.Key == "singleAlgorithm")
                continue;
            if (item.Value || all)
            {
                var algorithm = map_sampler_algorithms_names(item.Key);
                if (algorithm == null)
                {
                    Console.WriteLine("Warning: could not find a matching algorithm");
                    continue;
                }
                results.Add(algorithm);
            }
        }

        return results;
    }
}
