// Derived from http://json.org

grammar parameters;

//@lexer::members {public bool brackets = false;}
////@lexer::members {public static final boolean brackets = true;}

//@lexer::members {public const int FRED = 2;}
//@lexer::members {public static final int FRED = 2;}

/////////////////////////////////////////////////////////////////////////////

parameters: argumentsList;


/////////////////////////////////////////////////////////////////////////////

//json:	object
//		|	array
//		;

/////////////////////////////////////////////////////////////////////////////

//object
//		:	'{' pair (',' pair)* '}'
//		|	'{' '}' // empty object
//		;
//pair:	STRING ':' value ;


/////////////////////////////////////////////////////////////////////////////

//argumentsList
//	:	'(' value (',' value)* ')'	# aParametersList
//	|	'(' ')'						# emptyParametersList
//	;

//argumentsList
//	:	value (',' value)*
//	;

arrayArgumentsList
	:	value (',' value)* bterminal
	;


argumentsList
	:	value (',' value)* pterminal
//	|	')'
	;

//
// note EOF means UNTIL the end of file, but we also allow ')'
//
// if we have to process to the end-of-file that is okay, there
// is an issue where ANTLR will not stop parsing when it sees
// the ')' - so we notice this and flag EOF on the input (this
// is in ArgumentsParsers.cs
//

//arguments : value (',' value)* ;
pterminal : ')' | EOF;
bterminal : ']' | EOF;
//eof : EOF ;

/////////////////////////////////////////////////////////////////////////////

singleArgument
	:	value EOF
	;


/////////////////////////////////////////////////////////////////////////////

array
	:	'[' value (',' value)* ']'	# arrayOfValues
	|	'[' ']'											# emptyArray
	;


/////////////////////////////////////////////////////////////////////////////

value
		:	NUMBER					# Number
		|	INTEGER					# Integer
		|	STRING					# String
		|	OBJECT_REF			# ObjectRef
		|	TEXT						# UndelimitedText
//	|	object					# object						// recursion
		|	array						# arrayObject				// recursion
		|	'true'					# True							// keywords
		|	'false'					# False
		|	'null'					# Null
		|									# Empty
		;


/////////////////////////////////////////////////////////////////////////////

//NUMBER
//		:	'-'? INT '.' [0-9]+ EXP? // 1.35, 1.35E-9, 0.3, -4.5
//		|	'-'? INT EXP						// 1e10 -3e4
//		|	'-'? INT								// -3, 45
//		{ System.out.println( "number: '" + getText() + "'" ); }
//		;

//NUMBER
//		:	'-'? INT '.' INT	// 1.35, 1.35E-9, 0.3, -4.5
//		|	'-'? INT						// -3, 45
//		{ System.out.println( "number: '" + getText() + "'" ); }
//		;

INTEGER
		:	'-'? [0-9]+						// -3, 45
		//{ System.out.println( "int: '" + getText() + "'" ); }
		;

NUMBER
		:	'-'? [0-9]* '.' [0-9]+	// 1.35, 1.35E-9, 0.3, -4.5
		//{ System.out.println( "number: '" + getText() + "'" ); }
		;



//fragment INT :	'0' | [1-9] [0-9]* ; // no leading zeros
//fragment INT :	[0-9] [0-9]*? ; // no leading zeros
//fragment INT :	[0-9]+? ; // no leading zeros




fragment INT :	[0-9]+? ; // no leading zeros
fragment EXP :	[Ee] [+\-]? INT ; // \- since - means "range" inside [...]


/////////////////////////////////////////////////////////////////////////////
//
//
//

STRING
//
// this next allows " ... ` "xx" '  " (double quotes embedded in the
// string without having to quote them; however, this requires that all
// `' be fully balanced, so we're not using that
//
//		:	'"' (ESC | M4_QUOTES | ~[`"%])* '"' 
		:	'"' (ESC | ~["%])* '"' 
		;

SINGLE_QUOTE_STRING
		: '\'' (ESC | ~['%])* '\''
		-> type(STRING)
		;

M4_QUOTES
		: '`' (M4_QUOTES | '%`' | '%\'' | '%%' | ~['%])*? '\''
		-> type(STRING)
		;

//
// to change escape char need to modify ParametersListener, and maybe others
//
fragment ESC :	'%' (["%bfnrt] | UNICODE) ;
fragment UNICODE : 'u' HEX HEX HEX HEX ;
fragment HEX : [0-9a-fA-F] ;


/////////////////////////////////////////////////////////////////////////////
//
// wraps the reference to an object - a name which must be an existing macro,
//

OBJECT_REF
		: '&' OBJECT_START_CHARS OBJECT_BODY_CHARS OBJECT_END_CHAR
		//{ System.out.println( "objectRef: '" + getText() + "'" ); }
		;

fragment OBJECT_START_CHARS : [@#_a-zA-Z] ;
fragment OBJECT_BODY_CHARS :	[.-_a-zA-Z0-9]* ;
fragment OBJECT_END_CHAR :		[_a-zA-Z0-9] ;


/////////////////////////////////////////////////////////////////////////////
//
// TEXT is text that is not delimited by "", '', [], or `', and 
// starts with any legal macro start character
//
// purpose is to allow a single ?? macro (and any dotting, brackets
// or whatever follow-ons) to execute and provide an argument for the
// parameters list being parsed
//

TEXT 
	: (INNER_PARENS | MACRO_START_CHARS) (INNER_PARENS | INNER_BRACKETS | ~[,)\]\uE001] )*
		//{ System.out.println( "text: '" + getText() + "'" ); }
		;

fragment INNER_PARENS
	: '(' (INNER_PARENS | .)*? ')'
	;

fragment INNER_BRACKETS
	: '[' (INNER_BRACKETS | .)*? ']'
	;

fragment MACRO_START_CHARS	
		: [#@_a-zA-Z]
		;

fragment NOT_MACRO_START_CHARS	
		: ~[#@_a-zA-Z]
		-> skip ;

//fragment NOT_START_CHARS
//	: ~( ',' | '"' | '\'' | '`' | '[' | '&' | ')' | ']' | '-' | [0-9] | ' ' | '\t' | '\r' | '\n' )
//	;

//IGNORE : NOT_MACRO_START_CHARS;


/////////////////////////////////////////////////////////////////////////////
//
// '\uE001' is the EOL_COMMENT_PLACEHOLDER (inserted when ";;" is found so
// we can remove the newline after all macro processing; see StringReader.CleanText)
// this character needs to be treated as white-space inside of an argument list
//

WS	:	[ \t\n\r\uE001]+ -> skip ;


