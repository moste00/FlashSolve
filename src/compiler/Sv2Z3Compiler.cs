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

    public RandProblem Compile(SvClass cls) {
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

    public (Z3Expr, HashSet<string>) Compile(SvExprOrDist exd) {
        if (exd is SvExpr e) {
            return Compile(e);
        }
        UnrecognizedAstNode.Throw(exd);
        //unreachable
        return (null, null);
    }
    public (Z3Expr, HashSet<string>) Compile(SvUniqueness uniquenessExpr)
    {
        var (operand, varNames) = Compile(uniquenessExpr.OpenRange);
        return (
            Z3Expr.From(_z3Ctx.MkDistinct(
                    operand
                    )),
            varNames
        );
        // Z3Expr.From(_z3Ctx.MkDistinct(operandVarNames.Select(varName => _currProblem.GetVar(varName))))
    }

    public (Z3Expr, HashSet<string>) Compile(SvImplication implicationExpr)
    {
        var (antecedent, antecedentVarNames) = Compile(implicationExpr.Expr);
        var (consequent, consequentVarNames) = Compile(implicationExpr.ConstraintSet);
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

    public (Z3Expr, HashSet<string>) Compile(SvIfElse ifThenElseExpr)
    {
        var (condition, conditionVarNames) = Compile(ifThenElseExpr.Expr);
        var (thenExpr, thenVarNames) = Compile(ifThenElseExpr.Then);
        Z3Expr? elseExpr = null;
        HashSet<string>? elseVarNames = null;
        conditionVarNames.UnionWith(thenVarNames);
        if (ifThenElseExpr.Else is not null) {
            (elseExpr,elseVarNames) = Compile(ifThenElseExpr.Else);
            conditionVarNames.UnionWith(elseVarNames);
        }
        
        var varNames = conditionVarNames;
        if (ifThenElseExpr.Else is null) {
            return (
                Z3Expr.From(_z3Ctx.MkImplies(
                    Types.AssertBoolTypeOrFail(condition),
                    Types.AssertBoolTypeOrFail(thenExpr)
                )),
                varNames
            );
        }
        return (
            Z3Expr.From((BoolExpr)_z3Ctx.MkITE(
                Types.AssertBoolTypeOrFail(condition),
                Types.AssertBoolTypeOrFail(thenExpr),
                Types.AssertBoolTypeOrFail(elseExpr)
            )),
            varNames
        );
    }
    
    public (Z3Expr, HashSet<string>) Compile(SvConstraintSet constraintSet)
    {
        var constraintExprs = new List<BoolExpr>();
        var varNames = new HashSet<string>();

        foreach (var expr in constraintSet)
        {
            var (constraintExpr, exprVarNames) = Compile(expr);
            constraintExprs.Add(constraintExpr);
            varNames.UnionWith(exprVarNames);
        }

        var constraintExprsArray = constraintExprs.ToArray();
        var combinedExpr = _z3Ctx.MkAnd(constraintExprsArray);
        return (Z3Expr.From(combinedExpr)
            , varNames
            );
    }
    
    public (List<Expr>, HashSet<string>) Compile(SvOpenRange openRange)
    {
        var rangeExprs = new List<Expr>();
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
    public (Expr, HashSet<string>) Compile(SvValueRange valueRange)
    {
        // var varNames = new Tuple<SvExpr, SvExpr>();
        // var varNames = new HashSet<string>();

        if (valueRange.Item2 is not null) {
            throw new UnsupportedOperation("The range of 2 values is not supported in Uniqueness expressions");
            return (null, null);
        }
        var (rangeExpr, varNames) = Compile(valueRange.Item1);
        return (Types.AssertBitVecTypeOrFail(rangeExpr),
                varNames
            );
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
        var (operandExpr, operandVars) = Compile(un.Operand); // Compile the operand expression
        switch (un.OP) {
            case SvUnaryExpression.UnaryOP.Plus:
                return (operandExpr, operandVars);
            
            case SvUnaryExpression.UnaryOP.Minus:
                var operandExprArith = Types.AssertBitVecTypeOrFail(operandExpr);
                var minusExpr = _z3Ctx.MkUnaryMinus((ArithExpr)operandExprArith);
                var minusExpr2 = (Expr) minusExpr;
                return (Z3Expr.From((BitVecExpr)minusExpr2),
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.Negation:
                var operandExprBool = Types.AssertBoolTypeOrFail(operandExpr);
                var negatedExpr = _z3Ctx.MkNot(operandExprBool);
                return (Z3Expr.From(negatedExpr),
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.Complement:
                var complementExprBit = Types.AssertBitVecTypeOrFail(operandExpr);
                var complementExpr = _z3Ctx.MkBVNot((BitVecExpr)complementExprBit);
                return (Z3Expr.From(complementExpr),
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.BitwiseAnd:
                var bitwiseAndBit = (BitVecExpr)Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseAndBit.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var bitwiseAndExpr = _z3Ctx.MkBVRedAND(bitwiseAndBit);
                return (Z3Expr.From(bitwiseAndExpr), 
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.BitwiseNand:
                var bitwiseNandBit = (BitVecExpr)Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseNandBit.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var bitwiseNandExpr = _z3Ctx.MkBVRedAND(bitwiseNandBit);
                var convertBitToBool = _z3Ctx.MkEq(bitwiseNandExpr, _z3Ctx.MkBV(1, bitwiseNandExpr.SortSize));;
                // var bitwiseNandBool = _z3Ctx.MkNot((BoolExpr)convertBitToBool);
                return (Z3Expr.From(convertBitToBool), 
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.BitwiseOr:
                var bitwiseOrBit = (BitVecExpr)Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseOrBit.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var bitwiseOrExpr = _z3Ctx.MkBVRedAND(bitwiseOrBit);
                return (Z3Expr.From(bitwiseOrExpr), 
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.BitwiseNor:
                var bitwiseNorBit = (BitVecExpr)Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseNorBit.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var bitwiseNorExpr = _z3Ctx.MkBVRedAND(bitwiseNorBit);
                var convertBitToBool2 = _z3Ctx.MkEq(bitwiseNorExpr, _z3Ctx.MkBV(1, bitwiseNorExpr.SortSize));
                // var bitwiseNorBool = _z3Ctx.MkNot((BoolExpr)convertBitToBool2);
                return (Z3Expr.From(convertBitToBool2), 
                    operandVars);
            
            case SvUnaryExpression.UnaryOP.Xor:
                var bitwiseXorBit = (BitVecExpr)Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseXorBit.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var firstBitXor = _z3Ctx.MkExtract(0,0,bitwiseXorBit);
                var firstInputXor = _z3Ctx.MkEq(firstBitXor, _z3Ctx.MkBV(1, firstBitXor.SortSize));
                BoolExpr finalResultXor = null;
                var sizeXor = bitwiseXorBit.SortSize;
                for (uint i = 1; i < sizeXor; i++) {
                    var nextBitXor = _z3Ctx.MkExtract(i,i,bitwiseXorBit);
                    var secondInputXor = _z3Ctx.MkEq(nextBitXor, _z3Ctx.MkBV(1, nextBitXor.SortSize));
                    finalResultXor = _z3Ctx.MkXor(firstInputXor, secondInputXor);
                    firstInputXor = finalResultXor;
                }
                return (Z3Expr.From(finalResultXor),
                        operandVars);
            
            case SvUnaryExpression.UnaryOP.Xnor:
                var bitwiseXnorBit = (BitVecExpr)Types.AssertBitVecTypeOrFail(operandExpr);
                if (bitwiseXnorBit.SortSize <= 1) {
                    throw new IllegalExpression("The bit vector length should be greater than 1 in order to use this operator");
                }
                var firstBitXnor = _z3Ctx.MkExtract(0,0,bitwiseXnorBit);
                BitVecExpr finalResultXnor = null;
                var sizeXnor = bitwiseXnorBit.SortSize;
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
                Z3Expr.From(_currProblem.LookupVar(_currClsName +"#"+ hid.HierarchicalIds[0])),
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