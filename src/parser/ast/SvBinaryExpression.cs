namespace flashsolve.parser.ast;

public class SvBinaryExpression : SvExpr
{
    private SvExpr _left;
    private SvExpr _right;
    private Op _op;

    public SvBinaryExpression(SvExpr l, SvExpr r, Op o) {
        _left = l;
        _right = r;
        _op = o;
    }
    public enum Op {
        Plus,
        Minus,
        Mul,
        Div,
        Exp
    }

    public SvExpr Right => _right;
    public SvExpr Left => _left;
    public Op Operator => _op;
}

    