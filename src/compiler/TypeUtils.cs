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

        throw new TypeMismatch($"Expected the 2 objects ${left} and ${right} to have BitVec type, found that they are not.");
    }

    public static (BitVecExpr, BitVecExpr) 
        MakeSameSizeByZeroExtension(BitVecExpr l, BitVecExpr r, Context z3ctx) {
        if (l.SortSize > r.SortSize) {
            return (l,
                    z3ctx.MkZeroExt(l.SortSize - r.SortSize, r)
                );
        }
        else {
            return (z3ctx.MkZeroExt(r.SortSize - l.SortSize, l),
                    r
                );
        }
    }
}