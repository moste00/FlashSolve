namespace flashsolve.util.datastructs;

public class AppendOnlyMaxValueDictionary<TK, TV>
    where TV : IComparable
    where TK : notnull {

    protected Dictionary<TK, TV> _backing;
    protected TK _kmax;
    protected TV _max;

    public AppendOnlyMaxValueDictionary(TK dummy, TV init) {
        _backing = new Dictionary<TK, TV>();
        _kmax = dummy;
        _max = init;
    }

    public void Set(TK key, TV value) {
        _backing[key] = value;
        //if max < value
        if (_max.CompareTo(value) < 0) {
            _kmax = key;
            _max = value;
        }
    }

    public TV Get(TK key) {
        return _backing[key];
    }

    public (TK, TV) GetMax() {
        return (_kmax, _max);
    }

    public bool Contains(TK key) {
        return _backing.ContainsKey(key);
    }

    public TV this[TK key] {
        get => Get(key);
        set => Set(key, value);
    }

    public KeyValuePair<TK,TV>[] ToArray() {
        return _backing.ToArray();
    }

    public int Count => _backing.Count;
}

public class StringToIntDict : AppendOnlyMaxValueDictionary<string, int> {
    public StringToIntDict() : base("", -1) {
    }
    
    //specialized implementation to avoid boxing of integers in the call to .Compare
    public new void Set(string key, int value) {
        _backing[key] = value;
        //if max < value
        if (_max < value) {
            _kmax = key;
            _max = value;
        }
    }
    public new int this[string key] {
        get => Get(key);
        set => Set(key, value);
    }
    public void Incr(string varName) {
        if (Contains(varName)) {
            Set(varName,Get(varName)+1);
        } else {
            Set(varName, 0);
        }
    }
}