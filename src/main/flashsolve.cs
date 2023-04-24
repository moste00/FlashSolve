using flashsolve.util.cmdparse;

namespace flashsolve.main;

public static class FlashSolve {
    enum SubProgram {
        parse,
        sample
    }
    public static void Main(String[] args) {
        var cmdParser = new CmdArgsParser();
        cmdParser.Add(
            new EnumArg<SubProgram>() {
                    Name = "Do" 
            }
            .OnParse((sub) => {
                switch (sub) {
                        case SubProgram.parse: Console.WriteLine("I'm Parsing now"); break;
                        case SubProgram.sample:  Console.WriteLine("I'm sampling now"); break;
                }
            })
            .OnError((str) => Console.WriteLine($"{str} is not a valid subprogram, flashsolve can only be called with"
                                                    +$"the following options : {String.Join(",",Enum.GetValues<SubProgram>())}"))
        );
        var res = cmdParser.Run(args);
        Console.WriteLine(res);
    }
}