using Antlr4.Runtime.Tree;

namespace FlashSolve.parser {
    class CstToAst : ISystemVerilogVisitor<string> {
        
        public string Visit(IParseTree tree)
        {
            throw new NotImplementedException();
        }

        public string VisitChildren(IRuleNode node)
        {
            throw new NotImplementedException();
        }

        public string VisitTerminal(ITerminalNode node)
        {
            throw new NotImplementedException();
        }

        public string VisitErrorNode(IErrorNode node)
        {
            throw new NotImplementedException();
        }

        public string VisitSvprogram(SystemVerilogParser.SvprogramContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitClass_decl(SystemVerilogParser.Class_declContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitClass_item(SystemVerilogParser.Class_itemContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitClass_constraint(SystemVerilogParser.Class_constraintContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitConstraint_prototype(SystemVerilogParser.Constraint_prototypeContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitConstraint_decl(SystemVerilogParser.Constraint_declContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitConstraint_block(SystemVerilogParser.Constraint_blockContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitConstraint_block_item(SystemVerilogParser.Constraint_block_itemContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitConstraint_expr(SystemVerilogParser.Constraint_exprContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitUniqueness_constraint(SystemVerilogParser.Uniqueness_constraintContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitConstraint_set(SystemVerilogParser.Constraint_setContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitExpression_or_dist(SystemVerilogParser.Expression_or_distContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitExpression(SystemVerilogParser.ExpressionContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitInc_or_dec_expression(SystemVerilogParser.Inc_or_dec_expressionContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitOperator_assignment(SystemVerilogParser.Operator_assignmentContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitExpression_or_cond_pattern(SystemVerilogParser.Expression_or_cond_patternContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitCond_pattern(SystemVerilogParser.Cond_patternContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitPrimary(SystemVerilogParser.PrimaryContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitHierarchical_identifier(SystemVerilogParser.Hierarchical_identifierContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitPrimary_literal(SystemVerilogParser.Primary_literalContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitAttribute_instance(SystemVerilogParser.Attribute_instanceContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitPattern(SystemVerilogParser.PatternContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitOpen_range_list(SystemVerilogParser.Open_range_listContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitValue_range(SystemVerilogParser.Value_rangeContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitVariable_lvalue(SystemVerilogParser.Variable_lvalueContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitClass_data_decl(SystemVerilogParser.Class_data_declContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitData_type(SystemVerilogParser.Data_typeContext context)
        {
            throw new NotImplementedException();
        }
        
        private record SvClassCstRef(SystemVerilogParser.Class_declContext ctx) : CstRef {
            public string Name => ctx.GetChild(1).GetText();
        }
    }
}