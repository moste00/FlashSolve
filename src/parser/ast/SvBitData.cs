namespace flashsolve.parser.ast;

public class SvBitData : SvAstNode {
    private CstRef cstnode;
    private uint startIndex;
    private uint endIndex;
    private Random rand;
    
    public SvBitData(uint start, uint end) {
        startIndex = start;
        endIndex = end;
    }
    public SvBitData(CstRef cst, uint start, uint end) {
        cstnode = cst;
        startIndex = start;
        endIndex = end;
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

    public Random Rand {
        get {
            return rand;
        }
        set {
            rand = value;
        }
    }
}