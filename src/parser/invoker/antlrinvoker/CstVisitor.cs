using Antlr4.Runtime.Tree;
using flashsolve.parser.ast;

namespace flashsolve.parser.invoker.antlrinvoker;

public class VisitorMethodShouldNotBeCalled : Exception {
    public VisitorMethodShouldNotBeCalled() {}
    public VisitorMethodShouldNotBeCalled(string msg) : base(msg) {}
    public VisitorMethodShouldNotBeCalled(string msg,Exception inner) : base(msg,inner) {}
}
public class CstVisitor : ISystemVerilogParserVisitor<SvAstNode> {
    public SvAstNode Visit(IParseTree tree) {
        return tree.Accept(this);
    }

    public SvAstNode VisitChildren(IRuleNode node) {
        throw new VisitorMethodShouldNotBeCalled("Method VisitChildren is a generic hook that is not needed, you probably made a mistake somewhere");
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
        return new SvClass(MkAntlrCstRef.FromClassDecl(context));
    }

    public SvAstNode VisitClassConstraint(SystemVerilogParser.ClassConstraintContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitClassDataDecl(SystemVerilogParser.ClassDataDeclContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraintPrototype(SystemVerilogParser.ConstraintPrototypeContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraintDecl(SystemVerilogParser.ConstraintDeclContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraintPrototypeDecl(SystemVerilogParser.ConstraintPrototypeDeclContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraintDeclBody(SystemVerilogParser.ConstraintDeclBodyContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraintBlock(SystemVerilogParser.ConstraintBlockContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraintExpressionBlockItem(SystemVerilogParser.ConstraintExpressionBlockItemContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitExpressionOrDist(SystemVerilogParser.ExpressionOrDistContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitUniquenessConstraint(SystemVerilogParser.UniquenessConstraintContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitImpliesConstraint(SystemVerilogParser.ImpliesConstraintContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitIfThenElseConstraint(SystemVerilogParser.IfThenElseConstraintContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitOpenRangeList(SystemVerilogParser.OpenRangeListContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraintExpressionSet(SystemVerilogParser.ConstraintExpressionSetContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraintExpressionsSet(SystemVerilogParser.ConstraintExpressionsSetContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitExprOrDist(SystemVerilogParser.ExprOrDistContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitBitWiseOrOperator(SystemVerilogParser.BitWiseOrOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitOrOperator(SystemVerilogParser.OrOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitIncOrDecExpression(SystemVerilogParser.IncOrDecExpressionContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitPrimaryExpression(SystemVerilogParser.PrimaryExpressionContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitAndOperator(SystemVerilogParser.AndOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitMulDivModOperator(SystemVerilogParser.MulDivModOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConditionalExpression(SystemVerilogParser.ConditionalExpressionContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitAssignmentOperator(SystemVerilogParser.AssignmentOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitEqualityOperators(SystemVerilogParser.EqualityOperatorsContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitBitWiseAndOperator(SystemVerilogParser.BitWiseAndOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitAddSubOperator(SystemVerilogParser.AddSubOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitComparisonOperators(SystemVerilogParser.ComparisonOperatorsContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitPowerOperator(SystemVerilogParser.PowerOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitBitWiseXorXnorOperator(SystemVerilogParser.BitWiseXorXnorOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitUnaryOperator(SystemVerilogParser.UnaryOperatorContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitShiftOperators(SystemVerilogParser.ShiftOperatorsContext context) {
        throw new NotImplementedException();
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