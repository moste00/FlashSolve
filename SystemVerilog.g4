grammar SystemVerilog ;

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
attribute_instance : '(*' ID ( ',' ID )* '*)'
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
/* ********************************************************** Terminals  ************************************************************************* */

CLASS : 'class';
ENDCLASS : 'endclass';
CONSTRAINT : 'constraint';
SOFT : 'soft';
IF   : 'if';
ELSE : 'else';
UNIQUE : 'unique';
BIT : 'bit';
EXTERN : 'extern';
PURE : 'pure';
STATIC : 'static';
RAND : 'rand';
RANDC : 'randc';
INSIDE : 'inside' ;
MATCHES : 'matches' ; 

SEMICOLON : ';' ;
COLON : ':' ;
COMMA : ',' ;
OPEN_CURLY_BRACE : '{';
CLOSED_CURLY_BRACE : '}';
OPEN_PAREN : '(';
CLOSED_PAREN : ')';
OPEN_SQUARE_BRACKET : '[';
CLOSED_SQUARE_BRACKET : ']';
QUESTION_MARK : '?';
DOT : '.'  ;
DOT_STAR : '.*' ;

fragment DECIMAL_DIGIT : [0-9] ;
fragment BINARY_DIGIT : [01xzXZ] ;
fragment OCTAL_DIGIT : [0-7xzXZ] ;
fragment HEX_DIGIT : [0-9a-fA-FxzXZ] ;
fragment UNSIGNED_NUMBER : DECIMAL_DIGIT ('_'| DECIMAL_DIGIT )* ;
fragment BINARY_VALUE : BINARY_DIGIT ('_' | BINARY_DIGIT)* ;
fragment OCTAL_VALUE  : OCTAL_DIGIT ('_' | OCTAL_DIGIT)* ;
fragment HEX_VALUE : HEX_DIGIT ('_'| HEX_DIGIT )* ;
fragment BASE_SIGN_PREFIX : '\'' ('s'|'S')? ;
fragment BINARY_BASE : BASE_SIGN_PREFIX ('b'|'B') ;
fragment OCTAL_BASE :  BASE_SIGN_PREFIX ('o'|'O') ;
fragment HEX_BASE :  BASE_SIGN_PREFIX ('h'|'H') ;
fragment DECIMAL_BASE : BASE_SIGN_PREFIX ('d'|'D') ;
fragment NON_ZERO_UNSIGNED_NUMBER : [1-9]('_' | DECIMAL_DIGIT)* ;

DECIMAL_NUMBER : UNSIGNED_NUMBER ; //TODO: add based numbers
SIZE : NON_ZERO_UNSIGNED_NUMBER ;
fragment BINARY_NUMBER :  SIZE?  BINARY_BASE BINARY_VALUE ;
fragment OCTAL_NUMBER : SIZE? OCTAL_BASE OCTAL_VALUE ;
fragment HEX_NUMBER : SIZE? HEX_BASE HEX_VALUE ;


fragment INTEGRAL_NUMBER : OCTAL_NUMBER | BINARY_NUMBER | HEX_NUMBER | DECIMAL_NUMBER ;
NUMBER : INTEGRAL_NUMBER ; //TODO: | REAL_NUMBER

fragment ANY_ASCII_CHARACTER : [\u0000-\u007f] ;
STRING_LITERAL : '"'ANY_ASCII_CHARACTER*'"' ;

IMPLIES : '->';
TRIPLE_AMPERSAND : '&&&' ;
BITWISE_AND : '&' ;
BITWISE_OR  : '|' ;
BITWISE_XOR_XNOR :  '^' | '^~' | '~^' ;
UNARY_OPERATOR : '+' | '-' | '!' | '~' | BITWISE_AND | '~&' | BITWISE_OR | '~|' | BITWISE_XOR_XNOR ;
//BINARY_OPERATOR_n means the binary expression where the operator occurs has precedence n, where 1 is the highest precedence
BINARY_OPERATOR_1 : '**' ;
BINARY_OPERATOR_2 : '*' | '/' | '%' ; 
BINARY_OPERATOR_3 : '+' | '-' ;
BINARY_OPERATOR_4 : '>>' | '<<' | '>>>' | '<<<' ;
BINARY_OPERATOR_5 : '<' | '<=' | '>' | '>=' ;
BINARY_OPERATOR_6 : '==' | '!=' | '===' | '!==' | '==?' | '!=?' ;
BINARY_OPERATOR_7 : BITWISE_AND ;
BINARY_OPERATOR_8 : BITWISE_XOR_XNOR ;
BINARY_OPERATOR_9 : BITWISE_OR ;
BINARY_OPERATOR_10 : '&&' ;
BINARY_OPERATOR_11 : '||' ;
BINARY_OPERATOR_12 : IMPLIES | '<->' ;
INC_OR_DEC_OPERATOR : '++' | '--' ;
ASSIGNMENT_OPERATOR : '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<=' | '>>=' | '<<<=' | '>>>=' ;

WS : [ \t\r\n]+ -> skip ;
COMMENTS : '//' ~[\r\n]* -> skip ;
//IMPORTANT: ALWAYS MAKE THIS COME AFTER ALL KEYWORDS
ID : [a-zA-Z_][a-zA-Z_0-9$]* ;