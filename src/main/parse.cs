using Antlr4.Runtime;
using FlashSolve.parser;

namespace FlashSolve.main;

static class Parse {   
    public static void PMain(String[] args){
        if (args.Length != 1) {
            PrintUsage();
            return;
        }
        var input = new AntlrFileStream(args[0]);
        var lexer = new SystemVerilogLexer(input);
        var toks  = new CommonTokenStream(lexer);
        var parser = new SystemVerilogParser(toks) {
            BuildParseTree = true
        };
        var parseTree = parser.svprogram();
        Console.WriteLine(new CstToAst().Visit(parseTree));
    }

    private static void PrintUsage() {
        Console.WriteLine("This is not how any of this works.");
    }
}