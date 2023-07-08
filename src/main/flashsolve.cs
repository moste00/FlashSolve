namespace flashsolve.main;

using System;
using System.IO;
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
        FlashSampler,
        FlashTest
    }
    
    public static void Main(String[] args) {
        try {
            var subprogramArgs = args.Skip(1).ToArray();
            var cmdParser = new CmdArgsParser();

            var subprogramOption =
                new EnumArg<SubprogramType>()
                    { Name = "do" };
            subprogramOption
                .Map("parse".To(SubprogramType.FlashParser),
                    "sample".To(SubprogramType
                        .FlashSampler),
                    "compile".To(SubprogramType
                        .FlashCompiler),
                    "test".To(SubprogramType.FlashTest))
                .OnParse((sub) => {
                    switch (sub) {
                        case SubprogramType.FlashParser:
                            ParseMain(subprogramArgs);
                            break;
                        case SubprogramType.FlashSampler:
                            var inv = new AntlrInvoker();
                            inv.add_file("Tests/implication2.txt");
                            var compiler =
                                new Sv2Z3Compiler();
                            var problem =
                                compiler.Compile(
                                    (SvConstraintProgram)inv
                                        .Ast[0]);
                            var sv1 =
                                new Sample(50, problem, "Tests/implication2.txt");
                            sv1.Run();
                            break;
                        case SubprogramType.FlashCompiler:
                            CompileMain(subprogramArgs);
                            break;
                        case SubprogramType.FlashTest:
                            string folderPath = "Tests";
                            string[] fileNames =
                                Directory.GetFiles(
                                    folderPath);
                            
                            int[] sizes =
                                { 50, 100, 1000, 10000 };
                            foreach (string fileName in fileNames) {
                                var invoker = new AntlrInvoker();
                                invoker.add_file(fileName);
                                var svcompiler = new Sv2Z3Compiler();
                                var rproblem =
                                    svcompiler.Compile((SvConstraintProgram)invoker.Ast[0]);

                                foreach (int size in sizes) {
                                    Console.WriteLine(
                                        $"Testing: File {fileName} at size {size}");
                                    var filename = fileName
                                        .Replace("Tests/",
                                            "_"+ size +"_")
                                        .Replace("Tests\\",
                                            "_"+ size +"_");
                                    var sv = new Sample((uint)size, rproblem,filename);
                                    sv.Run();
                                }
                            }

                            break;
                    }
                })
                .OnError((str) => Console.WriteLine(
                    $"{str} is not a valid subprogram and flashsolve couldn't launch it.\n --------- \n Flashsolve can only launch"
                    + $"the following subprograms : {String.Join(",", subprogramOption.AllAcceptableValues())}"));

            cmdParser.Add(subprogramOption);

            cmdParser.Run(args);
        }
        catch (Exception e) {
            Console.WriteLine("Error : FlashSolve encountered an error and halted.");
            Console.WriteLine("Error Type : "+ e.GetType().Name);
            Console.WriteLine("Error Message : "+ e.Message);
        }
    }
}