parser grammar SystemVerilogParser ;

options {
            tokenVocab = SystemVerilogLexer;
        }

svprogram : class_decl+ EOF # Program
          ; 

class_decl : CLASS ID SEMICOLON
             class_item*
             ENDCLASS (COLON ID)? # ClassDecl
           ;

class_item : class_constraint # ClassConstraint
           | class_data_decl # ClassDataDecl
           ;

/* ********************************************************** Constraints ****************************************************************** */
class_constraint : constraint_prototype # ConstraintPrototype
                 | constraint_decl   # ConstraintDecl
                 ;

constraint_prototype : (EXTERN | PURE)? (STATIC)? CONSTRAINT ID # ConstraintPrototypeDecl
                     ;

constraint_decl : (STATIC)? CONSTRAINT ID constraint_block # ConstraintDeclBody
                ;

constraint_block : OPEN_CURLY_BRACE constraint_block_item* CLOSED_CURLY_BRACE # ConstraintBlock
                 ;

constraint_block_item : constraint_expr # ConstraintExpressionBlockItem
                      ;

constraint_expr : (SOFT)? expression_or_dist SEMICOLON # ExpressionOrDist
                | uniqueness_constraint  SEMICOLON # UniquenessConstraint
                | expression IMPLIES constraint_set  # ImpliesConstraint
                | IF OPEN_PAREN expression CLOSED_PAREN 
                  constraint_set
                  (ELSE constraint_set)?  # IfThenElseConstraint
                ;
uniqueness_constraint : UNIQUE OPEN_CURLY_BRACE open_range_list CLOSED_CURLY_BRACE # OpenRangeList
                      ;
constraint_set : constraint_expr # ConstraintExpressionSet
               | OPEN_CURLY_BRACE constraint_expr* CLOSED_CURLY_BRACE # ConstraintExpressionsSet
               ;
expression_or_dist :  expression # ExprOrDist
                   ;
expression : primary # PrimaryExpression
           | OPEN_PAREN operator_assignment CLOSED_PAREN # AssignmentOperator
           | UNARY_OPERATOR (attribute_instance)* primary # UnaryOperator
           | inc_or_dec_expression # IncOrDecExpression
           | expression BINARY_OPERATOR_1 (attribute_instance)* expression # PowerOperator
           | expression BINARY_OPERATOR_2 (attribute_instance)* expression # MulDivModOperator
           | expression BINARY_OPERATOR_3 (attribute_instance)* expression # AddSubOperator
           | expression BINARY_OPERATOR_4 (attribute_instance)* expression # ShiftOperators
           | expression BINARY_OPERATOR_5 (attribute_instance)* expression # ComparisonOperators
           | expression BINARY_OPERATOR_6 (attribute_instance)* expression # EqualityOperators
           | expression BINARY_OPERATOR_7 (attribute_instance)* expression # BitWiseAndOperator
           | expression BINARY_OPERATOR_8 (attribute_instance)* expression # BitWiseXorXnorOperator
           | expression BINARY_OPERATOR_9 (attribute_instance)* expression # BitWiseOrOperator
           | expression BINARY_OPERATOR_10 (attribute_instance)* expression # AndOperator
           | expression BINARY_OPERATOR_11 (attribute_instance)* expression # OrOperator
           | <assoc=right> expression ( MATCHES pattern )? ( TRIPLE_AMPERSAND expression_or_cond_pattern )* QUESTION_MARK (attribute_instance)* expression COLON expression # ConditionalExpression //conditional_expression
           | <assoc=right> expression INSIDE OPEN_CURLY_BRACE open_range_list CLOSED_CURLY_BRACE # InsideExpression //inside_expression 
           ;
inc_or_dec_expression : INC_OR_DEC_OPERATOR (attribute_instance)* variable_lvalue # Prefix
                      | variable_lvalue (attribute_instance)* INC_OR_DEC_OPERATOR # Postfix
                      ;
operator_assignment : variable_lvalue ASSIGNMENT_OPERATOR expression # AssignmentStatement
                    ;
expression_or_cond_pattern : cond_pattern # ConditionalPattern
                           | expression # ExprOrCond
                           ;
cond_pattern : expression MATCHES pattern # MatchesPattern
             ;
primary : primary_literal # PrimaryLiteral
        | hierarchical_identifier # HierarchicalIdentifier//TODO: Complete the rule as found in the standard
        ;
hierarchical_identifier : ID # HierarchicalId//TODO: Complete the rule as found in the standard 
                        ;
primary_literal : NUMBER # NumberPrimaryLit
                | STRING_LITERAL # StringPrimaryLit
                ;
//TODO: See if the supporting attr_spec rule in the formal grammar is relevant and deserve to be included here
attribute_instance : OPEN_PAREN_STAR ID ( COMMA ID)* STAR_CLOSED_PAREN # AttributeInstance
                   ;
pattern : DOT ID # DotIdPattern
        | DOT_STAR # DotStarPattern
        ;
open_range_list : value_range (COMMA value_range)*  # ValueRanges
                ;
value_range : expression # ExprValRange
            | OPEN_SQUARE_BRACKET expression COLON expression CLOSED_SQUARE_BRACKET # OpenRange
            ;
variable_lvalue : ID # LVariableId
                ;
/* ********************************************************** Data Declarations ****************************************************************** */
class_data_decl : (RAND | RANDC)? data_type ID SEMICOLON # DataTypeClassDecl
                ;
data_type : BIT  (OPEN_SQUARE_BRACKET DECIMAL_NUMBER COLON DECIMAL_NUMBER CLOSED_SQUARE_BRACKET)? # BitDataType
          ;