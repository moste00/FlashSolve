lexer grammar SystemVerilogLexer ;

// Things to be added: more tokens, based numbers, real numbers and any tokens according to the standard of the language (IEEE)
// run the ANTLR tool to generate the lexer first before running the parser
//IMPORTANT: ALWAYS KEEP THIS ABOVE ANY_ASCII_CHARACTER
WS : [ \t\r\n]+ -> skip ;
COMMENTS : '//' ~[\r\n]* -> skip ;

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
OPEN_PAREN_STAR : '(*';
STAR_CLOSED_PAREN : '*)';
OPEN_SQUARE_BRACKET : '[';
CLOSED_SQUARE_BRACKET : ']';
QUESTION_MARK : '?';
DOT : '.'  ;
DOT_STAR : '.*' ;

// fragment means that the rule is not a token by itself, but is used in other tokens
// it's like using a variable to store a specific regex and use it in other regexes
// fragments are not visible in the parser file
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

fragment DECIMAL_NUMBER : UNSIGNED_NUMBER ; //TODO: add based numbers
fragment SIZE : NON_ZERO_UNSIGNED_NUMBER ;
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
PLUS : '+';
MINUS: '-';
//"exclusive" means the operators which can only occur as unary (this is false for e.g. "&")
UNARY_OPERATOR_EXLUSIVE : '!' | '~' | '~&' | '~|' ;
//BINARY_OPERATOR_n means the binary expression where the operator occurs has precedence n, where 1 is the highest precedence
BINARY_OPERATOR_1 : '**' ;
BINARY_OPERATOR_2 : '*' | '/' | '%' ; 
BINARY_OPERATOR_4 : '>>' | '<<' | '>>>' | '<<<' ;
BINARY_OPERATOR_5 : '<' | '<=' | '>' | '>=' ;
BINARY_OPERATOR_6 : '==' | '!=' | '===' | '!==' | '==?' | '!=?' ;
BINARY_OPERATOR_10 : '&&' ;
BINARY_OPERATOR_11 : '||' ;
BINARY_OPERATOR_12 : IMPLIES | '<->' ;
INC_OR_DEC_OPERATOR : '++' | '--' ;
ASSIGNMENT_OPERATOR : '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<=' | '>>=' | '<<<=' | '>>>=' ;
//IMPORTANT: ALWAYS MAKE THIS THE LAST RULE
ID : [a-zA-Z_][a-zA-Z_0-9$]* ;