namespace flashsolve.util; 
using System.Text.Json;

public class Config
{
    //members
    public uint testing_sampleSize;
    public uint testing_timeLimitSecs;
    public bool testing_algorithms_naive;
    public bool testing_algorithms_maxsmt;
    public bool testing_algorithms_hash;
    public uint output_SampleSize;
    public bool sampler_timer;
    public bool sampler_paralization;
    public uint hashConstants_hashSize;
    public uint hashConstants_bitsCounts;
    
    
    //constructor
    public Config(string path)
    {
        string jsonString = File.ReadAllText(path);
        JsonDocument document = JsonDocument.Parse(jsonString);

        // Accessing values from the "testing" object
        JsonElement testingDiffAlg = document.RootElement.GetProperty("testing");
        testing_sampleSize = testingDiffAlg.GetProperty("sampleSize").GetUInt32();
        testing_timeLimitSecs = testingDiffAlg.GetProperty("timeLimitSecs").GetUInt32();

        // Accessing values from the "algorithms" object within "testing"
        JsonElement algorithms = testingDiffAlg.GetProperty("algorithms");
        testing_algorithms_naive = algorithms.GetProperty("naive").GetInt32() == 1;
        testing_algorithms_maxsmt = algorithms.GetProperty("maxsmt").GetInt32() == 1;
        testing_algorithms_hash = algorithms.GetProperty("hash").GetInt32() == 1;

        // Accessing values from the "output" object
        JsonElement output = document.RootElement.GetProperty("output");
        output_SampleSize = output.GetProperty("SampleSize").GetUInt32();
        
        
        // Accessing values from the "sampler" object
        JsonElement sampler = document.RootElement.GetProperty("sampler");
        sampler_timer = sampler.GetProperty("timer").GetInt32() == 1;
        sampler_paralization = sampler.GetProperty("paralization").GetInt32() == 1;
        
        // Accessing values from the "hashConstants" object
        JsonElement hashConstants = sampler.GetProperty("hashConstants");
        hashConstants_hashSize = hashConstants.GetProperty("hashSize").GetUInt32();
        hashConstants_bitsCounts = hashConstants.GetProperty("bitsCounts").GetUInt32();
        

    }
}