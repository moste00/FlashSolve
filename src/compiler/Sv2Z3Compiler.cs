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

    public static Bool From(BoolExpr e) => new(e);
    public static BitVec From(BitVecExpr e) => new(e);
}

public class Sv2Z3Compiler {
    private Context z3ctx;
    private uint uniqueId;
    public Sv2Z3Compiler() {
        z3ctx = new Context();
        uniqueId = 0;
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
        //TODO add other classes that inherit from SvConstraint.Expr
        
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

        if (ex is SvUnaryExpression ue) {
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
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(z3ctx.MkBVAdd(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,z3ctx);
                return (
                    Z3Expr.From(z3ctx.MkBVAdd(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Minus: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(z3ctx.MkBVSub(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,z3ctx);
                return (
                    Z3Expr.From(z3ctx.MkBVSub(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Mul: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(z3ctx.MkBVMul(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,z3ctx);
                return (
                    Z3Expr.From(z3ctx.MkBVMul(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Div: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(z3ctx.MkBVSDiv(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,z3ctx);
                return (
                    Z3Expr.From(z3ctx.MkBVSDiv(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Mod: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(z3ctx.MkBVSMod(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,z3ctx);
                return (
                    Z3Expr.From(z3ctx.MkBVSMod(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Exp:
                throw new UnsupportedOperation("Compiler doesn't currently support compiling exponentiation to Z3 expressions");
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.LogicalShiftRight:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.LogicalShiftLeft:
                throw new NotImplementedException("Logical Shift Left");
            case SvBinaryExpression.Op.ArithmeticShiftRight:
                throw new NotImplementedException("Arithmetic Shift Right");
            case SvBinaryExpression.Op.ArithmeticShiftLeft:
                throw new NotImplementedException("Arithmetic Shift Left");
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.Less:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.Greater:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.LessEqual:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.GreaterEqual:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.Equal:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.NotEqual:
                throw new NotImplementedException("Logical Shift Right");
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.EqualXZ:
                throw new UnsupportedOperation("Compiler doesn't currently support compiling constraints involving non-binary values (X and Z).");
            case SvBinaryExpression.Op.NotEqualXZ:
                throw new UnsupportedOperation("Compiler doesn't currently support compiling constraints involving non-binary values (X and Z).");
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.BitwiseAnd:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.BitwiseOr:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.BitwiseXor:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.BitwiseXnor:
                throw new NotImplementedException("Logical Shift Right");
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.And:
                throw new NotImplementedException("Logical Shift Right");
            case SvBinaryExpression.Op.Or:
                throw new NotImplementedException("Logical Shift Right");
            default:
                UnrecognizedAstPropertyValue.Throw(bin.Operator);
                //unreachable
                return (null, null);
        }
    }

    
    public (Z3Expr, HashSet<string>) Compile(SvUnaryExpression un) {
        return (null, null);
    }
    public (Z3Expr, HashSet<string>) Compile(SvPrimary prim) {
        return (null, null);
    }
}