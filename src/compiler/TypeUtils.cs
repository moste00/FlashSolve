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
    
    public static (Z3Expr.Bool, Z3Expr.Bool)
        AssertBoolTypeOrFail(Z3Expr left, Z3Expr right) {
        if (left is Z3Expr.Bool l) {
            if (right is Z3Expr.Bool r) {
                return (l, r);
            }
        }

        throw new TypeMismatch($"Expected the 2 objects ${left} and ${right} to have Bool type, found that they are not.");
    }


    public static Z3Expr.BitVec 
        AssertBitVecTypeOrFail(Z3Expr expr) {
        if (expr is Z3Expr.BitVec e) {
            return (e);
        }
        throw new TypeMismatch($"Expected the object ${expr} to have BitVec type, found that the object is not.");
    }
    public static Z3Expr.Bool
        AssertBoolTypeOrFail(Z3Expr expr) {
            if (expr is Z3Expr.Bool e) {
                return e;
            }

           throw new TypeMismatch($"Expected the objects ${expr} to have bool type, found that they are not.");
    }
    
    public static (BitVecExpr, BitVecExpr) 
        MakeSameSizeBySignExtension(BitVecExpr l, BitVecExpr r, Context z3Ctx) {
        if (l.SortSize > r.SortSize) {
            return (
                l,
                z3Ctx.MkSignExt(l.SortSize - r.SortSize, r)
            );
        }
        return (
            z3Ctx.MkSignExt(r.SortSize - l.SortSize, l),
            r
        );
    }
    public static (BitVecExpr, BitVecExpr,BitVecExpr) 
        MakeSameSizeBySignExtension(BitVecExpr l, BitVecExpr m, BitVecExpr r, Context z3Ctx) {
        //Precondition : (l.SortSize == r.SortSize && l.SortSize == m.SortSize) is false
        uint maxSz;
        
        //one of those 2 conditions must be a strict inequality
        if (l.SortSize >= m.SortSize &&
            l.SortSize >= r.SortSize) {
            maxSz = l.SortSize;

            if (l.SortSize == r.SortSize) {
                return (l, z3Ctx.MkSignExt(maxSz - m.SortSize, m), r);
            }
            if (l.SortSize == m.SortSize) {
                return (l, m, z3Ctx.MkSignExt(maxSz - r.SortSize, r));
            }
            return (
                l,
                z3Ctx.MkSignExt(maxSz - m.SortSize, m),
                z3Ctx.MkSignExt(maxSz - r.SortSize, r)
            );
        }
        /*
         * once we get here, one of the following or both is true
         *          -- m.SortSize is greater than l.SortSize
         *          -- r.SortSize is greater than l.SortSize
         */
        if (m.SortSize >= r.SortSize) {
            maxSz = m.SortSize;

            BitVecExpr lSameSz = z3Ctx.MkSignExt(maxSz - l.SortSize, l);
            if (r.SortSize == m.SortSize) {
                return (lSameSz, m, r);
            }
            BitVecExpr rSameSz = z3Ctx.MkSignExt(maxSz - r.SortSize, r);
            return (lSameSz, m, rSameSz);
        }
        /*
         * once we get here, one we know for sure that r.SortSize is the greatest possible size
         */
        maxSz = r.SortSize;
        return (
            z3Ctx.MkSignExt(maxSz - l.SortSize,l),
            z3Ctx.MkSignExt(maxSz - m.SortSize,m),
            r
        );
    }
    
    public static (BitVecExpr, BitVecExpr) 
        MakeSameSizeByZeroExtension(BitVecExpr l, BitVecExpr r, Context z3Ctx) {
        if (l.SortSize > r.SortSize) {
            return (
                l,
                z3Ctx.MkZeroExt(l.SortSize - r.SortSize, r)
            );
        }
        return (
            z3Ctx.MkZeroExt(r.SortSize - l.SortSize, l),
            r
        );
    }
    public static (BitVecExpr, BitVecExpr,BitVecExpr) 
        MakeSameSizeByZeroExtension(BitVecExpr l, BitVecExpr m, BitVecExpr r, Context z3Ctx) {
        //Precondition : (l.SortSize == r.SortSize && l.SortSize == m.SortSize) is false
        uint maxSz;
        
        //one of those 2 conditions must be a strict inequality
        if (l.SortSize >= m.SortSize &&
            l.SortSize >= r.SortSize) {
            maxSz = l.SortSize;

            if (l.SortSize == r.SortSize) {
                return (l, z3Ctx.MkZeroExt(maxSz - m.SortSize, m), r);
            }
            if (l.SortSize == m.SortSize) {
                return (l, m, z3Ctx.MkZeroExt(maxSz - r.SortSize, r));
            }
            return (
                l,
                z3Ctx.MkZeroExt(maxSz - m.SortSize, m),
                z3Ctx.MkZeroExt(maxSz - r.SortSize, r)
            );
        }
        /*
         * once we get here, one of the following or both is true
         *          -- m.SortSize is greater than l.SortSize
         *          -- r.SortSize is greater than l.SortSize
         */
        if (m.SortSize >= r.SortSize) {
            maxSz = m.SortSize;

            BitVecExpr lSameSz = z3Ctx.MkZeroExt(maxSz - l.SortSize, l);
            if (r.SortSize == m.SortSize) {
                return (lSameSz, m, r);
            }
            BitVecExpr rSameSz = z3Ctx.MkZeroExt(maxSz - r.SortSize, r);
            return (lSameSz, m, rSameSz);
        }
        /*
         * once we get here, one we know for sure that r.SortSize is the greatest possible size
         */
        maxSz = r.SortSize;
        return (
            z3Ctx.MkZeroExt(maxSz - l.SortSize,l),
            z3Ctx.MkZeroExt(maxSz - m.SortSize,m),
            r
        );
    }
}