using flashsolve.compiler;
using flashsolve.parser.ast;

namespace flashsolve.main; 

using flashsolve.parser.invoker.antlrinvoker;

public class Compile {
    public static void CompileMain(string[] args) {
        var invoker = new AntlrInvoker();
        invoker.add_file(args[0]);

        var ast = invoker.Ast[0];
        var compiler = new Sv2Z3Compiler();
        var problem = compiler.Compile((SvConstraintProgram)ast);
        
        Console.WriteLine("s");
    }
}