grammar Tiger;

options
{
	language = CSharp3;
	output = AST;
}

tokens
{
	// Words
	ARRAY = 'array';
	BREAK = 'break';
	DO = 'do';
	ELSE = 'else';
	END = 'end';
	FOR = 'for';
	FUNCTION = 'function';
	IF = 'if';
	IN = 'in';
	LET = 'let';
	NIL = 'nil';
	OF = 'of';
	THEN = 'then';
	TO = 'to';
	TYPE = 'type';
	VAR = 'var';
	WHILE = 'while';
	
	// Symbols
	COMMA = ',';
	COLON = ':';
	SEMICOLON = ';';
	LPARENTHESIS = '(';
	RPARENTHESIS = ')';
	LBRACKET = '[';
	RBRACKET = ']';
	LBRACE = '{';
	RBRACE = '}';
	DOT = '.';
	PLUS = '+';
	MINUS = '-';
	MULT = '*';
	DIV = '/';
	EQUAL = '=';
	NOT_EQUAL = '<>';
	LESS_THAN = '<';
	LESS_EQUAL_THAN = '<=';
	GREATER_THAN = '>';
	GREATER_EQUAL_THAN = '>=';
	AND = '&';
	OR = '|';
	ASSIGN = ':=';
	
	// Abstract
	EXPRESSION;
	NEGATE;
	LVALUE;
	EXPRESSION_SEQ;
	RECORD;
	ARRAY_INDEX;
	IF_THEN_ELSE;
	IF_THEN;
	FUNCTION_DECLARATION;
	RETURN_TYPE;
	TYPE_DECLARATION;
	VAR_DECLARATION;	
	RECORD_TYPE_DECLARATION;
	ALIAS_TYPE_DECLARATION;
	ARRAY_TYPE_DECLARATION;
	TYPE_DECLARATION_FIELD;	
}

// ------- LEXER ADD-INS: -----------------------------------------

@lexer::header
{
using System;
using TigerCompiler.ErrorHandling;
}
@lexer::members
{
public override void ReportError(RecognitionException exc) {
	Errors.AddSintacticError(exc, TokenNames);
}
}

// ------- PARSER ADD-INS: ----------------------------------------

@parser::header
{
using System;
using TigerCompiler.ErrorHandling;
}

@parser::members 
{
public override void ReportError(RecognitionException exc) { 
	Errors.AddSintacticError(exc, TokenNames);
} 
}

// LEXER RULES
fragment
DIGIT	:	'0'..'9'
	;
fragment
LETTER	:	'a'..'z' | 'A'..'Z'
	;
WS	:	(' ' |	'\t' | '\r' | '\n') { $channel=Hidden; }
	;
IDENTIFIER	:	LETTER (LETTER | DIGIT | '_')* 
	;
INT	:	DIGIT+
    	;
ESC_SEQ
	:	'\\' ( 't' | 'n' | 'r' | '\"' | '\\' | DIGIT DIGIT DIGIT | WS+ '\\' )
    	;
COMMENT	:	'/*' (options {greedy=false;} : COMMENT | .)* '*/' { $channel = Hidden; }
	;
STRING	:	'"' (ESC_SEQ | ~('\\'|'"'))* '"'
    	;

// PARSER RULES

public program 
	:	expr EOF -> ^(EXPRESSION expr)
	;
expr	:	conjunction_term ((OR^|AND^) conjunction_term)*
	;
conjunction_term
	:	comparison_term ((EQUAL^|NOT_EQUAL^|LESS_THAN^|GREATER_THAN^|LESS_EQUAL_THAN^|GREATER_EQUAL_THAN^) comparison_term)*
	;
comparison_term
	:	term ((PLUS^|MINUS^) term)*
	;
term	:	(atom | atom DIV) => atom ((MULT^ | DIV^) atom)*
	;
