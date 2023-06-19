namespace flashsolve.util; 
using System.Text.Json;

public class Config
{
    //members
    public int testing_sampleSize;
    public int testing_timeLimitSecs;
    public bool testing_algorithms_naive;
    public bool testing_algorithms_maxsmt;
    public bool testing_algorithms_hash;
    public int output_SampleSize;
    
    
    //constructor
    public Config(string path)
    {
        string jsonString = File.ReadAllText(path);
        JsonDocument document = JsonDocument.Parse(jsonString);

        // Accessing values from the "testing" object
        JsonElement testingDiffAlg = document.RootElement.GetProperty("testing");
        testing_sampleSize = testingDiffAlg.GetProperty("sampleSize").GetInt32();
        testing_timeLimitSecs = testingDiffAlg.GetProperty("timeLimitSecs").GetInt32();

        // Accessing values from the "algorithms" object within "testing"
        JsonElement algorithms = testingDiffAlg.GetProperty("algorithms");
        testing_algorithms_naive = algorithms.GetProperty("naive").GetInt32() == 1;
        testing_algorithms_maxsmt = algorithms.GetProperty("maxsmt").GetInt32() == 1;
        testing_algorithms_hash = algorithms.GetProperty("hash").GetInt32() == 1;

        // Accessing values from the "output" object
        JsonElement output = document.RootElement.GetProperty("output");
        output_SampleSize = output.GetProperty("SampleSize").GetInt32();

    }
}