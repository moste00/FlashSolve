using Antlr4.Runtime;
using flashsolve.parser.ast;

namespace flashsolve.parser.invoker.antlrinvoker;

class AntlrInvoker : ParserInvoker {
    private SvAstRoot _ast;

    public SvAstRoot Ast => _ast;

    public void add_file(string path) {
        var input = new AntlrFileStream(path);
        var lexer = new SystemVerilogLexer(input);
        var toks  = new CommonTokenStream(lexer);
        var parser = new SystemVerilogParser(toks) {
            BuildParseTree = true
        };
        var parseTree = parser.svprogram();
        var ast = new CstVisitor().Visit(parseTree);
    }

    public void add_string(string svcode) {
        throw new NotImplementedException();
    }
}