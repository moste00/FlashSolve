
using Antlr4.Runtime.Tree;

namespace FlashSolve {
    namespace parser {
        class CSTVisitor : ISystemVerilogVisitor<String>
        {
            string IParseTreeVisitor<string>.Visit(IParseTree tree)
            {
                throw new NotImplementedException();
            }

            string IParseTreeVisitor<string>.VisitChildren(IRuleNode node)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitClass_constraint(SystemVerilogParser.Class_constraintContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitClass_data_decl(SystemVerilogParser.Class_data_declContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitClass_decl(SystemVerilogParser.Class_declContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitClass_item(SystemVerilogParser.Class_itemContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitConstraint_block(SystemVerilogParser.Constraint_blockContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitConstraint_block_item(SystemVerilogParser.Constraint_block_itemContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitConstraint_decl(SystemVerilogParser.Constraint_declContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitConstraint_expr(SystemVerilogParser.Constraint_exprContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitConstraint_prototype(SystemVerilogParser.Constraint_prototypeContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitConstraint_set(SystemVerilogParser.Constraint_setContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitData_type(SystemVerilogParser.Data_typeContext context)
            {
                throw new NotImplementedException();
            }

            string IParseTreeVisitor<string>.VisitErrorNode(IErrorNode node)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitExpression(SystemVerilogParser.ExpressionContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitExpression_or_dist(SystemVerilogParser.Expression_or_distContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitOpen_range_list(SystemVerilogParser.Open_range_listContext context)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitSvprogram(SystemVerilogParser.SvprogramContext context)
            {
                throw new NotImplementedException();
            }

            string IParseTreeVisitor<string>.VisitTerminal(ITerminalNode node)
            {
                throw new NotImplementedException();
            }

            string ISystemVerilogVisitor<string>.VisitUniqueness_constraint(SystemVerilogParser.Uniqueness_constraintContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}