using Microsoft.Z3;

namespace flashsolve.compiler; 

using parser.ast;

public class Sv2Z3Compiler {
    private Context _z3Ctx;
    private RandProblem _currProblem;
    public Sv2Z3Compiler() {
        _z3Ctx = new Context();
        _currProblem = null;
    }
    public RandProblem Compile(SvConstraintProgram prog) {
        var result = new RandProblem();
        _currProblem = result;
        
        foreach (var cls in prog) {
            var problem = Compile(cls);
            result.Merge(problem);
        }

        _currProblem = null;
        return result;
    }

    public RandProblem Compile(SvClass cls) {
        var result = new RandProblem();
        RandProblem old = _currProblem;
        _currProblem = result;
        
        foreach (var dataDef in cls.Members) {
            string varName = $"${cls.Name}#${dataDef.Name}";
            result.AddVar(varName,
                        _z3Ctx.MkBVConst(varName,dataDef.End - dataDef.Start + 1));
        }   

        foreach (var svConstraint in cls.Constraints) {
            var (constraint, varNames) = Compile(svConstraint);
            result.AddConstraint(constraint);
            
            foreach (var varName in varNames) {
                result.AssociateConstraintWithVar(constraint,varName);
            }
        }

        _currProblem = old;
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
            _z3Ctx.MkAnd(items), 
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
            var (constraintExpr,varNames) = Compile(exOrDist);
            return (
                Types.AssertBoolTypeOrFail(constraintExpr),
                varNames
            );
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
                        Z3Expr.From(_z3Ctx.MkBVAdd(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVAdd(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Minus: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVSub(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVSub(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Mul: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVMul(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVMul(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Div: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVSDiv(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVSDiv(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Mod: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVSMod(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVSMod(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Exp:
                throw new UnsupportedOperation("Compiler doesn't currently support compiling exponentiation to Z3 expressions");
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.LogicalShiftRight: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVLSHR(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVLSHR(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.LogicalShiftLeft: 
                throw new NotImplementedException("Logical Shift Left");
            case SvBinaryExpression.Op.ArithmeticShiftRight: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVASHR(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVASHR(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.ArithmeticShiftLeft:
                throw new NotImplementedException("Arithmetic Shift Left");
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.Less: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVSLT(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVSLT(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Greater: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVSGT(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVSGT(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.LessEqual: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVSLE(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVSLE(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.GreaterEqual: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVSGE(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVSGE(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Equal: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkEq(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkEq(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.NotEqual: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkNot(_z3Ctx.MkEq(l, r))),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkNot(_z3Ctx.MkEq(sameSizeL, sameSizeR))),
                    varNames
                );
            }
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.EqualXZ:
                throw new UnsupportedOperation("Compiler doesn't currently support compiling constraints involving non-binary values (X and Z).");
            case SvBinaryExpression.Op.NotEqualXZ:
                throw new UnsupportedOperation("Compiler doesn't currently support compiling constraints involving non-binary values (X and Z).");
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.BitwiseAnd: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVAND(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVAND(sameSizeL, sameSizeR)),
                    varNames
                );
            } 
            case SvBinaryExpression.Op.BitwiseOr: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVOR(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVOR(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.BitwiseXor: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVXOR(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVXOR(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.BitwiseXnor: {
                var (l, r) = Types.AssertBitVecTypeOrFail(left, right);
                if (l.Expr.SortSize == r.Expr.SortSize) {
                    return (
                        Z3Expr.From(_z3Ctx.MkBVXNOR(l, r)),
                        varNames
                    );
                }

                var (sameSizeL, sameSizeR) = Types.MakeSameSizeByZeroExtension(l, r,_z3Ctx);
                return (
                    Z3Expr.From(_z3Ctx.MkBVXNOR(sameSizeL, sameSizeR)),
                    varNames
                );
            }
            //----------------------------------------------------------------------------------------------------------
            case SvBinaryExpression.Op.And: {
                var (l, r) = Types.AssertBoolTypeOrFail(left, right);
           
                return (
                    Z3Expr.From(_z3Ctx.MkAnd(l, r)),
                    varNames
                );
            }
            case SvBinaryExpression.Op.Or: {
                var (l, r) = Types.AssertBoolTypeOrFail(left, right);

                return (
                    Z3Expr.From(_z3Ctx.MkOr(l, r)),
                    varNames
                );
            }
            default:
                UnrecognizedAstPropertyValue.Throw(bin.Operator);
                //unreachable
                return (null, null);
        }
    }

    
    public (Z3Expr, HashSet<string>) Compile(SvUnaryExpression un) {
        switch (un.OP) {
            case SvUnaryExpression.UnaryOP.Plus:
                throw new NotImplementedException();
            case SvUnaryExpression.UnaryOP.Minus:
                throw new NotImplementedException();
            case SvUnaryExpression.UnaryOP.Negation:
                throw new NotImplementedException();
            case SvUnaryExpression.UnaryOP.Complement:
                throw new NotImplementedException();
            case SvUnaryExpression.UnaryOP.BitwiseAnd:
                throw new NotImplementedException();
            case SvUnaryExpression.UnaryOP.BitwiseNand:
                throw new NotImplementedException();
            case SvUnaryExpression.UnaryOP.BitwiseOr:
                throw new NotImplementedException();
            case SvUnaryExpression.UnaryOP.BitwiseNor:
                throw new NotImplementedException();
            case SvUnaryExpression.UnaryOP.Xor:
                throw new NotImplementedException();
            case SvUnaryExpression.UnaryOP.Xnor:
                throw new NotImplementedException();
            default:
                UnrecognizedAstPropertyValue.Throw(un.OP);
                //unreachable
                return (null, null);
        }    
    }
    public (Z3Expr, HashSet<string>) Compile(SvPrimary prim) {
        if (prim is SvLiteral lit) {
            return Compile(lit);
        }

        if (prim is SvHierarchicalId hid) {
            return Compile(hid);
        }

        UnrecognizedAstNode.Throw(prim);
        //unreachable
        return (null, null);
    }

    public (Z3Expr, HashSet<string>) Compile(SvLiteral lit) {
        if (lit is SvNumLiteral num) {
            return Compile(num);
        }

        if (lit is SvStringLiteral str) {
            throw new UnsupportedOperation("Compiler doesn't support compiling strings yet.");
        }

        UnrecognizedAstNode.Throw(lit);
        //unreachable
        return (null, null);
    }

    public (Z3Expr, HashSet<string>) Compile(SvHierarchicalId hid) {
        if (hid.HierarchicalIds.Count == 1) {
            return (
                Z3Expr.From(_currProblem.LookupVar(hid.HierarchicalIds[0])),
                new HashSet<string>() {hid.HierarchicalIds[0]}
            );
        }

        UnrecognizedAstPropertyValue.Throw(hid.HierarchicalIds);
        //unreachable
        return (null, null);
    }

    public (Z3Expr, HashSet<string>) Compile(SvNumLiteral numLit) {
        return (
            Z3Expr.BitVec.FromSvNum(numLit.Number,_z3Ctx),
            new HashSet<string>()
        );
    }
}