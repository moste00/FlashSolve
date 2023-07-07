namespace flashsolve.main;

using util.cmdparse;
using static Parse;
using static Compile;
using flashsolve.compiler;
using flashsolve.parser.ast;
using flashsolve.parser.invoker.antlrinvoker;
using flashsolve.util.datastructs;
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
                        var inv = new AntlrInvoker(); 
                        inv.add_file("Tests/test0.txt");
                        var compiler = new Sv2Z3Compiler();
                        var problem = compiler.Compile((SvConstraintProgram)inv.Ast[0]);
                        
                        var sv1 = new Sample(100000, problem);
                        sv1.Run();
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