using Antlr4.Runtime.Tree;
using flashsolve.parser.ast;

namespace flashsolve.parser.invoker.antlrinvoker; 

public class CstVisitor : ISystemVerilogVisitor<SvAstNode> {
    public SvAstNode Visit(IParseTree tree) {
        tree.Accept(this);
        return null;
    }

    public SvAstNode VisitChildren(IRuleNode node) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitTerminal(ITerminalNode node) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitErrorNode(IErrorNode node) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitSvprogram(SystemVerilogParser.SvprogramContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitClass_decl(SystemVerilogParser.Class_declContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitClass_item(SystemVerilogParser.Class_itemContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitClass_constraint(SystemVerilogParser.Class_constraintContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraint_prototype(SystemVerilogParser.Constraint_prototypeContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraint_decl(SystemVerilogParser.Constraint_declContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraint_block(SystemVerilogParser.Constraint_blockContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraint_block_item(SystemVerilogParser.Constraint_block_itemContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraint_expr(SystemVerilogParser.Constraint_exprContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitUniqueness_constraint(SystemVerilogParser.Uniqueness_constraintContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitConstraint_set(SystemVerilogParser.Constraint_setContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitExpression_or_dist(SystemVerilogParser.Expression_or_distContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitExpression(SystemVerilogParser.ExpressionContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitInc_or_dec_expression(SystemVerilogParser.Inc_or_dec_expressionContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitOperator_assignment(SystemVerilogParser.Operator_assignmentContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitExpression_or_cond_pattern(SystemVerilogParser.Expression_or_cond_patternContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitCond_pattern(SystemVerilogParser.Cond_patternContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitPrimary(SystemVerilogParser.PrimaryContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitHierarchical_identifier(SystemVerilogParser.Hierarchical_identifierContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitPrimary_literal(SystemVerilogParser.Primary_literalContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitAttribute_instance(SystemVerilogParser.Attribute_instanceContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitPattern(SystemVerilogParser.PatternContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitOpen_range_list(SystemVerilogParser.Open_range_listContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitValue_range(SystemVerilogParser.Value_rangeContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitVariable_lvalue(SystemVerilogParser.Variable_lvalueContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitClass_data_decl(SystemVerilogParser.Class_data_declContext context) {
        throw new NotImplementedException();
    }

    public SvAstNode VisitData_type(SystemVerilogParser.Data_typeContext context) {
        throw new NotImplementedException();
    }
}