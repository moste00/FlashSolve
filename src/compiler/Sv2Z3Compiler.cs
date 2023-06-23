using Microsoft.Z3;

namespace flashsolve.compiler; 

using parser.ast;

public class Z3Expr {
    public class Bool : Z3Expr {
        private BoolExpr _e;
        public Bool(BoolExpr e) {
            this._e = e;
        }
        public BoolExpr Expr => _e;
        public static implicit operator BoolExpr(Bool b) => b._e;
    }
    public class BitVec : Z3Expr {
        private BitVecExpr _e;
        public BitVec(BitVecExpr e) {
            this._e = e;
        }
        public BitVecExpr Expr => _e;
        public static implicit operator BitVecExpr(BitVec bv) => bv._e;
    }

    public static Z3Expr From(BoolExpr e) => new Bool(e);
    public static Z3Expr From(BitVecExpr e) => new BitVec(e);
}

public class Sv2Z3Compiler {
    private Context z3ctx;

    public Sv2Z3Compiler() {
        z3ctx = new Context();
    }
    public RandProblem Compile(SvConstraintProgram prog) {
        var result = new RandProblem();
        
        foreach (var cls in prog) {
            var problem = Compile(cls);
            result.Merge(problem);
        }

        return result;
    }

    public RandProblem Compile(SvClass cls) {
        var result = new RandProblem();
        foreach (var dataDef in cls.Members) {
            string varName = $"${cls.Name}#${dataDef.Name}";
            result.AddVar(varName);
        }

        foreach (var svConstraint in cls.Constraints) {
            var (constraint, varNames) = Compile(svConstraint);
            result.AddConstraint(constraint);
            
            foreach (var varName in varNames) {
                result.AssociateConstraintWithVar(constraint,varName);
            }
        }

        return result;
    }

    public (BoolExpr,HashSet<string>) Compile(SvConstraint constraint) {
        BoolExpr[] items = new BoolExpr[constraint.Items.items.Count];
        HashSet<string> constraintVarNames = new();

        int i = 0;
        foreach (var blockItem in constraint.Items.items) {
            var (item, itemVarNames) = Compile(blockItem);

            items[i] = item;
            constraintVarNames.UnionWith(itemVarNames);
            i++;
        }

        return (
            z3ctx.MkAnd(items), 
            constraintVarNames
        );
    }

    public (BoolExpr, HashSet<string>) Compile(SvConstraint.BlockItem item) {
        if (item is SvConstraint.Expr expr) {
            return Compile(expr);
        }

        UnrecognizedAstNode.Throw(item);
        //unreachable
        return (null,null);
    }

    public (BoolExpr, HashSet<string>) Compile(SvConstraint.Expr exp) {
        if (exp is SvExprOrDist exOrDist) {
            return Compile(exOrDist);
        }
        UnrecognizedAstNode.Throw(exp);
        //unreachable
        return (null, null);
    }

    public (Z3Expr, HashSet<string>) Compile(SvExprOrDist exd) {
        if (exd is SvExpr e) {
            return Compile(e);
        }
        UnrecognizedAstNode.Throw(exd);
        //unreachable
        return (null, null);
    }

    public (Z3Expr, HashSet<string>) Compile(SvExpr ex) {
        if (ex is SvBinaryExpression be) {
            return Compile(be);
        }

        if (ex is SvUnary ue) {
            return Compile(ue);
        }

        if (ex is SvPrimary pe) {
            return Compile(pe);
        }
        
        UnrecognizedAstNode.Throw(ex);
        //unreachable
        return (null, null);
    }

    public (Z3Expr, HashSet<string>) Compile(SvBinaryExpression bin) {
        var (left, leftVarNames) = Compile(bin.Left);
        var (right, rightVarNames) = Compile(bin.Right);

        leftVarNames.UnionWith(rightVarNames);
        var varNames = leftVarNames;
        switch (bin.Operator) {
            case SvBinaryExpression.Op.Plus: {
                var (l, r) = AssertBitVecTypeOrFail(left, right);
                return (
                    Z3Expr.From(z3ctx.MkBVAdd(l, r)),
                    varNames
                );
                break;
            }
            case SvBinaryExpression.Op.Minus: {
                var (l, r) = AssertBitVecTypeOrFail(left, right);
                return (
                    Z3Expr.From(z3ctx.MkBVSub(l, r)),
                    varNames
                );
                break;
            }
            case SvBinaryExpression.Op.Mul: {
                var (l, r) = AssertBitVecTypeOrFail(left, right);
                return (
                    Z3Expr.From(z3ctx.MkBVMul(l, r)),
                    varNames
                );
                break;
            }
            case SvBinaryExpression.Op.Div: {
                var (l, r) = AssertBitVecTypeOrFail(left, right);
                return (
                    Z3Expr.From(z3ctx.MkBVSDiv(l, r)),
                    varNames
                );
                break;
            }
            case SvBinaryExpression.Op.Mod: {
                var (l, r) = AssertBitVecTypeOrFail(left, right);
                return (
                    Z3Expr.From(z3ctx.MkBVSMod(l, r)),
                    varNames
                );
                break;
            }
            case SvBinaryExpression.Op.Exp:
                throw new UnsupportedOperation(
                    "Compiler doesn't currently support compiling to exponentiation to Z3 expressions");
            case SvBinaryExpression.Op.LogicalShiftRight:
                break;
            case SvBinaryExpression.Op.LogicalShiftLeft:
                break;
            case SvBinaryExpression.Op.ArithmeticShiftRight:
                break;
            case SvBinaryExpression.Op.ArithmeticShiftLeft:
                break;
            case SvBinaryExpression.Op.Less:
                break;
            case SvBinaryExpression.Op.Greater:
                break;
            case SvBinaryExpression.Op.LessEqual:
                break;
            case SvBinaryExpression.Op.GreaterEqual:
                break;
            case SvBinaryExpression.Op.Equal:
                break;
            case SvBinaryExpression.Op.NotEqual:
                break;
            case SvBinaryExpression.Op.EqualXZ:
                break;
            case SvBinaryExpression.Op.NotEqualXZ:
                break;
            case SvBinaryExpression.Op.BitwiseAnd:
                break;
            case SvBinaryExpression.Op.BitwiseOr:
                break;
            case SvBinaryExpression.Op.BitwiseXor:
                break;
            case SvBinaryExpression.Op.BitwiseXnor:
                break;
            case SvBinaryExpression.Op.And:
                break;
            case SvBinaryExpression.Op.Or:
                break;
            default:
                throw new UnrecognizedAstPropertyValue(bin.Operator);
        }
    }

    

    public (Z3Expr, HashSet<string>) Compile(SvUnary un) {
        return (null, null);
    }
    public (Z3Expr, HashSet<string>) Compile(SvPrimary prim) {
        return (null, null);
    }
    
    private (Z3Expr.BitVec, Z3Expr.BitVec) AssertBitVecTypeOrFail(Z3Expr left, Z3Expr right) {
        if (left is Z3Expr.BitVec l) {
            if (right is Z3Expr.BitVec r) {
                return (l, r);
            }
        }

        throw new TypeMismatch($"Expected the 2 objects ${left} and ${right} to have BitVec type, found that they are not.");
    }
}