namespace flashsolve.parser.ast;

public class SvUnaryExpression : SvExpr
{
    private SvPrimary operand;
    private UnaryOP unaryOP;

    public SvUnaryExpression(SvPrimary operand, UnaryOP unaryOP)
    {
        Operand = operand;
        OP = unaryOP;
    }
    public enum UnaryOP {
        Plus,
        Minus,
        Negation,
        Complement,
        BitwiseAnd,
        BitwiseNand,
        BitwiseOr,
        BitwiseNor,
        Xor,
        Xnor
    }

    public SvPrimary Operand
    {
        get
        {
           return operand ;
        }
        set
        {
            operand = value;
        }
    }

    public UnaryOP OP
    {
        get
        {
            return unaryOP;
        }
        set
        {
            unaryOP = value;
        }
    }
}