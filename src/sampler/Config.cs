namespace flashsolve.sampler; 
using System.Text.Json;

public class Config
{
    //members
    public uint TestingSampleSize;
    public uint TestingTimeLimitSecs;
    public bool TestingAlgorithmsNaive;
    public bool TestingAlgorithmsMaxsmt;
    public bool TestingAlgorithmsHash;
    public bool SamplerTimer;
    public uint HashConstantsHashSize;
    public uint HashConstantsBitsCounts;
    public bool paralizedHashOn;
    public float paralizedHashThreadPercentage;
    public bool TestingAlgorithmsHybird0;
    public bool TestingAlgorithmsHybird1;
    public bool TestingAlgorithmsHybird2;
    
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
        TestingAlgorithmsHybird0 = algorithms.GetProperty("hybird0").GetInt32() == 1;
        TestingAlgorithmsHybird1 = algorithms.GetProperty("hybird1").GetInt32() == 1;
        TestingAlgorithmsHybird2 = algorithms.GetProperty("hybird2").GetInt32() == 1;
        

        // Accessing values from the "sampler" object
        JsonElement sampler = document.RootElement.GetProperty("sampler");
        SamplerTimer = sampler.GetProperty("timer").GetInt32() == 1;

        // Accessing values from the "paralizedHash" object
        JsonElement paralizedHash = sampler.GetProperty("paralizedHash");
        paralizedHashOn = paralizedHash.GetProperty("on").GetUInt32() == 1;
        paralizedHashThreadPercentage = paralizedHash.GetProperty("threadPercentage").GetUInt32();
        if (paralizedHashThreadPercentage > 100)
            paralizedHashThreadPercentage = 100;

        // Accessing values from the "hashConstants" object
        JsonElement hashConstants = sampler.GetProperty("hashConstants");
        HashConstantsHashSize = hashConstants.GetProperty("hashSize").GetUInt32();
        HashConstantsBitsCounts = hashConstants.GetProperty("bitsCounts").GetUInt32();
    }
}