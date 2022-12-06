using Antlr4.Runtime;

using FlashSolve.parser;

namespace FlashSolve {
    namespace main {
        class Parse {   
            static void Main(String[] args){
                Console.WriteLine(args[0]);

                var input = new AntlrFileStream(args[0]);
                var lexer = new SystemVerilogLexer(input);
                var toks  = new CommonTokenStream(lexer);
                var parser = new SystemVerilogParser(toks);
                
                var tree = parser.BuildParseTree;

                var visitor = new CSTVisitor();
            }
        }
    }
}