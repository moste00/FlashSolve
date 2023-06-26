using flashsolve.parser.ast;

namespace flashsolve.parser.invoker; 

public interface ParserInvoker {
    SvAstRoot[] Ast { get; }

    void add_file(string path);
    void add_string(string svcode);
}