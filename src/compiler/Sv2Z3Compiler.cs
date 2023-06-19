using Microsoft.Z3;

namespace flashsolve.compiler; 

using flashsolve.parser.ast;

public static class Sv2Z3Compiler {
    public static RandProblem Compile(this SvConstraintProgram prog) {
        var result = new RandProblem();
        
        foreach (var cls in prog) {
            var problem = cls.Compile();
            result.Merge(problem);
        }

        return result;
    }

    public static RandProblem Compile(this SvClass cls) {
        var result = new RandProblem();
        foreach (var dataDef in cls.Members) {
            string varName = $"${cls.Name}#${dataDef.Name}";
            result.AddVar(varName);
        }

        foreach (var svConstraint in cls.Constraints) {
            result.AddConstraint(svConstraint.Compile());
        }

        return result;
    }

    public static BoolExpr Compile(this SvConstraint constraint) {
        throw new Exception("qwkepqwkep");
    }
}