namespace flashsolve.util.cmdparse; 

public class CmdArgsParser {
    private List<ArgSpec> _argsSpecs;
    
    public record ParseResult(Dictionary<string, object> Named, List<object> Positional) { }
    
    public CmdArgsParser() {
        _argsSpecs = new List<ArgSpec>();
    }
    public CmdArgsParser Add(ArgSpec argSpec) {
        _argsSpecs.Add(argSpec);
        return this;
    }

    public class NamedArgNoValueProvided : Exception {}
    public class ArgFailedToParse : Exception {}
    public class NotEnoughArgValues : Exception {}
        
    public ParseResult? Run(string[] args) {
        Dictionary<string, string> providedNamedArgs =  new Dictionary<string, string>();
        List<string> providedPositionalArgs = new List<string>();

        for (int i = 0; i < args.Length; i++) {
            if (args[i].StartsWith("--")) {
                if (i == args.Length - 1) {
                    throw new NamedArgNoValueProvided();
                }
                providedNamedArgs[args[i].Substring(2)] = args[i + 1];
            }
            else {
                providedPositionalArgs.Add(args[i]);
            }
        }

        var named = new Dictionary<string, object>();
        var positional = new List<object>();
        var successfulParsers = new HashSet<ArgSpec>();
        
        foreach (var argSpec in _argsSpecs) {
            if (argSpec.Name != "" && providedNamedArgs.ContainsKey(argSpec.Name)) {
                try {
                    string val = providedNamedArgs[argSpec.Name];
                    named[argSpec.Name] = argSpec.Parse(val);
                    successfulParsers.Add(argSpec);
                }
                catch(ArgParseFailure) {
                    throw new ArgFailedToParse();
                }
            }
        }

        int currentPosArgValue = 0;
        foreach (var argSpec in _argsSpecs) {

            if (!successfulParsers.Contains(argSpec)) {
                try {
                    string val = providedPositionalArgs[currentPosArgValue];
                    positional.Add(argSpec.Parse(val));
                    
                    successfulParsers.Add(argSpec);
                    currentPosArgValue++;
                }
                catch (ArgParseFailure) {
                    throw new ArgFailedToParse();
                }
            }
        }

        foreach (var argSpec in _argsSpecs) {
            if (!successfulParsers.Contains(argSpec) && !argSpec.IsOptional) {
                throw new NotEnoughArgValues();
            }
        }

        return new ParseResult(named, positional);
    }
}