namespace flashsolve.parser.ast;

public class SvBitData : SvAstNode {
    private CstRef cstnode;
    private uint startIndex;
    private uint endIndex;
    private Random rand;
    
    public SvBitData(uint start, uint end) {
        if (start < end) {
            startIndex = start;
            endIndex = end;
        }
        else {
            startIndex = end;
            endIndex = start;
        }
    }
    public SvBitData(CstRef cst, uint start, uint end) {
        cstnode = cst;
        if (start < end) {
            startIndex = start;
            endIndex = end;
        }
        else {
            startIndex = end;
            endIndex = start;
        }
    }

    public enum Random {
        rand,
        randc,
        notRand
    }

    public CstRef CstNode {
        get {
            return cstnode;
        }
        set {
            cstnode = value;
        }
    }
    public string Name => cstnode.Name;
    public uint Start => startIndex;
    public uint End => endIndex;
    public Random Rand {
        get {
            return rand;
        }
        set {
            rand = value;
        }
    }
}