using System.Diagnostics;
using System.Numerics;

namespace flashsolve.compiler; 

using Microsoft.Z3;
using util;

public class Z3Expr {
    public class Bool : Z3Expr {
        private BoolExpr _e;
        public Bool(BoolExpr e) {
            this._e = e;
        }
        public BoolExpr Expr => _e;
        public static implicit operator BoolExpr(Bool b) => b._e;
        public static implicit operator Expr(Bool b) => b._e;
    }
    public class BitVec : Z3Expr {
        private bool _isSigned;
        private BitVecExpr _e;
        public BitVec(BitVecExpr e) {
            this._e = e;
        }
        public BitVecExpr Expr => _e;
        public static implicit operator BitVecExpr(BitVec bv) => bv._e;
        public static implicit operator Expr(BitVec bv) => bv._e;
        
        public BitVec SetSigned(bool s) {
            _isSigned = s;
            return this;
        }
        public static BitVec FromSvNum(string numLitNumber, Context z3Ctx) {
            //decimal literal
            if (!numLitNumber.Any(
                    (c) => c == '\'')
               ) {
                return FromSvNumUnbased(numLitNumber, z3Ctx);
            }
            
            var parts = numLitNumber.Split("'");
            if (parts.Length != 2) {
                throw new UnrecognizedNumberFormat(
                    "Something is wrong about this number literal, it shouldn't have been parsed successfully.");
            }

            var size = parts[0];
            var num = parts[1];
            uint sizeBits = 32;
            if (size != "") {
                if (!UInt32.TryParse(size, 
                                 out sizeBits)) {
                    throw new UnrecognizedNumberFormat(
                        "Something is wrong about this number literal, it shouldn't have been parsed successfully.");
                }
            }

            if (sizeBits <= 64) {
                return FromSmallSvNumBased(num, sizeBits, z3Ctx);
            }

            return FromBigSvNumBased(num, sizeBits, z3Ctx);
        }

        private static BitVec FromSvNumUnbased(string numLitNumber, Context z3Ctx) {
            return Z3Expr.From(
                z3Ctx.MkBV(Int32.Parse(numLitNumber), 32)
            );
        }

        private static BitVec FromBigSvNumBased(string num, uint sizeBits, Context z3Ctx) {
            bool isSigned = num[0] == 's' || num[0] == 'S';
            uint baseIndex = (uint)(isSigned? 
                             1:
                             0);
            char basee = num[(int)baseIndex];

            switch (basee) {
                case 'h':
                case 'H': {
                    bool[] number = num.Slice()
                                   .From(baseIndex+1)
                                   .ToEnd
                                   .HexToBoolArr(sizeBits);
                    return Z3Expr.From(z3Ctx.MkBV(number));
                }
                case 'b':
                case 'B': {
                    bool[] number = num.Slice()
                                   .From(baseIndex+1)
                                   .ToEnd
                                   .BinToBoolArr(sizeBits);
                    return Z3Expr.From(z3Ctx.MkBV(number));
                }
                case 'o':
                case 'O': {
                    bool[] number = num.Slice()
                                   .From(baseIndex+1)
                                   .ToEnd
                                   .OctToBoolArr(sizeBits);
                    return Z3Expr.From(z3Ctx.MkBV(number));
                }
                default:
                    throw new UnrecognizedNumberFormat($"Base {basee} is an unrecognized number system.");
            }
        }

        private static BitVec FromSmallSvNumBased(string num, uint sizeBits, Context z3Ctx) { 
            bool isSigned = num[0] == 's' || num[0] == 'S';
            uint baseIndex = (uint)(isSigned? 
                            1:
                            0);
            char basee = num[(int)baseIndex];
            UInt64 bas = basee switch {
                'h' => (UInt64)16,
                'H' => (UInt64)16,
                
                'o' => (UInt64)8,
                'O' => (UInt64)8,
                
                'b' => (UInt64)2,
                'B' => (UInt64)2,
                
                _ => throw new UnrecognizedNumberFormat($"Base {basee} is an unrecognized number system.")
            };
            UInt64 pow = 1;
            UInt64 number = 0;
            foreach (var digit in num.Slice()
                                     .From(baseIndex+1)
                                     .ToEnd
                                     .Reversed) {
                int digitAsNum = digit switch {
                    '0' => 0,
                    '1' => 1,
                    '2' => 2,
                    '3' => 3,
                    '4' => 4,
                    '5' => 5,
                    '6' => 6,
                    '7' => 7,
                    '8' => 8,
                    '9' => 9,
                    'a' => 10,
                    'A' => 10,
                    'b' => 11,
                    'B' => 11,
                    'c' => 12,
                    'C' => 12,
                    'd' => 13,
                    'D' => 13,
                    'e' => 14,
                    'E' => 14,
                    'f' => 15,
                    'F' => 15,
                    
                    _ => throw new UnrecognizedNumberFormat($"Digit {digit} is unrecognized.")
                };
                
                number += pow * (uint)digitAsNum;
                pow *= bas;
            }

            return Z3Expr.From(
                    z3Ctx.MkBV(number,sizeBits)
            );
;        }
    }

    public static Bool From(BoolExpr e) => new(e);
    public static BitVec From(BitVecExpr e) => new(e);

    public static implicit operator Expr(Z3Expr e) =>
        (e is BitVec bv) ? bv.Expr :
        (e is Bool b   ) ? b.Expr  : 
        throw new NoSuchChildClass(
            $"{e.GetType().Name} is not a recognized child class of Z3Expr");
    public static Expr[] ToZ3(List<Z3Expr> z3Exprs) {
        Expr[] exprs = new Expr[z3Exprs.Count];
        int i = 0;
        foreach (var z3Exp in z3Exprs) {
            exprs[i++] = z3Exp;
        }

        return exprs;
    }
}