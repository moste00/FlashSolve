using Antlr4.Runtime.Tree;
using flashsolve.parser.ast;

namespace flashsolve.parser.invoker.antlrinvoker;

public class NonReachableControlFlow : Exception {
    public NonReachableControlFlow() {}
    public NonReachableControlFlow(string msg) : base(msg) {}
    public NonReachableControlFlow(string msg,Exception inner) : base(msg,inner) {}
}
public class IllegalExpression : Exception {
    public IllegalExpression() {}
    public IllegalExpression(string msg) : base(msg) {}
    public IllegalExpression(string msg,Exception inner) : base(msg,inner) {}
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
            switch (itemAst) {
                case SvConstraint constraint:
                    cls.Add(constraint);
                    break;
                case SvBitData data:
                    cls.Add(data);
                    break;
                default:
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
        return context.inc_or_dec_expression().Accept(this);
    }

    public SvAstNode VisitPrimaryExpression(SystemVerilogParser.PrimaryExpressionContext context) {
        return context.primary().Accept(this);
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
        var op = context.BINARY_OPERATOR_2().GetText();
        SvBinaryExpression.Op finalOp; 
        switch (op) {
            case "*":
                finalOp = SvBinaryExpression.Op.Mul;
                break;
            case "/":
                finalOp = SvBinaryExpression.Op.Div;
                break;
            case "%":
                finalOp = SvBinaryExpression.Op.Mod;
                break;
            default:
                throw new NonReachableControlFlow($"This operator is unrecognized (operator {op})");
        }
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitConditionalExpression(SystemVerilogParser.ConditionalExpressionContext context) {
        var exprs = context.expression();
        var then = (SvExpr)exprs.Last().Accept(this);
        var els = (SvExpr)exprs[exprs.Length - 2].Accept(this);
        SvConditionalExpression condExpr = new SvConditionalExpression(then, els);
        for (int i = 0; i < exprs.Length - 2; i++) {
            var item = (SvExpr)context.expression()[i].Accept(this);
            condExpr.Add(item);
        }

        return condExpr;
    }

    public SvAstNode VisitAssignmentOperator(SystemVerilogParser.AssignmentOperatorContext context) {
        throw new IllegalExpression("Assignment operator is not supported inside constraint expression");
    }

    public SvAstNode VisitEqualityOperators(SystemVerilogParser.EqualityOperatorsContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = context.BINARY_OPERATOR_6().GetText();
        SvBinaryExpression.Op finalOp; 
        switch (op) {
            case "==":
                finalOp = SvBinaryExpression.Op.Equal;
                break;
            case "!=":
                finalOp = SvBinaryExpression.Op.NotEqual;
                break;
            case "===":
                finalOp = SvBinaryExpression.Op.EqualXZ;
                break;
            case "!==":
                finalOp = SvBinaryExpression.Op.NotEqualXZ;
                break;
            default:
                throw new NonReachableControlFlow($"This operator is unrecognized (operator {op})");
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
        var op = context.BINARY_OPERATOR_3().GetText();
        SvBinaryExpression.Op finalOp; 
        switch (op) {
            case "+":
                finalOp = SvBinaryExpression.Op.Plus;
                break;
            case "-":
                finalOp = SvBinaryExpression.Op.Minus;
                break;
            default:
                throw new NonReachableControlFlow($"This operator is unrecognized (operator {op})");
        }

        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitComparisonOperators(SystemVerilogParser.ComparisonOperatorsContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = context.BINARY_OPERATOR_5().GetText();
        SvBinaryExpression.Op finalOp; 
        switch (op) {
            case "<":
                finalOp = SvBinaryExpression.Op.Less;
                break;
            case ">":
                finalOp = SvBinaryExpression.Op.Greater;
                break;
            case "<=":
                finalOp = SvBinaryExpression.Op.LessEqual;
                break;
            case ">=":
                finalOp = SvBinaryExpression.Op.GreaterEqual;
                break;
            default:
                throw new NonReachableControlFlow($"This operator is unrecognized (operator {op})");
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
        var op = context.BINARY_OPERATOR_8().GetText();
        SvBinaryExpression.Op finalOp; 
        switch (op) {
            case "^":
                finalOp = SvBinaryExpression.Op.BitwiseXor;
                break;
            case "~^":
            case "^~":
                finalOp = SvBinaryExpression.Op.BitwiseXnor;
                break;
            default:
                throw new NonReachableControlFlow($"This operator is unrecognized (operator {op})");
        }
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitUnaryOperator(SystemVerilogParser.UnaryOperatorContext context) {
        var op = context.UNARY_OPERATOR().GetText();
        var primary = (SvPrimary)context.primary().Accept(this);
        SvUnary.UnaryOP finalOp;
        switch (op) {
            case "+":
                finalOp = SvUnary.UnaryOP.Plus;
                break;
            case "-":
                finalOp = SvUnary.UnaryOP.Minus;
                break;
            case "!":
                finalOp = SvUnary.UnaryOP.Negation;
                break;
            case "~":
                finalOp = SvUnary.UnaryOP.Complement;
                break;
            case "&":
                finalOp = SvUnary.UnaryOP.BitwiseAnd;
                break;
            case "|":
                finalOp = SvUnary.UnaryOP.BitwiseOr;
                break;
            case "~&":
                finalOp = SvUnary.UnaryOP.BitwiseNand;
                break;
            case "~|":
                finalOp = SvUnary.UnaryOP.BitwiseNor;
                break;
            case "~^":
                finalOp = SvUnary.UnaryOP.Xnor;
                break;
            case "^~":
                finalOp = SvUnary.UnaryOP.Xor;
                break;
            default:
                throw new NonReachableControlFlow($"This operator is unrecognized (operator {op})");
        }
        SvUnary unary = new SvUnary(primary, finalOp);
        return unary;
    }

    public SvAstNode VisitShiftOperators(SystemVerilogParser.ShiftOperatorsContext context) {
        var left = (SvExpr)context.expression()[0].Accept(this);
        var right = (SvExpr)context.expression()[1].Accept(this);
        var op = context.BINARY_OPERATOR_4().GetText();
        SvBinaryExpression.Op finalOp; 
        switch (op) {
            case "<<":
                finalOp = SvBinaryExpression.Op.LogicalShiftLeft;
                break;
            case ">>":
                finalOp = SvBinaryExpression.Op.LogicalShiftRight;
                break;
            case "<<<":
                finalOp = SvBinaryExpression.Op.ArithmeticShiftLeft;
                break;
            case ">>>":
                finalOp = SvBinaryExpression.Op.ArithmeticShiftRight;
                break;
            default:
                throw new NonReachableControlFlow($"This operator is unrecognized (operator {op})");
        }
        SvBinaryExpression binaryExpression = new SvBinaryExpression(left, right, finalOp);
        return binaryExpression;
    }

    public SvAstNode VisitInsideExpression(SystemVerilogParser.InsideExpressionContext context) {
        var expr = (SvExpr)context.expression().Accept(this);
        SvInsideExpression insideExpression = new SvInsideExpression(expr);
        var openRange = (SvOpenRange)context.open_range_list().Accept(this);
        insideExpression.OpenRange = openRange;
        return insideExpression;
    }

    public SvAstNode VisitPrefix(SystemVerilogParser.PrefixContext context) {
        var variableId = (SvVariableLvalue)context.variable_lvalue().Accept(this);
        
        SvIncOrDecExpr incOrDecExpr = new SvIncOrDecExpr(variableId, true);
        return incOrDecExpr;
    }

    public SvAstNode VisitPostfix(SystemVerilogParser.PostfixContext context) {
        var variableId = (SvVariableLvalue)context.variable_lvalue().Accept(this);
        
        SvIncOrDecExpr incOrDecExpr = new SvIncOrDecExpr(variableId, false);
        return incOrDecExpr;
    }

    public SvAstNode VisitAssignmentStatement(SystemVerilogParser.AssignmentStatementContext context) {
        throw new NonReachableControlFlow("You can't assign variables inside constraint block");
    }

    public SvAstNode VisitConditionalPattern(SystemVerilogParser.ConditionalPatternContext context) {
        return context.cond_pattern().Accept(this);
    }

    public SvAstNode VisitExprOrCond(SystemVerilogParser.ExprOrCondContext context) {
        return context.expression().Accept(this);
    }

    public SvAstNode VisitMatchesPattern(SystemVerilogParser.MatchesPatternContext context) {
        throw new NonReachableControlFlow("Match is not supported");
    }

    public SvAstNode VisitPrimaryLiteral(SystemVerilogParser.PrimaryLiteralContext context) {
        return context.Accept(this);
    }

    public SvAstNode VisitHierarchicalIdentifier(SystemVerilogParser.HierarchicalIdentifierContext context) {
        return context.hierarchical_identifier().Accept(this);
    }

    public SvAstNode VisitHierarchicalId(SystemVerilogParser.HierarchicalIdContext context) {
        SvHierarchicalId hierarchicalId = new SvHierarchicalId();
        var id = context.ID().GetText();
        hierarchicalId.Add(id);
        return hierarchicalId;
    }

    public SvAstNode VisitNumberPrimaryLit(SystemVerilogParser.NumberPrimaryLitContext context) {
        SvNumLiteral numberLit = new SvNumLiteral();
        var num = context.NUMBER().GetText();
        numberLit.Number = num;
        return numberLit;
    }

    public SvAstNode VisitStringPrimaryLit(SystemVerilogParser.StringPrimaryLitContext context) {
        SvStringLiteral stringLit = new SvStringLiteral();
        var num = context.STRING_LITERAL().GetText();
        stringLit.StringLiteral = num;
        return stringLit;
    }

    public SvAstNode VisitAttributeInstance(SystemVerilogParser.AttributeInstanceContext context) {
        throw new NonReachableControlFlow("Attribute instance is not reachable.");
    }

    public SvAstNode VisitDotIdPattern(SystemVerilogParser.DotIdPatternContext context) {
        throw new NonReachableControlFlow("Dot ID is non reachable.");
    }

    public SvAstNode VisitDotStarPattern(SystemVerilogParser.DotStarPatternContext context) {
        throw new NonReachableControlFlow("DotStar is non reachable.");
    }

    public SvAstNode VisitValueRanges(SystemVerilogParser.ValueRangesContext context) {
        var ranges = context.value_range();
        SvOpenRange openRange = new SvOpenRange();
        foreach (var range in ranges) {
            var item = range.Accept(this);
            openRange.Add((SvValueRange)item);
        }

        return openRange;
    }

    public SvAstNode VisitExprValRange(SystemVerilogParser.ExprValRangeContext context) {
        var expr = (SvExpr)context.expression().Accept(this);
        SvValueRange valueRange = new SvValueRange(expr, null);
        return valueRange;
    }

    public SvAstNode VisitOpenRange(SystemVerilogParser.OpenRangeContext context) {
        var expressions = context.expression();
        var expr1 = (SvExpr)expressions[0].Accept(this);
        var expr2 = (SvExpr)expressions[1].Accept(this);
        SvValueRange valueRange = new SvValueRange(expr1, expr2);
        return valueRange;
    }

    public SvAstNode VisitLVariableId(SystemVerilogParser.LVariableIdContext context) {
        SvVariableLvalue varLvalue = new SvVariableLvalue();
        var id = context.ID().GetText();
        varLvalue.Id = id;
        return varLvalue;
    }

    public SvAstNode VisitDataTypeClassDecl(SystemVerilogParser.DataTypeClassDeclContext context) {
        var cstNode = MkAntlrCstRef.FromDataDecl(context);
        SvBitData bitData = (SvBitData)context.data_type().Accept(this);
        bitData.CstNode = cstNode;
        string? rand;
        try {
            rand = context.RAND().GetText();
        }
        catch (Exception e) {
            rand = null;
        }
        // Console.WriteLine($"rand: {rand}");
        string? randc;
        try {
            randc = context.RANDC().GetText();
        }
        catch (Exception e) {
            randc = null;
        }
        // Console.WriteLine($"randc: {randc}");

        if (rand != null) {
            bitData.Rand = SvBitData.Random.rand;
        }
        else if (randc != null) {
            bitData.Rand = SvBitData.Random.randc;
        }
        else {
            bitData.Rand = SvBitData.Random.notRand;
        }
        // Console.WriteLine($"random: {bitData.Rand}");
        return bitData;
    }

    public SvAstNode VisitBitDataType(SystemVerilogParser.BitDataTypeContext context) {
        var startIndex = uint.Parse(context.DECIMAL_NUMBER()[1].GetText());
        var endIndex = uint.Parse(context.DECIMAL_NUMBER()[0].GetText());
        SvBitData bitData = new SvBitData(startIndex, endIndex);
        return bitData;
    }
}
