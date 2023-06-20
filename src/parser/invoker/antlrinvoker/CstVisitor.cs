using Antlr4.Runtime.Tree;
using flashsolve.parser.ast;

namespace flashsolve.parser.invoker.antlrinvoker;

public class NonReachableControlFlow : Exception {
    public NonReachableControlFlow() {}
    public NonReachableControlFlow(string msg) : base(msg) {}
    public NonReachableControlFlow(string msg,Exception inner) : base(msg,inner) {}
}
public class CstVisitor : ISystemVerilogParserVisitor<SvAstNode> {
    public SvAstNode Visit(IParseTree tree) {
        return tree.Accept(this);
    }

    public SvAstNode VisitChildren(IRuleNode node) {
        throw new NonReachableControlFlow("Method VisitChildren is a generic hook that is not needed and should never be called, you probably made a mistake somewhere.");
    }

    public SvAstNode VisitTerminal(ITerminalNode node) {
        Console.WriteLine("I'm visiting a terminal ! its text is "+ node.GetText());
        return null;
    }

    public SvAstNode VisitErrorNode(IErrorNode node) {
        return null;
    }

    public SvAstNode VisitProgram(SystemVerilogParser.ProgramContext context) {
        var decls = context.class_decl();

        var svProg = new SvConstraintProgram();
        foreach (var classDeclaration in decls) {
            var svClass = classDeclaration.Accept(this)!;
            svProg.Add((SvClass)svClass);
        }

        return svProg;
    }

    public SvAstNode VisitClassDecl(SystemVerilogParser.ClassDeclContext context) {
        var cls = new SvClass(
            MkAntlrCstRef.FromClassDecl(context)
        );
        var items = context.class_item();
        foreach (var item in items) {
            var itemAst = item.Accept(this);
            if (itemAst is SvConstraint constraint) {
                cls.Add(constraint);
            }
            else if (itemAst is SvBitData data) {
                cls.Add(data);
            }
            else {
                throw new NonReachableControlFlow("Classes can only contain constraints or data definitions, found neither");
            }
        }

        return cls;
    }

    public SvAstNode VisitClassConstraint(SystemVerilogParser.ClassConstraintContext context) {
        return context.class_constraint().Accept(this);
    }

    public SvAstNode VisitClassDataDecl(SystemVerilogParser.ClassDataDeclContext context) {
        return context.class_data_decl().Accept(this);
    }

    public SvAstNode VisitConstraintPrototype(SystemVerilogParser.ConstraintPrototypeContext context) {
        return context.constraint_prototype().Accept(this);
    }

    public SvAstNode VisitConstraintDecl(SystemVerilogParser.ConstraintDeclContext context) {
        return context.constraint_decl().Accept(this);
    }

    public SvAstNode VisitConstraintPrototypeDecl(SystemVerilogParser.ConstraintPrototypeDeclContext context) {
        return new SvConstraint(
            MkAntlrCstRef.FromContraintPrototype(context)
        );
    }

    public SvAstNode VisitConstraintDeclBody(SystemVerilogParser.ConstraintDeclBodyContext context) {
        var constr = new SvConstraint(
            MkAntlrCstRef.FromConstraintDecl(context)
        ) {
            Items = (SvConstraint.Block) context.constraint_block().Accept(this)
        };
        return constr;
    }

    public SvAstNode VisitConstraintBlock(SystemVerilogParser.ConstraintBlockContext context) {
        var items = new List<SvConstraint.BlockItem>();

        foreach (var block_item in context.constraint_block_item()) {
            items.Add((SvConstraint.BlockItem) block_item.Accept(this));
        }

        return new SvConstraint.Block(items);
    }

    public SvAstNode VisitConstraintExpressionBlockItem(SystemVerilogParser.ConstraintExpressionBlockItemContext context) {
        return context.Accept(this);
    }

    public SvAstNode VisitExpressionOrDist(SystemVerilogParser.ExpressionOrDistContext context)
    {
        return context.expression_or_dist().Accept(this);
    }

    public SvAstNode VisitUniquenessConstraint(SystemVerilogParser.UniquenessConstraintContext context) {
        return context.uniqueness_constraint().Accept(this);
    }

