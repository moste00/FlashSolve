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

constraint_block : CURLYOPEN constraint_block_item* CURLYCLOSE
                 ;

constraint_block_item : constraint_expr
                      ;

constraint_expr : (SOFT)? expression_or_dist   
                | uniqueness_constraint  
                | expression IMPLIES constraint_set  
                | IF PARENOPEN expression PARENCLOSE 
                  constraint_set 
                  (ELSE constraint_set)?
                ;

uniqueness_constraint : UNIQUE CURLYOPEN open_range_list CURLYCLOSE 
                      ;

constraint_set : constraint_expr 
               | CURLYOPEN constraint_expr* CURLYCLOSE 
               ;

expression_or_dist :  expression 
                   ;

expression : primary | UNARYOPERATORS  (attribute_instance)* primary| inside_expression | INCDECOPERATOR 
           | operator_assignment | expression BINARYOPERATOR (attribute_instance)* expression
           | conditional_expression| ID  
           ;
primary : primary_literal;
primary_literal : number |STINGLITRAL ;
inside_expression : expression 'inside' '{' open_range_list '}';
operator_assignment : variable_lvalue assignment_operator expression ;
conditional_expression : cond_predicate '?' (attribute_instance)* expression ':' expression ;
number : integral_number | NUMDECIMAL ;
integral_number : octal_number | binary_number | hex_number ;
binary_number :  'size'?  BINARYBASE binary_value ;
octal_number : 'size'? OCTALBASE octal_value ;
hex_number : 'size'? HEXBASE hex_value ;
binary_value : BINARYDIGIT ('_' | BINARYDIGIT )*;
octal_value : OCTALDIGIT ('_' | OCTALDIGIT )*;
hex_value : HEXDIGIT ('_'| HEXDIGIT )*;
attribute_instance : '(*' ID ( ',' ID )* '*)';
//attr_spec ::= attr_name [ = constant_expression ] 
//attr_name ::= identifier
cond_predicate : expression_or_cond_pattern ('&&&' expression_or_cond_pattern)*;
expression_or_cond_pattern : cond_pattern | expression ;
cond_pattern : expression 'matches' pattern ;
pattern : '.' ID | '.*';
open_range_list : ID *
                ;
variable_lvalue : ID
                ;
/* ********************************************************** Data Declarations ****************************************************************** */
class_data_decl : (RAND | RANDC)? data_type ID 
                ;
data_type : BIT  (SEQBRACKETOPEN NUMDECIMAL COLON NUMDECIMAL SEQBRACKETCLOSE)?
          ;
/* ********************************************************** Terminals  ******************************************************************* */
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
SEMICOLON : ';';
COLON : ':';
CURLYOPEN : '{';
CURLYCLOSE : '}';
PARENOPEN : '(';
PARENCLOSE : ')';
SEQBRACKETOPEN : '[';
SEQBRACKETCLOSE : ']';
IMPLIES : '->';
NUMDECIMAL : '[0-9]+';
BINARYBASE : '\'(\S|\s)?b|\'(\S|\s)?B';
OCTALBASE : '\'(\S|\s)?o|\'(\S|\s)?O';
HEXBASE : '\'(\S|\s)?h|\'(\S|\s)?H';
BINARYDIGIT : '[01xzXZ]';
OCTALDIGIT : '[0-7xzXZ]';
HEXDIGIT : '[0-9a-fA-FxzXZ]';
STINGLITRAL : '\S*';
UNARYOPERATORS : '+' | '-' | '!' | '~' | '&' | '~&' | '|' | '~|' | '^' | '~^' | '^~';
BINARYOPERATOR : '+' | '-' | '*' | '/' | '%' | '==' | '!=' | '===' | '!==' | '==?' | '!=?' | '&&' | '||' | '**'
| '<' | '<=' | '>' | '>=' | '&' | '|' | '^' | '^~' | '~^' | '>>' | '<<' | '>>>' | '<<<'
| '->' | '<->';
INCDECOPERATOR : '++' | '--';
assignment_operator : '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<=' | '>>=' | '<<<=' | '>>>=';
//unary_module_path_operator : ! | ~ | & | ~& | | | ~| | ^ | ~^ | ^~;
//binary_module_path_operator : == | != | && | || | & | | | ^ | ^~ | ~^;
//IMPORTANT: ALWAYS MAKE THIS THE LAST RULE
ID :  '[a-zA-Z_]'('[a-zA-Z_0-9$]')?;