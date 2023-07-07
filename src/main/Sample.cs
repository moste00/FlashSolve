namespace flashsolve.main;

using flashsolve.sampler.algorithms;
using flashsolve.sampler;
using System.Collections.Concurrent;
using System.Threading;
using flashsolve.compiler;

public class Sample
{
    //constants
    private const string ConfigFilePath = "src/main/config.json";
    
    // members
    private readonly Config _configs;
    private readonly uint _numOfOutputs;
    private Dictionary<string, bool> _testAlgorithms;
    private Base _candidateAlgorithm;
    private string _candidateAlgorithmName;
    private Dictionary<string, List<object>> _batchedResults;
    private RandProblem _problem;
    
    // Constructor
    public Sample(uint numOfOutputs, RandProblem problem)
    {
        _configs = new Config(ConfigFilePath);
        _numOfOutputs = numOfOutputs;
        _problem = problem;
        initialize_test_algorithms();
    }

    public void Run()
    {
        while (_candidateAlgorithm == null)
        {
            Console.WriteLine("Info: Started testing the selected algorithms");
            run_test();
            Console.WriteLine("Info: Done testing the selected algorithms");
        }
        Console.WriteLine("************************************************************************************");
        
        if (_numOfOutputs <= _configs.TestingSampleSize)
        {
            Console.WriteLine("Warning: getting data from the test sample (numOfOutputs <= TestingSampleSize)");
            Console.WriteLine("Info: candidate algorithm=    " + _candidateAlgorithmName);
            Helper.print_output_dictionary(_batchedResults);
        }
        else
        {
            Console.WriteLine("Info: running candidate algorithm " + _candidateAlgorithmName);
            _candidateAlgorithm.run_algorithm();
        }
        Console.WriteLine("************************************************************************************");
    }

    private void run_test()
    {
        if (_testAlgorithms["singleAlgorithm"])
        {
            _candidateAlgorithm = get_sampler_algorithms(false).First();
            if (_candidateAlgorithm == null)
                throw new Exception("Exception: there is no valid algorithm selected");
        }
        else
        {
            var algorithms = get_sampler_algorithms(_testAlgorithms["All"]);
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
                thread.Join(TimeSpan.FromSeconds(_configs.TestingTimeLimitSecs));
            }

            evaluate_test_result(results);
   
            if (_candidateAlgorithm == null)
            {
                Console.WriteLine("Warning: threads were timed out and no algo was selected... so we fall back to Naive");
                _candidateAlgorithmName = "Naive";
                _candidateAlgorithm = map_sampler_algorithms_names(_candidateAlgorithmName);
            }
        }
    }

    private void initialize_test_algorithms()
    {
        var selected = _configs.TestingAlgorithmsNaive
                       + _configs.TestingAlgorithmsHash
                       + _configs.TestingAlgorithmsMaxsmt
                       + _configs.TestingAlgorithmsHybrid0
                       + _configs.TestingAlgorithmsHybrid1
                       + _configs.TestingAlgorithmsHybrid2;
        
        _testAlgorithms = new Dictionary<string, bool>()
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
                result = new Naive(_configs, _numOfOutputs, _problem);
                break;
            case "Hash":
                result = new Hash(_configs, _numOfOutputs, _problem);
                break;
            case "Maxsmt":
                Console.WriteLine("Warning: Maxsmt is still under development...... expecting the code to break.");
                break;
            case "Hybrid0":
                result = new Hybrid(_configs, _numOfOutputs, 0, _problem);
                break;
            case "Hybrid1":
                result = new Hybrid(_configs, _numOfOutputs, 1, _problem);
                break;
            case "Hybrid2":
                result = new Hybrid(_configs, _numOfOutputs ,2, _problem);
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

        foreach (var item in _testAlgorithms)
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

    private void evaluate_test_result(ConcurrentDictionary<string, Dictionary<string, List<object>>> results)
    {
        if(results.IsEmpty)
            return;

        double totSpread = 0.0;
        double totTime = 0.0;

        Dictionary<string, List<double>> benchmarks = new Dictionary<string, List<double>>();
        
        foreach (var res in results)
        {
            var spread = Helper.calculate_spread(res.Value);
            var timing = Helper.CalcTimePerSolution(res.Value).Item1;
            totSpread += spread;
            totTime += timing;

            benchmarks[res.Key] = new List<double>() { spread, timing } ;
        }

        double bestScore = 0.0;
        string bestAlgo = "";
        foreach (var benchmark in benchmarks)
        {
            var score = (benchmark.Value[0] / totSpread) * 0.6 + (1 - benchmark.Value[1] / totTime) * 0.4;

            if (score > bestScore)
            {
                bestScore = score;
                bestAlgo = benchmark.Key;
            }
        }
        
        // Console.WriteLine("tot_spread= " + tot_spread + "   tot_time= " + tot_time);
        // Console.WriteLine("best algo is  " + best_algo + "      with score=  " + best_score);
        _candidateAlgorithmName = bestAlgo;
        _candidateAlgorithm = map_sampler_algorithms_names(bestAlgo);


        if (_numOfOutputs <= _configs.TestingSampleSize)
            _batchedResults = results[_candidateAlgorithmName];
    }
}
