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

expression : ID 
           ;

open_range_list : ID *
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
//IMPORTANT: ALWAYS MAKE THIS THE LAST RULE
ID :  '[a-zA-Z_]'('[a-zA-Z_0-9$]')?;