    public SvAstNode VisitImpliesConstraint(SystemVerilogParser.ImpliesConstraintContext context) {
        var expr = context.expression().Accept(this);
        
        SvImplication svImplication = new((SvExpr)expr);
        
        var constraintSet = context.constraint_set().Accept(this);
        
        svImplication.ConstraintSet = (SvConstraintSet)constraintSet;
        return svImplication;
    }

    public SvAstNode VisitIfThenElseConstraint(SystemVerilogParser.IfThenElseConstraintContext context) {
        var cond = (SvExpr)context.expression().Accept(this);

        var thenAndElse = context.constraint_set();
        var thenCst = thenAndElse[0];
        var then = (SvConstraintSet)thenCst.Accept(this);
        
        var ifThenElse = new SvIfElse(cond) {
            Then = then
        };

        if (thenAndElse.Length > 1) {
            var els = (SvConstraintSet)thenAndElse[1].Accept(this);
            ifThenElse.Else = els;
        }
        return ifThenElse;
    }

    public SvAstNode VisitOpenRangeList(SystemVerilogParser.OpenRangeListContext context) {
        return  context.open_range_list().Accept(this);
    }

    public SvAstNode VisitConstraintExpressionSet(SystemVerilogParser.ConstraintExpressionSetContext context)
    {
        var constraintSet = new SvConstraintSet
        {
            (SvConstraint.Expr)context.constraint_expr().Accept(this)
        };
        return constraintSet;
    }

    public SvAstNode VisitConstraintExpressionsSet(SystemVerilogParser.ConstraintExpressionsSetContext context)
    {
        var constraintSet = new SvConstraintSet();

        var constraintExprCsts = context.constraint_expr();
        foreach (var constraintCst in constraintExprCsts)
        {
            constraintSet.Add((SvConstraint.Expr)constraintCst.Accept(this));
        }

        return constraintSet;
    }

    public SvAstNode VisitExprOrDist(SystemVerilogParser.ExprOrDistContext context) {
        return context.expression().Accept(this);

    }