atom	:	flow_instructions
	|	(lvalue ASSIGN) => lvalue ASSIGN expr -> ^(ASSIGN lvalue expr)
	|	(type_id LBRACE) => type_id LBRACE field_list? RBRACE -> ^(RECORD[((CommonTree)$type_id.tree).Token, "RECORD"] type_id field_list?)
	|	(type_id LBRACKET expr RBRACKET OF) => type_id LBRACKET a=expr RBRACKET OF b=expr -> ^(ARRAY[((CommonTree)$type_id.tree).Token, "ARRAY"] type_id $a $b)
	|	(IDENTIFIER LPARENTHESIS) => IDENTIFIER LPARENTHESIS expr_list? RPARENTHESIS -> ^(FUNCTION[$IDENTIFIER, "FUNCTION"] IDENTIFIER expr_list?)
	|	(MINUS INT) => MINUS INT -> ^(NEGATE[$MINUS, "NEGATE"] INT)
	|	LPARENTHESIS expr_seq? RPARENTHESIS -> ^(EXPRESSION_SEQ[$LPARENTHESIS, "EXPRESSION_SEQ"] expr_seq?)
	|	INT
	|	NIL
	|	STRING
	|	BREAK
	|	MINUS lvalue -> ^(NEGATE[$MINUS, "NEGATE"] lvalue)
	|	lvalue
	;
extensions
	:	DOT IDENTIFIER
	|	LBRACKET expr RBRACKET -> ARRAY_INDEX[$LBRACKET, "ARRAY_INDEX"] expr
	;
lvalue	:	IDENTIFIER (others+=extensions)* -> ^(LVALUE[$IDENTIFIER, "LVALUE"] IDENTIFIER $others*)
	;
flow_instructions
	:	FOR IDENTIFIER ASSIGN a=expr TO b=expr DO c=expr -> ^(FOR IDENTIFIER $a $b $c)
	|	LET declaration_list_wrapper IN expr_seq? END -> ^(LET declaration_list_wrapper ^(EXPRESSION_SEQ[$IN, "EXPRESSION_SEQ"] expr_seq?))
	|	WHILE^ expr DO! expr
	|	(IF expr THEN expr ELSE) => IF a=expr THEN b=expr ELSE c=expr -> ^(IF_THEN_ELSE[$IF, "IF_THEN_ELSE"] $a $b $c)
	|	IF a=expr THEN b=expr -> ^(IF_THEN[$IF, "IF_THEN"] $a $b)
	;
field_list
	:	IDENTIFIER EQUAL a+=expr (COMMA IDENTIFIER EQUAL a+=expr)* -> (IDENTIFIER $a)+
	;
expr_list
	:	expr (COMMA! expr)*
	;
expr_seq:	expr (SEMICOLON! expr)*
	;
	
	
declaration_list_wrapper
	:	declaration_list+
	;
declaration_list
	:	type_declaration
	|	variable_declaration_wrapper
	|	function_declaration_wrapper
	;
type_declaration
	:	(TYPE type_id EQUAL type)+ -> ^(TYPE_DECLARATION[$TYPE, "TYPE_DECLARATION"] type_id type)+
	;
type	:	type_id -> ALIAS_TYPE_DECLARATION[((CommonTree)$type_id.tree).Token, "ALIAS_TYPE_DECLARATION"] type_id
	|	LBRACE type_fields? RBRACE -> RECORD_TYPE_DECLARATION[$LBRACE, "RECORD_TYPE_DECLARATION"] type_fields?
	|	ARRAY OF type_id -> ARRAY_TYPE_DECLARATION[$ARRAY, "ARRAY_TYPE_DECLARATION"] type_id
	;
type_fields
	:	type_field (COMMA! type_field)*
	;
type_field
	:	IDENTIFIER COLON type_id -> TYPE_DECLARATION_FIELD[$IDENTIFIER, "TYPE_DECLARATION_FIELD"] IDENTIFIER type_id
	;
type_id	:	IDENTIFIER
	;
variable_declaration_wrapper
	:	variable_declaration+
	;
variable_declaration
	:	VAR IDENTIFIER (COLON type_id)? ASSIGN expr -> ^(VAR_DECLARATION[$VAR, "VAR_DECLARATION"] IDENTIFIER (RETURN_TYPE type_id)? expr)
	;
function_declaration_wrapper
	:	function_declaration+
	;
function_declaration
	:	FUNCTION IDENTIFIER LPARENTHESIS type_fields? RPARENTHESIS (COLON type_id)? EQUAL expr -> ^(FUNCTION_DECLARATION[$FUNCTION, "FUNCTION_DECLARATION"] IDENTIFIER (RETURN_TYPE type_id)? type_fields? expr)
	;
