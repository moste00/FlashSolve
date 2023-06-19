namespace flashsolve.main;

using flashsolve.util.cmdparse;
using static Parse;
using static Sample;
    
public static class FlashSolve {
    enum SubprogramType {
        FlashParser,
        FlashSampler,
        FlashSamplerV1
    }
    
    public static void Main(String[] args) {
        var subprogramArgs = args.Skip(1).ToArray();
        var cmdParser = new CmdArgsParser();
        
        var subprogramOption = new EnumArg<SubprogramType>() { Name = "do" };
        subprogramOption
            .Map("parse".To(SubprogramType.FlashParser),
                 "sample".To(SubprogramType.FlashSampler),
                 "samplev1".To(SubprogramType.FlashSamplerV1))    
            .OnParse((sub) => {
                switch (sub) {
                    case SubprogramType.FlashParser:
                        ParseMain(subprogramArgs);
                        break;
                    case SubprogramType.FlashSampler:
                        SampleMain(subprogramArgs);
                        break;
                    case SubprogramType.FlashSamplerV1:
                        // should create an object of sampleV1 with the correct configs and constraints
                        var samplev1_obj = new SampleV1(1);
                        break;
                }
            })
            .OnError((str) => Console.WriteLine(
                $"{str} is not a valid subprogram and flashsolve couldn't launch it.\n --------- \n Flashsolve can only launch"
                + $"the following subprograms : {String.Join(",",subprogramOption.AllAcceptableValues())}"));
        
        cmdParser.Add(subprogramOption);
        
        var res = cmdParser.Run(args);
    }
}