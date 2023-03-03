parser grammar SystemVerilogParser ;

options {
            tokenVocab = SystemVerilogLexer;
        }

svprogram : class_decl+ EOF 
          ; 

class_decl : CLASS ID SEMICOLON 
             class_item* 
             ENDCLASS (COLON ID)?
           ;

class_item : class_constraint 
           | class_data_decl 
           ;

/* ********************************************************** Constraints ****************************************************************** */
class_constraint : constraint_prototype 
                 | constraint_decl   
                 ;

constraint_prototype : (EXTERN | PURE)? (STATIC)? CONSTRAINT ID 
                     ;

constraint_decl : (STATIC)? CONSTRAINT ID constraint_block 
                ;

constraint_block : OPEN_CURLY_BRACE constraint_block_item* CLOSED_CURLY_BRACE
                 ;

constraint_block_item : constraint_expr
                      ;

constraint_expr : (SOFT)? expression_or_dist SEMICOLON
                | uniqueness_constraint  SEMICOLON
                | expression IMPLIES constraint_set  
                | IF OPEN_PAREN expression CLOSED_PAREN 
                  constraint_set 
                  (ELSE constraint_set)?
                ;
uniqueness_constraint : UNIQUE OPEN_CURLY_BRACE open_range_list CLOSED_CURLY_BRACE 
                      ;
constraint_set : constraint_expr 
               | OPEN_CURLY_BRACE constraint_expr* CLOSED_CURLY_BRACE 
               ;
expression_or_dist :  expression 
                   ;
expression : primary
           | OPEN_PAREN operator_assignment CLOSED_PAREN 
           | UNARY_OPERATOR (attribute_instance)* primary
           | inc_or_dec_expression 
           | expression BINARY_OPERATOR_1 (attribute_instance)* expression
           | expression BINARY_OPERATOR_2 (attribute_instance)* expression
           | expression BINARY_OPERATOR_3 (attribute_instance)* expression
           | expression BINARY_OPERATOR_4 (attribute_instance)* expression
           | expression BINARY_OPERATOR_5 (attribute_instance)* expression
           | expression BINARY_OPERATOR_6 (attribute_instance)* expression
           | expression BINARY_OPERATOR_7 (attribute_instance)* expression
           | expression BINARY_OPERATOR_8 (attribute_instance)* expression
           | expression BINARY_OPERATOR_9 (attribute_instance)* expression
           | expression BINARY_OPERATOR_10 (attribute_instance)* expression
           | expression BINARY_OPERATOR_11 (attribute_instance)* expression
           | <assoc=right> expression ( MATCHES pattern )? ( TRIPLE_AMPERSAND expression_or_cond_pattern )* QUESTION_MARK (attribute_instance)* expression COLON expression //conditional_expression
           | <assoc=right> expression INSIDE OPEN_CURLY_BRACE open_range_list CLOSED_CURLY_BRACE //inside_expression 
           ;
inc_or_dec_expression : INC_OR_DEC_OPERATOR (attribute_instance)* variable_lvalue 
                      | variable_lvalue (attribute_instance)* INC_OR_DEC_OPERATOR 
                      ;
operator_assignment : variable_lvalue ASSIGNMENT_OPERATOR expression 
                    ;
expression_or_cond_pattern : cond_pattern | expression ;
cond_pattern : expression MATCHES pattern 
             ;
primary : primary_literal 
        | hierarchical_identifier //TODO: Complete the rule as found in the standard
        ;
hierarchical_identifier : ID //TODO: Complete the rule as found in the standard 
                        ;
primary_literal : NUMBER | STRING_LITERAL 
                ;
//TODO: See if the supporting attr_spec rule in the formal grammar is relevant and deserve to be included here
attribute_instance : OPEN_PAREN_STAR ID ( COMMA ID)* STAR_CLOSED_PAREN
                   ;
pattern : DOT ID 
        | DOT_STAR
        ;
open_range_list : value_range (COMMA value_range)*  
                ;
value_range : expression
            | OPEN_SQUARE_BRACKET expression COLON expression CLOSED_SQUARE_BRACKET
            ;
variable_lvalue : ID
                ;
/* ********************************************************** Data Declarations ****************************************************************** */
class_data_decl : (RAND | RANDC)? data_type ID SEMICOLON
                ;
data_type : BIT  (OPEN_SQUARE_BRACKET DECIMAL_NUMBER COLON DECIMAL_NUMBER CLOSED_SQUARE_BRACKET)?
          ;