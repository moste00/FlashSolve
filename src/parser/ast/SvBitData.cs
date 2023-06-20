namespace flashsolve.parser.ast;

public class SvBitData {
    private CstRef cstnode;
    private uint startIndex;
    private uint endIndex;
    

    public SvBitData(CstRef cst, uint start, uint end) {
        cstnode = cst;
        startIndex = start;
        endIndex = end;
    }

    public string Name => cstnode.Name;
}