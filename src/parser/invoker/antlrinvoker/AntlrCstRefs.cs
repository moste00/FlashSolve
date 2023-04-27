using flashsolve.parser.ast;

namespace flashsolve.parser.invoker.antlrinvoker; 

public static class MkAntlrCstRef {
    public record AntlrCst(string Name) : CstRef;

        public static CstRef FromClassDecl(SystemVerilogParser.ClassDeclContext decl) {
            return new AntlrCst(decl.ID()[0].GetText());
        }
}