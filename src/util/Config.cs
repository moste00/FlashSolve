namespace flashsolve.util; 
using System.Text.Json;

public class Config
{
    //members
    public uint TestingSampleSize;
    public uint TestingTimeLimitSecs;
    public bool TestingAlgorithmsNaive;
    public bool TestingAlgorithmsMaxsmt;
    public bool TestingAlgorithmsHash;
    public uint OutputSampleSize;
    public bool SamplerTimer;
    public bool SamplerParalization;
    public uint HashConstantsHashSize;
    public uint HashConstantsBitsCounts;
    
    
    //constructor
    public Config(string path)
    {
        string jsonString = File.ReadAllText(path);
        JsonDocument document = JsonDocument.Parse(jsonString);

        // Accessing values from the "testing" object
        JsonElement testingDiffAlg = document.RootElement.GetProperty("testing");
        TestingSampleSize = testingDiffAlg.GetProperty("sampleSize").GetUInt32();
        TestingTimeLimitSecs = testingDiffAlg.GetProperty("timeLimitSecs").GetUInt32();

        // Accessing values from the "algorithms" object within "testing"
        JsonElement algorithms = testingDiffAlg.GetProperty("algorithms");
        TestingAlgorithmsNaive = algorithms.GetProperty("naive").GetInt32() == 1;
        TestingAlgorithmsMaxsmt = algorithms.GetProperty("maxsmt").GetInt32() == 1;
        TestingAlgorithmsHash = algorithms.GetProperty("hash").GetInt32() == 1;

        // Accessing values from the "output" object
        JsonElement output = document.RootElement.GetProperty("output");
        OutputSampleSize = output.GetProperty("SampleSize").GetUInt32();
        
        
        // Accessing values from the "sampler" object
        JsonElement sampler = document.RootElement.GetProperty("sampler");
        SamplerTimer = sampler.GetProperty("timer").GetInt32() == 1;
        SamplerParalization = sampler.GetProperty("paralization").GetInt32() == 1;
        
        // Accessing values from the "hashConstants" object
        JsonElement hashConstants = sampler.GetProperty("hashConstants");
        HashConstantsHashSize = hashConstants.GetProperty("hashSize").GetUInt32();
        HashConstantsBitsCounts = hashConstants.GetProperty("bitsCounts").GetUInt32();
        

    }
}