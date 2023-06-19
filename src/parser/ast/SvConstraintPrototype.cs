namespace flashsolve.parser.ast;

public class SvConstraintPrototype : SvAstNode
{
    private CstRef cstnode;
    private bool isExtern;
    private bool isPure;
    private bool isStatic;

    public SvConstraintPrototype(CstRef cstnode)
    {
        this.cstnode = cstnode;
        IsExtern = false;
        IsPure = false;
        IsStatic = false;
    }
    public SvConstraintPrototype(CstRef cstnode, bool isExtern = false , bool isPure = false)
    {
        this.cstnode = cstnode;
        IsExtern = isExtern;
        IsPure = isPure;
        IsStatic = false;
    }
    public SvConstraintPrototype(CstRef cstnode, bool isStatic = false)
    {
        this.cstnode = cstnode;
        IsExtern = false;
        IsPure = false;
        IsStatic = isStatic;
    }
    public SvConstraintPrototype(CstRef cstnode, bool isExtern = false, bool isPure = false, bool isStatic = false)
    {
        this.cstnode = cstnode;
        IsExtern = isExtern;
        IsPure = isPure;
        IsStatic = isStatic;
    }

    public bool IsExtern
    {
        get
        {
            return isExtern;
        }
        set
        {
            isExtern = value;
        }
    }

    public bool IsPure
    {
        get
        {
            return isPure;
        }
        set
        {
            isPure = value;
        }
    }
    public bool IsStatic
    {
        get
        {
            return isStatic;
        }
        set
        {
            isStatic = value;
        }
    }
}