    public SvAstNode VisitBitWiseOrOperator(SystemVerilogParser.BitWiseOrOperatorContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = SvBinaryExpression.Op.BitwiseOr;
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, op);
        return binaryExpression;
    }

    public SvAstNode VisitOrOperator(SystemVerilogParser.OrOperatorContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = SvBinaryExpression.Op.Or;
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, op);
        return binaryExpression;
    }

    public SvAstNode VisitIncOrDecExpression(SystemVerilogParser.IncOrDecExpressionContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitPrimaryExpression(SystemVerilogParser.PrimaryExpressionContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitAndOperator(SystemVerilogParser.AndOperatorContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = SvBinaryExpression.Op.And;
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, op);
        return binaryExpression;
    }

    public SvAstNode VisitMulDivModOperator(SystemVerilogParser.MulDivModOperatorContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = context.GetText();
        SvBinaryExpression.Op finalOp = SvBinaryExpression.Op.Mul; 
        if (op == "*") {
            finalOp = SvBinaryExpression.Op.Mul;
        }
        else if (op == "/") {
            finalOp = SvBinaryExpression.Op.Div;
        }
        else if (op == "%") {
            finalOp = SvBinaryExpression.Op.Mod;
        }
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitConditionalExpression(SystemVerilogParser.ConditionalExpressionContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitAssignmentOperator(SystemVerilogParser.AssignmentOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitEqualityOperators(SystemVerilogParser.EqualityOperatorsContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = context.GetText();
        SvBinaryExpression.Op finalOp = SvBinaryExpression.Op.Equal; 
        if (op == "==") {
            finalOp = SvBinaryExpression.Op.Equal;
        }
        else if (op == "!=") {
            finalOp = SvBinaryExpression.Op.NotEqual;
        }
        else if (op == "===") {
            finalOp = SvBinaryExpression.Op.EqualXZ;
        }
        else if (op == "!==") {
            finalOp = SvBinaryExpression.Op.NotEqualXZ;
        }
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitBitWiseAndOperator(SystemVerilogParser.BitWiseAndOperatorContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = SvBinaryExpression.Op.BitwiseAnd;
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, op);
        return binaryExpression;
    }

    public SvAstNode VisitAddSubOperator(SystemVerilogParser.AddSubOperatorContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = context.GetText();
        SvBinaryExpression.Op finalOp = SvBinaryExpression.Op.Plus; 
        if (op == "+") {
            finalOp = SvBinaryExpression.Op.Plus;
        }
        else if (op == "-") {
            finalOp = SvBinaryExpression.Op.Minus;
        }

        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitComparisonOperators(SystemVerilogParser.ComparisonOperatorsContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = context.GetText();
        SvBinaryExpression.Op finalOp = SvBinaryExpression.Op.Less; 
        if (op == "<") {
            finalOp = SvBinaryExpression.Op.Less;
        }
        else if (op == ">") {
            finalOp = SvBinaryExpression.Op.Greater;
        }
        else if (op == "<=") {
            finalOp = SvBinaryExpression.Op.LessEqual;
        }
        else if (op == ">=") {
            finalOp = SvBinaryExpression.Op.GreaterEqual;
        }
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitPowerOperator(SystemVerilogParser.PowerOperatorContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = SvBinaryExpression.Op.Exp;
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, op);
        return binaryExpression;
    }

    public SvAstNode VisitBitWiseXorXnorOperator(SystemVerilogParser.BitWiseXorXnorOperatorContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = context.GetText();
        SvBinaryExpression.Op finalOp = SvBinaryExpression.Op.BitwiseXor; 
        if (op == "^") {
            finalOp = SvBinaryExpression.Op.BitwiseXor;
        }
        else if (op == "~^" || op == "^~") {
            finalOp = SvBinaryExpression.Op.BitwiseXnor;
        }
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitUnaryOperator(SystemVerilogParser.UnaryOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitShiftOperators(SystemVerilogParser.ShiftOperatorsContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = context.GetText();
        SvBinaryExpression.Op finalOp = SvBinaryExpression.Op.LogicalShiftLeft; 
        if (op == "<<") {
            finalOp = SvBinaryExpression.Op.LogicalShiftLeft;
        }
        else if (op == ">>") {
            finalOp = SvBinaryExpression.Op.LogicalShiftRight;
        }
        else if (op == "<<<") {
            finalOp = SvBinaryExpression.Op.ArithmeticShiftLeft;
        }
        else if (op == ">>>") {
            finalOp = SvBinaryExpression.Op.ArithmeticShiftRight;
        }
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitInsideExpression(SystemVerilogParser.InsideExpressionContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitPrefix(SystemVerilogParser.PrefixContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitPostfix(SystemVerilogParser.PostfixContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitAssignmentStatement(SystemVerilogParser.AssignmentStatementContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConditionalPattern(SystemVerilogParser.ConditionalPatternContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitExprOrCond(SystemVerilogParser.ExprOrCondContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitMatchesPattern(SystemVerilogParser.MatchesPatternContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitPrimaryLiteral(SystemVerilogParser.PrimaryLiteralContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitHierarchicalIdentifier(SystemVerilogParser.HierarchicalIdentifierContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitHierarchicalId(SystemVerilogParser.HierarchicalIdContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitNumberPrimaryLit(SystemVerilogParser.NumberPrimaryLitContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitStringPrimaryLit(SystemVerilogParser.StringPrimaryLitContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitAttributeInstance(SystemVerilogParser.AttributeInstanceContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitDotIdPattern(SystemVerilogParser.DotIdPatternContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitDotStarPattern(SystemVerilogParser.DotStarPatternContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitValueRanges(SystemVerilogParser.ValueRangesContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitExprValRange(SystemVerilogParser.ExprValRangeContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitOpenRange(SystemVerilogParser.OpenRangeContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitLVariableId(SystemVerilogParser.LVariableIdContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitDataTypeClassDecl(SystemVerilogParser.DataTypeClassDeclContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitBitDataType(SystemVerilogParser.BitDataTypeContext context) {
        throw new NotImplementedException();
    }
}
