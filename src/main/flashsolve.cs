namespace flashsolve.main;

using util.cmdparse;
using static Parse;
using static Compile;
public static class FlashSolve {
    enum SubprogramType {
        FlashParser,
        FlashCompiler,
        FlashSampler
    }
    
    public static void Main(String[] args) {
        var subprogramArgs = args.Skip(1).ToArray();
        var cmdParser = new CmdArgsParser();
        
        var subprogramOption = new EnumArg<SubprogramType>() { Name = "do" };
        subprogramOption
            .Map("parse".To(SubprogramType.FlashParser),
                 "sample".To(SubprogramType.FlashSampler),
                 "compile".To(SubprogramType.FlashCompiler))    
            .OnParse((sub) => {
                switch (sub) {
                    case SubprogramType.FlashParser:
                        ParseMain(subprogramArgs);
                        break;
                    case SubprogramType.FlashSampler:
                        var sv1 = new Sample(100000);
                        sv1.run();
                        //SampleMain(subprogramArgs);
                        break;
                    case SubprogramType.FlashCompiler:
                        CompileMain(subprogramArgs);
                        break;
                }
            })
            .OnError((str) => Console.WriteLine(
                $"{str} is not a valid subprogram and flashsolve couldn't launch it.\n --------- \n Flashsolve can only launch"
                + $"the following subprograms : {String.Join(",",subprogramOption.AllAcceptableValues())}"));
        
        cmdParser.Add(subprogramOption);
        
        cmdParser.Run(args);
    }
}