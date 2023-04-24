using Antlr4.Runtime;

namespace flashsolve.main;

static class Parse {   
    public static void ParseMain(String[] args){
        if (args.Length != 1) {
            PrintUsage();
            return;
        }
    }

    private static void PrintUsage() {
        Console.WriteLine("This is not how any of this works.");
    }
}