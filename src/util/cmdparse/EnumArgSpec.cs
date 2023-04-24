namespace flashsolve.util.cmdparse; 

public class EnumArg<TE> : ArgSpec where TE : Enum {
    private TE? _defaultValue;
    private Action<TE>? _parseCallback;
    private Action<string>? _errorCallback;
    
    public EnumArg() {
        _defaultValue = default(TE);
        _parseCallback = null;
        _errorCallback = null;
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

    public override object Parse(string value) {
        try {
            TE result = (TE)Enum.Parse(typeof(TE), value);
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
}