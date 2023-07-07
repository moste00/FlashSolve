namespace flashsolve.sampler;

using flashsolve.sampler.algorithms;
using flashsolve.compiler;

public class HandleOutputs
{
    private const string OutputDurationKey = "duration_in_millis";
    
    // members
    private readonly Config _configs;
    private readonly uint _noOutputs;
    private Dictionary<string, List<object>> _namesToValues;
    private uint _currentNoOutputs;
    private RandProblem _problem;
    // Constructor
    // SHOULD TAKE THE CONSTRAINS STRUCT
    public HandleOutputs(Config configs,RandProblem problem ,uint noOutputs, Dictionary<string, List<object>> namesToValues)
    {
        _configs = configs;
        _noOutputs = noOutputs;
        _namesToValues = namesToValues;
        _problem = problem;
        _currentNoOutputs = (uint)namesToValues.Values.Min(list => list.Count);
    }

    public Dictionary<string, List<object>> handle_missing_values()
    {
        if (_currentNoOutputs >= _noOutputs || _currentNoOutputs == 0)
            return _namesToValues;
        uint neededOutputs = _noOutputs - _currentNoOutputs;
        var percentage = ((double)_currentNoOutputs / _noOutputs);
        if ( percentage < 0.75)
        {
            var dict =run_naive(neededOutputs);
            merge_to_names_to_values(dict);
        }

        if (_currentNoOutputs < _noOutputs)
        {
            neededOutputs = _noOutputs - _currentNoOutputs;
            var dict =run_repeat();
            merge_to_names_to_values(dict);
        }

        return _namesToValues;
    }

    private Dictionary<string, List<object>> run_naive(uint neededOutputs)
    {
        Naive sampler = new Naive(_configs, neededOutputs, _problem);
        
        return sampler.run_algorithm();
    }
    
    private Dictionary<string, List<object>> run_repeat()
    {
        Dictionary<string, List<object>> res = new Dictionary<string, List<object>>();

        foreach (var key in _namesToValues.Keys)
        {
            res[key] = new List<object>();
        }

        var originalMax = _currentNoOutputs;
        Random random = new Random();
        
        while (_currentNoOutputs < _noOutputs)
        {
            var randomNumber = random.Next(0, (int)originalMax);
            foreach (var key in res.Keys)
            {
                if (key == OutputDurationKey)
                {
                    res[key].Add(0.0);
                    continue;
                }
                res[key].Add(_namesToValues[key][randomNumber]);
            }

            _currentNoOutputs++;
        }

        return res;
    }

    private void merge_to_names_to_values(Dictionary<string, List<object>> srcDict)
    {
        foreach (var kvp in srcDict)
        {
            string key = kvp.Key;
            List<object> values = kvp.Value;

            if (_namesToValues.ContainsKey(key))
            {
                _namesToValues[key].AddRange(values);
            }
        }
        _currentNoOutputs = (uint)_namesToValues.Values.Min(list => list.Count);
    }

    public static bool did_not_reached_no_outputs(uint noOutputs, Dictionary<string, List<object>> namesToValues)
    {
        uint length = (uint)namesToValues.Values.Min(list => list.Count);
        return length < noOutputs;
    }
}