namespace flashsolve.sampler; 
using System.Text.Json;

public class Config
{
    //members
    public uint TestingSampleSize;
    public double TestingTimeLimitSecs;
    public int TestingAlgorithmsNaive;
    public int TestingAlgorithmsMaxsmt;
    public int TestingAlgorithmsHash;
    public int TestingAlgorithmsHybrid0;
    public int TestingAlgorithmsHybrid1;
    public int TestingAlgorithmsHybrid2;
    public bool SamplerTimer;
    public uint HashConstantsHashSize;
    public uint HashConstantsBitsCounts;
    public bool paralizedHashOn;
    public float paralizedHashThreadPercentage;

    //constructor
    public Config(string path)
    {
        string jsonString = File.ReadAllText(path);
        JsonDocument document = JsonDocument.Parse(jsonString);

        // Accessing values from the "testing" object
        JsonElement testingDiffAlg = document.RootElement.GetProperty("testing");
        TestingSampleSize = testingDiffAlg.GetProperty("sampleSize").GetUInt32();
        TestingTimeLimitSecs = testingDiffAlg.GetProperty("timeLimitSecs").GetDouble();

        // Accessing values from the "algorithms" object within "testing"
        JsonElement algorithms = testingDiffAlg.GetProperty("algorithms");
        TestingAlgorithmsNaive = algorithms.GetProperty("naive").GetInt32();
        TestingAlgorithmsMaxsmt = algorithms.GetProperty("maxsmt").GetInt32();
        TestingAlgorithmsHash = algorithms.GetProperty("hash").GetInt32();
        TestingAlgorithmsHybrid0 = algorithms.GetProperty("hybird0").GetInt32();
        TestingAlgorithmsHybrid1 = algorithms.GetProperty("hybird1").GetInt32();
        TestingAlgorithmsHybrid2 = algorithms.GetProperty("hybird2").GetInt32();
        

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