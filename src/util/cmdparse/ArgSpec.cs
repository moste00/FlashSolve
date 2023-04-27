namespace flashsolve.util.cmdparse; 

public abstract class ArgSpec {
    private string _name;
    private bool _isPositional;
    private bool _isOptional;
    private string _helpMsg;

    protected ArgSpec() {
        _name = "";
        _isPositional = true;
        _isOptional = false;
        _helpMsg = "";
    }

    public string Name {
        get => _name;
        set => _name = value;
    }
    public bool IsPositional {
        get => _isPositional;
        set => _isPositional = value;
    }
    public bool IsOptional {
        get => _isOptional;
        set => _isOptional = value;
    }
    public string HelpMsg {
        get => _helpMsg;
        set => _helpMsg = value;
    }
    
    public abstract object Parse(string value);
    public abstract string[] AllAcceptableValues();
}
public class ArgParseFailure : Exception {}