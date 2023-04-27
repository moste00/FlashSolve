using System.Runtime.Serialization;

namespace flashsolve.util.cmdparse; 

public static class StrExtensions {
    public static ValueTuple<String, TE> To<TE>(this string s,TE value) {
        return (s, value);
    }
}

public class EnumArg<TE> : ArgSpec where TE : Enum {
    
    private TE? _defaultValue;
    private Action<TE>? _parseCallback;
    private Action<string>? _errorCallback;
    private Dictionary<string, TE>? _strMapping;
    
    
    public EnumArg() {
        _defaultValue = default(TE);
        _parseCallback = null;
        _errorCallback = null;
        _strMapping = null;
    }

    public EnumArg<TE> Default(TE? defaultValue) {
        _defaultValue = defaultValue;
        return this;
    }
    public EnumArg<TE> OnError(Action<string> errCallback) {
        _errorCallback = errCallback;
        return this;
    }
    public EnumArg<TE> OnParse(Action<TE> parseCallback) {
        _parseCallback = parseCallback;
        return this;
    }

    public EnumArg<TE> Map(params ValueTuple<string, TE>[] mappings) {
        _strMapping ??= new Dictionary<string, TE>();
        foreach (var mapping in mappings) {
            _strMapping[mapping.Item1] = mapping.Item2;
        }

        return this;
    }

    public override object Parse(string value) {
        try {
            TE result = (_strMapping is not null && _strMapping.ContainsKey(value))? 
                        _strMapping[value]: 
                        (TE)Enum.Parse(typeof(TE), value);
            
            if (_parseCallback is not null) {
                _parseCallback(result);
            }
            return result;
        }
        catch (ArgumentException) {
            if (_errorCallback is not null) {
                _errorCallback(value);
            }
            throw new ArgParseFailure();
        }
    }

    public override string[] AllAcceptableValues() {
        var enumNames = Enum.GetNames(typeof(TE));
        
        if (_strMapping is not null) {
            List<string> acceptable = new List<string>();
            var addedEnums = new HashSet<TE>();
            
            foreach (var mapping in _strMapping) {
                acceptable.Add(mapping.Key);
                addedEnums.Add(mapping.Value);
            }

            foreach (var enumValue in Enum.GetValues(typeof(TE))) {
                if (!addedEnums.Contains((TE)enumValue)) {
                    string enumName = Enum.GetName(typeof(TE),(TE)enumValue)!;
                    acceptable.Add(enumName);
                }
            }
            return acceptable.ToArray();
        }
        else {
            return enumNames;
        }
    }
}