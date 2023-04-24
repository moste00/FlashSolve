namespace FlashSolve.main;

public static class FlashSolve {
    public static void Main(String[] args) {
        if (args.Length == 0) {
            PrintUsage();
            return;
        }
        var subcommand = args[0].ToLower();
        var subcommandArgs = args.Skip(1).ToArray(); //subcommand doesn't need to redundantly know its own name 
        switch (subcommand) {
            case "parse": {
                Parse.PMain(subcommandArgs);
                break;
            }
            case "sample": {
                Sample.main(subcommandArgs);
                break;
            }
            default: {
                Console.WriteLine($"I don't know how to {subcommand}, Sorry.");
                break;
            }
        }
    }

    private static void PrintUsage() {
        Console.WriteLine("Please pass the name of the subprogram to call. Allowed options are 'parse' and 'sample'");
    }
}