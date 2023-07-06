using flashsolve.parser.invoker.antlrinvoker;
using Microsoft.Z3;

namespace flashsolve.compiler; 

using parser.ast;

public class Sv2Z3Compiler {
    private Context? _z3Ctx;
    private RandProblem? _currProblem;
    private string? _currClsName;
    public Sv2Z3Compiler() {
        _z3Ctx = null;
        _currProblem = null;
        _currClsName = null;
    }
    public RandProblem Compile(SvConstraintProgram prog) {
        var result = new RandProblem();
        _currProblem = result;
        _z3Ctx = new Context();
        
        foreach (var cls in prog) {
            var problem = Compile(cls);
            result.Merge(problem);
        }
        result.Context = _z3Ctx;
        
        _currProblem = null;
        _z3Ctx = null;
        return result;
    }

    private RandProblem Compile(SvClass cls) {
        var result = new RandProblem();
        string oldClsName = _currClsName;
        RandProblem old = _currProblem;
        _currClsName = cls.Name;
        _currProblem = result;
        
        foreach (var dataDef in cls.Members) {
            string varName = $"{cls.Name}#{dataDef.Name}";
            result.AddVar(varName,
                        _z3Ctx.MkBVConst(varName,dataDef.End - dataDef.Start + 1));
        }   

        foreach (var svConstraint in cls.Constraints) {
            var (constraint, varNames) = Compile(svConstraint);
            result.AddConstraint(constraint);
            
            foreach (var varName in varNames) {
                result.AssociateConstraintWithVar(constraint,$"{cls.Name}#{varName}");
            }
        }

        _currClsName = oldClsName;
        _currProblem = old;
        return result;
    }

