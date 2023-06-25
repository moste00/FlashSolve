using flashsolve.compiler;
using Microsoft.Z3;

namespace flashsolve.main;

using flashsolve.util.cmdparse;
using static Parse;
using static Sample;
    
public static class FlashSolve {
    enum SubprogramType {
        FlashParser,
        FlashSampler
    }
    
    public static void Main(String[] args) {
        var subprogramArgs = args.Skip(1).ToArray();
        var cmdParser = new CmdArgsParser();
        
        var subprogramOption = new EnumArg<SubprogramType>() { Name = "do" };
        subprogramOption
            .Map("parse".To(SubprogramType.FlashParser),
                 "sample".To(SubprogramType.FlashSampler))    
            .OnParse((sub) => {
                switch (sub) {
                    case SubprogramType.FlashParser:
                        ParseMain(subprogramArgs);
                        break;
                    case SubprogramType.FlashSampler:
                        SampleMain(subprogramArgs);
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