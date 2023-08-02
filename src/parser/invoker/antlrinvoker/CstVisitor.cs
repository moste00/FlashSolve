using Antlr4.Runtime.Tree;
using flashsolve.parser.ast;

namespace flashsolve.parser.invoker.antlrinvoker;

// the following classes are extending the exception class to be able to throw them in the visitor
// in clear way to the user

// this exception is thrown when the visitor is trying to visit a node that is not reachable
public class NonReachableControlFlow : Exception {
    public NonReachableControlFlow() {}
    public NonReachableControlFlow(string msg) : base(msg) {}
    public NonReachableControlFlow(string msg,Exception inner) : base(msg,inner) {}
}

// this exception is thrown when the visitor is trying to visit a node that is not supported yet(it's code not implemented yet)
public class UnsupportedSvSubset : Exception {
    public UnsupportedSvSubset() {}
    public UnsupportedSvSubset(string msg) : base(msg) {}
    public UnsupportedSvSubset(string msg,Exception inner) : base(msg,inner) {}
}

// this exception is thrown when the visitor is trying to visit a node that is supported in the grammar but it has no meaning like assignment inside constraint block
public class IllegalExpression : Exception {
    public IllegalExpression() {}
    public IllegalExpression(string msg) : base(msg) {}
    public IllegalExpression(string msg,Exception inner) : base(msg,inner) {}
}

// this class is used to create a the AST
public class CstVisitor : ISystemVerilogParserVisitor<SvAstNode> {
    // this method is to visit the CST 
    public SvAstNode Visit(IParseTree tree) {
        return tree.Accept(this);
    }
    // this method is to visit a node that it's not implemented yet
    public SvAstNode VisitChildren(IRuleNode node) {
        throw new NonReachableControlFlow("Method VisitChildren is a generic hook that is not needed and should never be called, you probably made a mistake somewhere.");
    }
    // this method is to visit a EOF node
    public SvAstNode VisitTerminal(ITerminalNode node) {
        Console.WriteLine("I'm visiting a terminal ! its text is "+ node.GetText());
        return null;
    }

    public SvAstNode VisitErrorNode(IErrorNode node) {
        return null;
    }

    // starting from here are the methods that are used to visit the CST nodes that are needed to create the AST
    // the methods are named after the name of the node in the CST, which came from naming the alternatives in the grammar not the Non-terminals itself
    // the creation of a node depends on the AST node that is needed to be created, and the AST node depends heavily on the grammar
    // this method is to visit a program which is the root of the CST
    public SvAstNode VisitProgram(SystemVerilogParser.ProgramContext context) {
        // the program is a list of class declarations, so get them all
        var decls = context.class_decl();
        // create a new AST program node
        var svProg = new SvConstraintProgram();
        // visit all the class declarations and add them to the AST program node
        foreach (var classDeclaration in decls) {
            var svClass = classDeclaration.Accept(this)!;
            svProg.Add((SvClass)svClass);
        }

        return svProg;
    }

