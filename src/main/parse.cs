using flashsolve.parser.invoker.antlrinvoker;

namespace flashsolve.main;

static class Parse {
    public static void ParseMain(String[] args) {
        var invoker = new AntlrInvoker();
        invoker.add_file(args[0]);
    }
}