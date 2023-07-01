using Antlr4.Runtime;
using flashsolve.parser.ast;

namespace flashsolve.parser.invoker.antlrinvoker;

class AntlrInvoker : ParserInvoker {
    private List<SvAstRoot> _asts;

    public SvAstRoot this[int n] => _asts[n];

    public SvAstRoot[] Ast => _asts.ToArray();

    public AntlrInvoker() {
        _asts = new List<SvAstRoot>();
    }
    public void add_file(string path) {
        var input = new AntlrFileStream(path);
        var lexer = new SystemVerilogLexer(input);
        var toks  = new CommonTokenStream(lexer);
        var parser = new SystemVerilogParser(toks) {
            BuildParseTree = true
        };
        var parseTree = parser.svprogram();
        var ast = new CstVisitor().Visit(parseTree);
        _asts.Add((SvAstRoot)ast);
    }

    public void add_string(string svcode) {
        var input = new AntlrInputStream(svcode);
        var lexer = new SystemVerilogLexer(input);
        var toks  = new CommonTokenStream(lexer);
        var parser = new SystemVerilogParser(toks) {
            BuildParseTree = true
        };
        var parseTree = parser.svprogram();
        var ast = new CstVisitor().Visit(parseTree);
        _asts.Add((SvAstRoot)ast);
    }
}