    private (BoolExpr,HashSet<string>) Compile(SvConstraint constraint) {
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
            (items.Length == 1)? items[0]:_z3Ctx.MkAnd(items), 
            constraintVarNames
        );
    }

    private (BoolExpr, HashSet<string>) Compile(SvConstraint.BlockItem item) {
        if (item is SvConstraint.Expr expr) {
            return Compile(expr);
        }

        UnrecognizedAstNode.Throw(item);
        //unreachable
        return (null,null);
    }

    private (BoolExpr, HashSet<string>) Compile(SvConstraint.Expr exp) {
        if (exp is SvExprOrDist exOrDist) {
            var (constraintExpr,varNames) = Compile(exOrDist);
            return (
                Types.AssertBoolTypeOrFail(constraintExpr),
                varNames
            );
        }
        //TODO add other classes that inherit from SvConstraint.Expr
        if (exp is SvUniqueness uniqueness)
        {
            var (constraintExpr,varNames) = Compile(uniqueness);
            return (
                Types.AssertBoolTypeOrFail(constraintExpr),
                varNames
            );
        }
        if (exp is SvImplication implication)
        {
            var (constraintExpr,varNames) = Compile(implication);
            return (
                Types.AssertBoolTypeOrFail(constraintExpr),
                varNames
            );
        }
        if (exp is SvIfElse ifElse)
        {
            var (constraintExpr,varNames) = Compile(ifElse);
            return (
                Types.AssertBoolTypeOrFail(constraintExpr),
                varNames
            );
        }
        
        UnrecognizedAstNode.Throw(exp);
        //unreachable
        return (null, null);
    }

    private (Z3Expr, HashSet<string>) Compile(SvExprOrDist exd) {
        if (exd is SvExpr e) {
            return Compile(e);
        }
        UnrecognizedAstNode.Throw(exd);
        //unreachable
        return (null, null);
    }
    private (Z3Expr, HashSet<string>) Compile(SvUniqueness uniquenessExpr)
    {
        var (operand, varNames) = Compile(uniquenessExpr.OpenRange);
        List<Z3Expr> operandsDistinct = new List<Z3Expr>();
        foreach (var entry in operand) {
            if (entry.Item2 != null) {
                throw new UnsupportedOperation("The range of 2 values is not supported in Uniqueness expressions");
            }
            operandsDistinct.Add(entry.Item1);
        }
        
        return (
            Z3Expr.From(_z3Ctx.MkDistinct(
                    Z3Expr.ToZ3(operandsDistinct)
            )),
            varNames
        );
    }
    private (Z3Expr, HashSet<string>) Compile(SvImplication implicationExpr)
    {
        var (antecedent, 
             antecedentVarNames) = Compile(implicationExpr.Expr);
        var (consequent, 
             consequentVarNames) = Compile(implicationExpr.ConstraintSet);
        
        antecedentVarNames.UnionWith(consequentVarNames);
        var varNames = antecedentVarNames;

        return (
            Z3Expr.From(_z3Ctx.MkImplies(
                Types.AssertBoolTypeOrFail(antecedent),
                Types.AssertBoolTypeOrFail(consequent)
            )),
            varNames
        );
        
    }

    private (Z3Expr, HashSet<string>) Compile(SvIfElse ifThenElseExpr)
    {
        var (condition, conditionVarNames) = Compile(ifThenElseExpr.Expr);
        var (thenExpr, thenVarNames) = Compile(ifThenElseExpr.Then);
        conditionVarNames.UnionWith(thenVarNames);
        
        Z3Expr? elseExpr = null;
        if (ifThenElseExpr.Else is not null) {
            (elseExpr,
             HashSet<string> elseVarNames) = Compile(ifThenElseExpr.Else);
             conditionVarNames.UnionWith(elseVarNames);
        }
        
        var varNames = conditionVarNames;

        var thenImplication = Z3Expr.From(_z3Ctx.MkImplies(
            Types.AssertBoolTypeOrFail(condition),
            Types.AssertBoolTypeOrFail(thenExpr)
        ));
        if (ifThenElseExpr.Else is null) {
            return (
                thenImplication,
                varNames
            );
        }
        
        var elseImplication = Z3Expr.From(_z3Ctx.MkImplies(
            _z3Ctx.MkNot(Types.AssertBoolTypeOrFail(condition)),
            Types.AssertBoolTypeOrFail(elseExpr!)
        ));
        return (
            Z3Expr.From(_z3Ctx.MkAnd(thenImplication,elseImplication)),
            varNames
        );
    }
    
    private (Z3Expr.Bool, HashSet<string>) Compile(SvConstraintSet constraintSet)
    {
        var constraintExprs = new List<BoolExpr>();
        var varNames = new HashSet<string>();

        foreach (var expr in constraintSet)
        {
            (BoolExpr constraintExpr, 
             HashSet<string> exprVarNames) = Compile(expr);
            constraintExprs.Add(constraintExpr);
            varNames.UnionWith(exprVarNames);
        }

        var constraintExprsArray = constraintExprs.ToArray();
        var combinedExpr = _z3Ctx.MkAnd(constraintExprsArray);
        return (
            Z3Expr.From(combinedExpr), 
            varNames
        );
    }
    
    private (Z3Expr, HashSet<string>) Compile(SvExpr ex) {
        if (ex is SvBinaryExpression be) {
            return Compile(be);
        }

        if (ex is SvUnaryExpression ue) {
            return Compile(ue);
        }

        if (ex is SvPrimary pe) {
            return Compile(pe);
        }

        if (ex is SvInsideExpression ie) {
            return Compile(ie);
        }


            UnrecognizedAstNode.Throw(ex);
        //unreachable
        return (null, null);
    }

    private (Z3Expr, HashSet<string>) Compile(SvBinaryExpression bin) {
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

    
    private (Z3Expr, HashSet<string>) Compile(SvUnaryExpression un) {
        var (operandExpr, operandVars) = Compile(un.Operand); // Compile the operand expression
        switch (un.OP) {
            case SvUnaryExpression.UnaryOP.Plus:
                return (operandExpr, operandVars);

            case SvUnaryExpression.UnaryOP.Negation:
                var operandExprBool = Types.AssertBoolTypeOrFail(operandExpr);
                var negatedExpr = _z3Ctx.MkNot(operandExprBool);
                return (Z3Expr.From(negatedExpr),
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.Complement:
                var complementExprBit = Types.AssertBitVecTypeOrFail(operandExpr);
                var complementExpr = _z3Ctx.MkBVNot(complementExprBit);
                return (Z3Expr.From(complementExpr),
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.BitwiseAnd:
                var bitwiseAndBit = Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseAndBit.Expr.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var bitwiseAndExpr = _z3Ctx.MkBVRedAND(bitwiseAndBit);
                var bitwiseAndExprBool = _z3Ctx.MkEq(bitwiseAndExpr, _z3Ctx.MkBV(1, bitwiseAndExpr.SortSize));
                return (Z3Expr.From(bitwiseAndExprBool), 
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.BitwiseNand:
                var bitwiseNandBit = Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseNandBit.Expr.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var bitwiseNandExpr = _z3Ctx.MkBVRedAND(bitwiseNandBit);
                var convertBitToBoolNand = _z3Ctx.MkEq(bitwiseNandExpr, _z3Ctx.MkBV(1, bitwiseNandExpr.SortSize));
                return (Z3Expr.From(
                        _z3Ctx.MkNot(convertBitToBoolNand)), 
                    operandVars
                );
            
            case SvUnaryExpression.UnaryOP.BitwiseOr:
                var bitwiseOrBit = Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseOrBit.Expr.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var bitwiseOrExpr = _z3Ctx.MkBVRedOR(bitwiseOrBit);
                var bitwiseOrExprBool = _z3Ctx.MkEq(bitwiseOrExpr, _z3Ctx.MkBV(1, bitwiseOrExpr.SortSize));
                return (Z3Expr.From(bitwiseOrExprBool), 
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.BitwiseNor:
                var bitwiseNorBit = Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseNorBit.Expr.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var bitwiseNorExpr = _z3Ctx.MkBVRedOR(bitwiseNorBit);
                var convertBitToBoolNor = _z3Ctx.MkEq(bitwiseNorExpr, _z3Ctx.MkBV(1, bitwiseNorExpr.SortSize));
                return (Z3Expr.From(
                        _z3Ctx.MkNot(convertBitToBoolNor)), 
                    operandVars
                );
            
            case SvUnaryExpression.UnaryOP.Xor:
                var bitwiseXorBit = Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseXorBit.Expr.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var firstBitXor = _z3Ctx.MkExtract(0,0,bitwiseXorBit);
                var firstInputXor = _z3Ctx.MkEq(firstBitXor, _z3Ctx.MkBV(1, firstBitXor.SortSize));
                BoolExpr finalResultXor = null;
                var sizeXor = bitwiseXorBit.Expr.SortSize;
                for (uint i = 1; i < sizeXor; i++) {
                    var nextBitXor = _z3Ctx.MkExtract(i,i,bitwiseXorBit);
                    var secondInputXor = _z3Ctx.MkEq(nextBitXor, _z3Ctx.MkBV(1, nextBitXor.SortSize));
                    finalResultXor = _z3Ctx.MkXor(firstInputXor, secondInputXor);
                    firstInputXor = finalResultXor;
                }
                return (Z3Expr.From(finalResultXor),
                        operandVars);
            
            case SvUnaryExpression.UnaryOP.Xnor:
                var bitwiseXnorBit = Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseXnorBit.Expr.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var firstBitXnor = _z3Ctx.MkExtract(0,0,bitwiseXnorBit);
                BitVecExpr finalResultXnor = null;
                var sizeXnor = bitwiseXnorBit.Expr.SortSize;
                for (uint i = 1; i < sizeXnor; i++) {
                    var nextBitXnor = _z3Ctx.MkExtract(i,i,bitwiseXnorBit);
                    finalResultXnor = _z3Ctx.MkBVXNOR(firstBitXnor, nextBitXnor);
                    firstBitXnor = finalResultXnor;
                }
                var finalResultBool = _z3Ctx.MkEq(finalResultXnor, _z3Ctx.MkBV(1, finalResultXnor.SortSize));
                return (Z3Expr.From(finalResultBool),
                    operandVars);
            
            default:
                UnrecognizedAstPropertyValue.Throw(un.OP);
                //unreachable
                return (null, null);
        }    
    }
    private (List<(Z3Expr.BitVec,Z3Expr.BitVec?)>, 
             HashSet<string>) 
             Compile(SvOpenRange openRange)
    {
        var rangeExprs = new List<(Z3Expr.BitVec,Z3Expr.BitVec?)>();
        var varNames = new HashSet<string>();

        foreach (var valueRange in openRange)
        {
            var (rangeExpr, rangeVarNames) = Compile(valueRange);
            rangeExprs.Add(rangeExpr);
            varNames.UnionWith(rangeVarNames);
        }
        
        return (rangeExprs,
                varNames
            );
    }
    private ((Z3Expr.BitVec, Z3Expr.BitVec?), HashSet<string>) Compile(SvValueRange valueRange)
    {
        var (rangeExpr1, varNames1) = Compile(valueRange.Item1);
        var rangeExpr1Bv = Types.AssertBitVecTypeOrFail(rangeExpr1);
        
        if (valueRange.Item2 != null) {
            var (rangeExpr2, varNames2) = Compile(valueRange.Item2);
            var rangeExpr2Bv = Types.AssertBitVecTypeOrFail(rangeExpr2);
            varNames1.UnionWith(varNames2);
            
            return (
                (rangeExpr1Bv, rangeExpr2Bv),
                varNames1
            );
        }
        
        return (
            (rangeExpr1Bv, null),
            varNames1
        );
    }

    private (Z3Expr, HashSet<string>) Compile(SvInsideExpression ie) {
        (Z3Expr leftExpr, 
         HashSet<string> exprVars) = Compile(ie.Expr);
        var (openRangeExprs, 
             openRangeVars) = Compile(ie.OpenRange);
        exprVars.UnionWith(openRangeVars);
        
        var leftExprBv = Types.AssertBitVecTypeOrFail(leftExpr);
        BoolExpr[] insideConstraints = new BoolExpr[openRangeExprs.Count];

        int i = 0;
        foreach (var openRange in openRangeExprs) {
            BoolExpr constraint;
            //compiles to an equal constraint
            if (openRange.Item2 == null) {
                if (leftExprBv.Expr.SortSize == openRange.Item1.Expr.SortSize) {
                    constraint = _z3Ctx.MkEq(
                        leftExprBv,
                        openRange.Item1
                    );
                }
                else {
                    var (leftExprBvSameSz,item1SameSz) = Types.MakeSameSizeByZeroExtension(
                        leftExprBv, openRange.Item1, _z3Ctx
                     );
                    constraint = _z3Ctx.MkEq(
                        leftExprBvSameSz,
                        item1SameSz
                    );
                }
            }
            //compiles to a conjunction of 2 range constraints (inequalities)
            else {
                if (leftExprBv.Expr.SortSize == openRange.Item1.Expr.SortSize && 
                    leftExprBv.Expr.SortSize == openRange.Item2.Expr.SortSize) {
                    constraint = _z3Ctx.MkAnd(
                        _z3Ctx.MkBVSLE(leftExprBv,openRange.Item2),
                        _z3Ctx.MkBVSGE(leftExprBv,openRange.Item1)
                    );
                }
                else {
                    var (leftExprBvSameSz,item1SameSz,item2SameSz) = Types.MakeSameSizeByZeroExtension(
                        leftExprBv, openRange.Item1, openRange.Item2, _z3Ctx
                    );
                    constraint = _z3Ctx.MkAnd(
                        _z3Ctx.MkBVSLE(leftExprBvSameSz,item2SameSz),
                        _z3Ctx.MkBVSGE(leftExprBvSameSz, item1SameSz)
                    );
                }
            }
            insideConstraints[i++] = constraint;
        }

        return (
            Z3Expr.From((insideConstraints.Length > 1)?
                         _z3Ctx.MkOr(insideConstraints): 
                         insideConstraints[0]),
            exprVars
        );
    }
    private (Z3Expr, HashSet<string>) Compile(SvPrimary prim) {
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

    private (Z3Expr, HashSet<string>) Compile(SvLiteral lit) {
        if (lit is SvNumLiteral num) {
            return Compile(num);
        }

        if (lit is SvStringLiteral) {
            throw new UnsupportedOperation("Compiler doesn't support compiling strings yet.");
        }

        UnrecognizedAstNode.Throw(lit);
        //unreachable
        return (null, null);
    }

    private (Z3Expr, HashSet<string>) Compile(SvHierarchicalId hid) {
        if (hid.HierarchicalIds.Count == 1) {
            return (
                Z3Expr.From(_currProblem.LookupVar(_currClsName +"#"+ hid.HierarchicalIds[0])),
                new HashSet<string>() {hid.HierarchicalIds[0]}
            );
        }

        UnrecognizedAstPropertyValue.Throw(hid.HierarchicalIds);
        //unreachable
        return (null, null);
    }

    private (Z3Expr, HashSet<string>) Compile(SvNumLiteral numLit) {
        return (
            Z3Expr.BitVec.FromSvNum(numLit.Number,_z3Ctx),
            new HashSet<string>()
        );
    }
}