    public SvAstNode VisitClassDecl(SystemVerilogParser.ClassDeclContext context) {
        // create a new AST class node, and give it the CST node as a reference which is just its name
        // maybe change in the future to be a more representative form
        var cls = new SvClass(
            MkAntlrCstRef.FromClassDecl(context)
        );
        // get all the items in the class
        var items = context.class_item();
        // visit all the items and add them to the AST class node
        foreach (var item in items) {
            var itemAst = item.Accept(this);
            switch (itemAst) {
                // if the item is a constraint, add it to the class constraints
                case SvConstraint constraint:
                    cls.Add(constraint);
                    break;
                // if the item is a data definition, add it to the class data definitions
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
        // if you found this in any methods, thats mean that the cst node doesn't have a valuable information
        // and we should just ignore it and visit it to get its children
        // and also its children may have valuable information or not
        // what determines if the node has valuable information or not is you. 
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
        // create a new AST constraint node, and give it the CST node as a reference which is just its name
        return new SvConstraint(
            MkAntlrCstRef.FromContraintPrototype(context)
        );
    }

    public SvAstNode VisitConstraintDeclBody(SystemVerilogParser.ConstraintDeclBodyContext context) {
        // create a new AST constraint node, and give it the CST node as a reference which is just its name
        var constr = new SvConstraint(
            MkAntlrCstRef.FromConstraintDecl(context)
        ) {
            // get the constraint block and visit it
            Items = (SvConstraint.Block) context.constraint_block().Accept(this)
        };
        return constr;
    }

    public SvAstNode VisitConstraintBlock(SystemVerilogParser.ConstraintBlockContext context) {
        // create a new AST constraint block node
        var items = new List<SvConstraint.BlockItem>();
        // get all the items in the constraint block
        foreach (var block_item in context.constraint_block_item()) {
            // visit all the items and add them to the AST constraint block node
            items.Add((SvConstraint.BlockItem) block_item.Accept(this));
        }

        return new SvConstraint.Block(items);
    }

    public SvAstNode VisitConstraintExpressionBlockItem(SystemVerilogParser.ConstraintExpressionBlockItemContext context) {
        return context.constraint_expr().Accept(this);
    }

    public SvAstNode VisitExpressionOrDist(SystemVerilogParser.ExpressionOrDistContext context) {
        // future work: support soft constraints
        if (context.SOFT() != null) {
            throw new UnsupportedSvSubset("Soft constraints unsupported yet");
        }
        return context.expression_or_dist().Accept(this);
    }

    public SvAstNode VisitUniquenessConstraint(SystemVerilogParser.UniquenessConstraintContext context) {
        return context.uniqueness_constraint().Accept(this);
    }

    public SvAstNode VisitImpliesConstraint(SystemVerilogParser.ImpliesConstraintContext context) {
        // get the expression from implication constraint and visit it
        var expr = context.expression().Accept(this);
        // create a new AST implication node, and give the constructor the expression
        SvImplication svImplication = new((SvExpr)expr);
        // get the constraint set from implication constraint and visit it
        var constraintSet = context.constraint_set().Accept(this);
        // set the constraint set of the implication node
        svImplication.ConstraintSet = (SvConstraintSet)constraintSet;
        return svImplication;
    }

    public SvAstNode VisitIfThenElseConstraint(SystemVerilogParser.IfThenElseConstraintContext context) {
        // get the expression from if-then-else constraint and visit it
        var cond = (SvExpr)context.expression().Accept(this);
        // get the constraint set from if-then-else constraint
        var thenAndElse = context.constraint_set();
        // the first constraint set is the then constraint set
        var thenCst = thenAndElse[0];
        // visit the then constraint set
        var then = (SvConstraintSet)thenCst.Accept(this);
        
        // create a new AST if-then-else node, and give the constructor the expression and the then constraint set
        var ifThenElse = new SvIfElse(cond) {
            Then = then
        };

        // check whether there is an else constraint set or not
        if (thenAndElse.Length > 1) {
            // if there is an else constraint set, get it , it is the second constraint set and visit it
            var els = (SvConstraintSet)thenAndElse[1].Accept(this);
            ifThenElse.Else = els;
        }
        return ifThenElse;
    }

    public SvAstNode VisitOpenRangeList(SystemVerilogParser.OpenRangeListContext context) {
        // create a new AST uniqueness node
        return new SvUniqueness(
            (SvOpenRange)context.open_range_list().Accept(this)
        );
    }

    public SvAstNode VisitConstraintExpressionSet(SystemVerilogParser.ConstraintExpressionSetContext context)
    {
        // create a new AST constraint set node
        var constraintSet = new SvConstraintSet
        {
            // get the constraint expression from the rule and visit it
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
        // get the left expression from the rule and visit it
        var left = (SvExpr)context.expression()[0].Accept(this);
        // get the right expression from the rule and visit it
        var right = (SvExpr)context.expression()[1].Accept(this);
        
        var op = SvBinaryExpression.Op.BitwiseOr;
        // create a new AST binary expression node, and give the constructor the left and right expressions and the operator
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
        // check which operator is it in the sv file
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

    // this method is to visit a constraint expression which is like ternary operator
    public SvAstNode VisitConditionalExpression(SystemVerilogParser.ConditionalExpressionContext context) {
       // get the expressions from conditional expression
        var exprs = context.expression();
        // get the then expression from conditional expression and visit it
        var then = (SvExpr)exprs.Last().Accept(this);
        // get the else expression from conditional expression and visit it
        var els = (SvExpr)exprs[exprs.Length - 2].Accept(this);
        // create a new AST conditional expression node
        SvConditionalExpression condExpr = new SvConditionalExpression(then, els);
        // get the condition expression from conditional expression and visit it
        // then add them to the AST conditional expression node
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
        string op = "";
        // check which operator is it in the sv file first
        if (context.PLUS() != null) {
            op = context.PLUS().GetText();
        }
        else if (context.MINUS() != null) {
            op = context.MINUS().GetText();
        }
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
        string op = "";
        if (context.BITWISE_XOR_XNOR() != null) {
            op = context.BITWISE_XOR_XNOR().GetText();
        }
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
        string op = "";
        if (context.PLUS() != null) {
            op = context.PLUS().GetText();
        }
        else if (context.MINUS() != null) {
            op = context.MINUS().GetText();
        }
        else if (context.BITWISE_AND() != null) {
            op = context.BITWISE_AND().GetText();
        }
        else if (context.BITWISE_OR() != null) {
            op = context.BITWISE_OR().GetText();
        }
        else if (context.BITWISE_XOR_XNOR() != null) {
            op = context.BITWISE_XOR_XNOR().GetText();
        }
        else if (context.UNARY_OPERATOR_EXLUSIVE() != null) {
            op = context.UNARY_OPERATOR_EXLUSIVE().GetText();
        }
        // get the primary from unary expression and visit it
        var primary = (SvPrimary)context.primary().Accept(this);
        SvUnaryExpression.UnaryOP finalOp;
        switch (op) {
            case "+":
                finalOp = SvUnaryExpression.UnaryOP.Plus;
                break;
            case "-":
                finalOp = SvUnaryExpression.UnaryOP.Minus;
                break;
            case "!":
                finalOp = SvUnaryExpression.UnaryOP.Negation;
                break;
            case "~":
                finalOp = SvUnaryExpression.UnaryOP.Complement;
                break;
            case "&":
                finalOp = SvUnaryExpression.UnaryOP.BitwiseAnd;
                break;
            case "|":
                finalOp = SvUnaryExpression.UnaryOP.BitwiseOr;
                break;
            case "~&":
                finalOp = SvUnaryExpression.UnaryOP.BitwiseNand;
                break;
            case "~|":
                finalOp = SvUnaryExpression.UnaryOP.BitwiseNor;
                break;
            case "~^":
                finalOp = SvUnaryExpression.UnaryOP.Xnor;
                break;
            case "^~":
                finalOp = SvUnaryExpression.UnaryOP.Xor;
                break;
            default:
                throw new NonReachableControlFlow($"This operator is unrecognized (operator {op})");
        }
        // create a new AST unary expression node, and give the constructor the primary and the operator
        SvUnaryExpression unaryExpression = new SvUnaryExpression(primary, finalOp);
        return unaryExpression;
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
        // get the expression from inside expression and visit it
        var expr = (SvExpr)context.expression().Accept(this);
        // create a new AST inside expression node, and give the constructor the expression
        SvInsideExpression insideExpression = new SvInsideExpression(expr);
        // get the open range from inside expression and visit it
        var openRange = (SvOpenRange)context.open_range_list().Accept(this);
        // set the open range of the inside expression node
        insideExpression.OpenRange = openRange;
        return insideExpression;
    }

    public SvAstNode VisitPrefix(SystemVerilogParser.PrefixContext context) {
        // get the variable id from prefix and visit it
        var variableId = (SvVariableLvalue)context.variable_lvalue().Accept(this);
        // create a new AST inc or dec expression node, and give the constructor the variable id and the operator
        // true means that it is a prefix
        SvIncOrDecExpr incOrDecExpr = new SvIncOrDecExpr(variableId, true);
        return incOrDecExpr;
    }

    public SvAstNode VisitPostfix(SystemVerilogParser.PostfixContext context) {
        // get the variable id from postfix and visit it
        var variableId = (SvVariableLvalue)context.variable_lvalue().Accept(this);
        
        // create a new AST inc or dec expression node, and give the constructor the variable id and the operator
        // false means that it is a postfix
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
        return context.primary_literal().Accept(this);
    }

    public SvAstNode VisitHierarchicalIdentifier(SystemVerilogParser.HierarchicalIdentifierContext context) {
        return context.hierarchical_identifier().Accept(this);
    }

    public SvAstNode VisitHierarchicalId(SystemVerilogParser.HierarchicalIdContext context) {
        // create a new AST hierarchical id node
        SvHierarchicalId hierarchicalId = new SvHierarchicalId();
        // get all the ids in the hierarchical id 
        // it's 1 for now but it may change in the future to be more than 1 and it should be more than 1
        // we took into consideration that it may be more than 1, but there is no time to implement it
        var id = context.ID().GetText();
        // add the id to the AST hierarchical id node
        hierarchicalId.Add(id);
        return hierarchicalId;
    }

    public SvAstNode VisitNumberPrimaryLit(SystemVerilogParser.NumberPrimaryLitContext context) {
        // create a new AST number literal node
        SvNumLiteral numberLit = new SvNumLiteral();
        // get the number from the rule and add it to the AST number literal node
        var num = context.NUMBER().GetText();
        // set the number of the AST number literal node
        numberLit.Number = num;
        return numberLit;
    }

    public SvAstNode VisitStringPrimaryLit(SystemVerilogParser.StringPrimaryLitContext context) {
        // create a new AST string literal node
        SvStringLiteral stringLit = new SvStringLiteral();
        // get the string from the rule and add it to the AST string literal node
        var str = context.STRING_LITERAL().GetText();
        // set the string of the AST string literal node
        stringLit.StringLiteral = str;
        return stringLit;
    }

    public SvAstNode VisitAttributeInstance(SystemVerilogParser.AttributeInstanceContext context) {
        // should be implemented in the future
        throw new NonReachableControlFlow("Attribute instance is not reachable.");
    }

    public SvAstNode VisitDotIdPattern(SystemVerilogParser.DotIdPatternContext context) {
        // should be implemented in the future
        throw new NonReachableControlFlow("Dot ID is non reachable.");
    }

    public SvAstNode VisitDotStarPattern(SystemVerilogParser.DotStarPatternContext context) {
        // should be implemented in the future
        throw new NonReachableControlFlow("DotStar is non reachable.");
    }

    public SvAstNode VisitValueRanges(SystemVerilogParser.ValueRangesContext context) {
        // get the ranges
        var ranges = context.value_range();
        // create new AST open range node
        SvOpenRange openRange = new SvOpenRange();
        // loop over all ranges
        foreach (var range in ranges) {
            // visit them
            var item = range.Accept(this);
            // add them to openRange list 
            openRange.Add((SvValueRange)item);
        }

        return openRange;
    }

    public SvAstNode VisitExprValRange(SystemVerilogParser.ExprValRangeContext context) {
        // get the expression from expr val range rule 
        var expr = (SvExpr)context.expression().Accept(this);
        // create new AST value range node, and give its constructor the expression and null which means that we have only one expression
        SvValueRange valueRange = new SvValueRange(expr, null);
        return valueRange;
    }

    public SvAstNode VisitOpenRange(SystemVerilogParser.OpenRangeContext context) {
        // get the expression from the rule
        var expressions = context.expression();
        // visit them both
        var expr1 = (SvExpr)expressions[0].Accept(this);
        var expr2 = (SvExpr)expressions[1].Accept(this);
        //  // create new AST value range node, and give its constructor the expressions which means that we have 2 expression
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
        // create a cst ref node to store the name of the register from sv file
        var cstNode = MkAntlrCstRef.FromDataDecl(context);
        // get the bit data from the rule and visit it
        SvBitData bitData = (SvBitData)context.data_type().Accept(this);
        // set the cst node to the cst ref node
        bitData.CstNode = cstNode;
        string? rand;
        // check if the register is rand or randc or none (not random at all)
        try {
            rand = context.RAND().GetText();
        }
        catch (Exception e) {
            rand = null;
        }
        string? randc;
        try {
            randc = context.RANDC().GetText();
        }
        catch (Exception e) {
            randc = null;
        }

        if (rand != null) {
            bitData.Rand = SvBitData.Random.rand;
        }
        else if (randc != null) {
            bitData.Rand = SvBitData.Random.randc;
        }
        else {
            bitData.Rand = SvBitData.Random.notRand;
        }
        return bitData;
    }

    public SvAstNode VisitBitDataType(SystemVerilogParser.BitDataTypeContext context) {
        // get the start and end indecies of the register
        var startIndex = uint.Parse(context.NUMBER()[1].GetText());
        var endIndex = uint.Parse(context.NUMBER()[0].GetText());
        // create a AST bit data node , and give its constructor the start and end indecies
        SvBitData bitData = new SvBitData(startIndex, endIndex);
        return bitData;
    }
}
