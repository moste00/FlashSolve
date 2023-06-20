using flashsolve.parser.ast;

namespace flashsolve.parser.invoker.antlrinvoker;

public static class MkAntlrCstRef {
    private record AntlrCst(string Name) : CstRef;

    public static CstRef FromClassDecl(SystemVerilogParser.ClassDeclContext decl) {
        return new AntlrCst(decl.ID()[0].GetText());
    }

    public static CstRef FromContraintPrototype(SystemVerilogParser.ConstraintPrototypeDeclContext proto) {
        return new AntlrCst(proto.ID().GetText());
    }

    public static CstRef FromConstraintDecl(SystemVerilogParser.ConstraintDeclBodyContext decl) {
        return new AntlrCst(decl.ID().GetText());
    }

    public static CstRef FromDataDecl(SystemVerilogParser.DataTypeClassDeclContext decl) {
        return new AntlrCst(decl.ID().GetText());
    }
}
