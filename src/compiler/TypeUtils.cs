namespace flashsolve.compiler; 

using Microsoft.Z3;

public static class Types {
    public static (Z3Expr.BitVec, Z3Expr.BitVec) 
        AssertBitVecTypeOrFail(Z3Expr left, Z3Expr right) {
            if (left is Z3Expr.BitVec l) {
                if (right is Z3Expr.BitVec r) {
                    return (l, r);
                }
            }

            throw new TypeMismatch($"Expected the 2 objects ${left} and ${right} to both have BitVec type, found that they are not.");
    }
    public static Z3Expr.Bool
        AssertBoolTypeOrFail(Z3Expr expr) {
            if (expr is Z3Expr.Bool e) {
                return e;
            }

           throw new TypeMismatch($"Expected the objects ${expr} to have bool type, found that they are not.");
    }
    public static (BitVecExpr, BitVecExpr) 
        MakeSameSizeByZeroExtension(BitVecExpr l, BitVecExpr r, Context z3Ctx) {
        if (l.SortSize > r.SortSize) {
            return (l,
                    z3Ctx.MkZeroExt(l.SortSize - r.SortSize, r)
                );
        }
        else {
            return (z3Ctx.MkZeroExt(r.SortSize - l.SortSize, l),
                    r
                );
        }
    }

    public static (Z3Expr.Bool, Z3Expr.Bool)
        AssertBoolTypeOrFail(Z3Expr left, Z3Expr right) {
        if (left is Z3Expr.Bool l) {
            if (right is Z3Expr.Bool r) {
                return (l, r);
            }
        }

        throw new TypeMismatch($"Expected the 2 objects ${left} and ${right} to have Bool type, found that they are not.");
    }


    public static Expr 
        AssertBitVecTypeOrFail(Z3Expr expr) {
            if (expr is Z3Expr.BitVec e) {
                return (e);
            }
            throw new TypeMismatch($"Expected the object ${expr} to have BitVec type, found that the object is not.");
    }
}