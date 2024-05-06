# CSTINY

CSTINY is a Basic language to Assembly language converter.

This project was created to celebrate the 60th anniversary of the Basic language.

## DESCRIPTION

CSTINY is a command line tool written in C# 10 .NET 6

Given a source file written in Tiny Basic language it can :

- display a simple Ast on stdout 
- directly execute the program
- convert it to Assembly language

## USAGE

```
> CSTINY.EXE [params]

Params reference:

-t srcfile           [display the ast on stdout]
-i srcfile           [execute the interpreter]
-n srcfile dstfile   [call the nasm transpiler]
```

## IMPLEMENTATION

The code project is divided in simple classes with a contructor and a main public method to call.

The ***Lexer*** class is used to create a stream of tokens starting from a text file as input.

The ***Parser*** class organize the tokens to build an Ast.

According to the original grammar, unary operators are not allowed.

Expressions starting with a sign will be rewritten using binary operators.

As example, the expression -3 will be handled as (0 - 3) converting it using BinaryNode(0, "-", 3) 

When the Ast has been generated it is easy walk through it to accomplish different tasks.

Each tree visitor contains a "dispatcher" metod Visit(Ast ast) to explore all or only the needed nodes.

The ***Printer*** class will print on the stdout the shape of the tree, indenting somehow the nodes.

The ***Analyzer*** class will take care to verify that :

- there are no duplicated line numbers
- line numbers are an ordered sequence
- GOTO and GOSUB statements have a valid destination target

The ***Symtab*** class will :

- create a symbol table, collecting the name and the type of variables
- verify that variables are not used before declaration
- enforce type safety

The ***Nasmer*** class will explore the tree returning a string containing the converted assembly source code.

## TINY BASIC LANGUAGE

The supported language is a lightly modified version of the original Tiny Basic grammar, to allow the support of both integers and strings variables.

**COMMENTS**

- comments lines can be inserted using the '#' character
- everything after the '#' character will be ignored

```
# this is a comment
PRINT 3
# PRINT "hello"
--> print "3"
```

**INTEGER**

- declare a variable of type integer
- to declare mutiple variables use the "," character as separator
- default value for integers is 0
- integers variables can store numbers between -32767 and +32767

```
INTEGER a
PRINT a
--> print "0"

INTEGER i,j
LET i = 33
PRINT i,j
-->print "33"
```

**STRING**

- declare a variable of type string
- to declare mutiple variables use the "," character as separator
- default string length is 32 including the null terminator character 00h
- default value for strings is an empty string

```
STRING A
PRINT A
--> print ""

STRING A,B
LET A = "hello"
PRINT A,B
--> print "hello"
```

**LET**

- change the content of a variable

```
INTEGER A
LET A = 69 
--> store in A the number 69

INTEGER a
LET A = 5*2 
--> store in A the nuber 10

STRING A
LET A = "hello"
--> store in A the string "hello"

STRING A,B
LET A = "hello" 
LET B = A
--> store in A the string "hello", copy in B the string "hello"

INTEGER A
STRING B
LET A = B
--> generate an error
```

**INPUT**

- according to the variable type, read a number or a string from the terminal and store it in the designated variable

```
INTEGER A
INPUT A
--> read a number and store it in A

STRING A
INPUT A
--> read a string and store it in A
```

**PRINT**

- allow to print on screen numbers, expressions, variables, strings and their combinations
- to concatenate elements use the "," character as separator

```
PRINT 123
--> print "123"

PRINT (4*5)+6
--> print "26"

PRINT A
--> print the value stored in A

PRINT "hello"
--> print "hello""

PRINT "2+3 is :",2+3
--> print "2+3 is 5"
```

**IF**

- if the condition is true then execute the instruction following THEN keyword
- logical operators supported : ==,!=,<,>,<=,>=

```
IF 2!=1 THEN PRINT "hello"
--> always print "hello"

IF A<(3*4) THEN PRINT "hello"
--> print "hello" only if the content of the variable A is less than 12
```

**GOTO**

- continue the execution of the program at the indicated line number

```
GOTO 25
--> continue the execution at line 25

10 PRINT "hello"
20 GOTO 10
30 END
--> print "hello" repeatedly (does not terminate)
```

**GOSUB**

- execute the instructions from the indicated line number up to the first RETURN keyword found

```
10 PRINT "line1"
20 GOSUB 100
30 END
100 PRINT "line2"
110 RETURN
--> print "line1", "line2" and terminate
```

**RETURN**

- resumes the execution of the program at the line following the last call to GOSUB

```
10 PRINT "line1"
20 GOSUB 100
30 END
100 PRINT "line2"
110 RETURN
--> print "line1", "line2" and terminate
```

**END**

- Terminate the execution of the program and return the control to DOS

```
10 PRINT "hello" 
20 END						
--> print "hello" and terminate

10 PRINT "line1"
20 END
30 PRINT "line2"		
--> print "line1" and terminate
```



## GRAMMAR

```
    program ::= line* EOF

    line ::= number statement CR
 
    statement ::= 'INTEGER' identlist
                  'STRING' identlist
                  'LET' ident = item
                  'INPUT' ident
                  'PRINT' itemlist
                  'IF' expression relop expression 'THEN' statement
                  'GOTO' number
                  'GOSUB' number
                  'RETURN'
                  'END'
 
    identlist ::= ident ( ',' ident)*
    
    item ::= (integer | string | ident | expression)
    
    itemlist ::=  item (',' item)*
    
    expression ::= ('+'|'-'|)? term (('+|-') term)*
 
    term ::= factor (('*'|'/') factor)*
 
    factor ::= ident | number | '(' expression ')'
 
    op ::= '+' | '-' | '*' | '/'

    relop ::= '<' | '<=' | '>' | '>=' | '==' | '<>' | '!='

    ident ::= LETTER (LETTER | DIGIT)*

    number ::= DIGIT DIGIT*
    
    string ::= '"' .... '"'
 
    DIGIT ::= 0 .. 9
 
    LETTER ::= a .. z | A .. Z
```

## ASSEMBLY LANGUAGE

The transpiler will convert Basic source code into Assembly code using NASM syntax.

The target is set for 8086 isa, using tiny model (CS, DS, and SS will point to the same 64 Kb segment)

The output is source ready to be assembled into a COM executable for DOS.

The entry point is set as :

```
ORG 100h
```

and the return to Dos prompt is set as :

```
mov ah,4Ch
mov al,00h
int 21h
```

Example of how to invoke Nasm against the generated assembly code :

```
nasm.exe -f bin test.asm -o test.com
```

By default 32 Bytes are reserved for variables of type string, this can be changed modifying the **STRING_SIZE** value.

Integers are stored as signed shorts using a single Word, expressions are solved using the Stack.

All the builtin functions make use only of Bios interrupts of consequence with few changes the generated code could also be used in first and second stage boot loaders.

Source code of all internal routines is available [here](BUILTIN.MD).

## CONVERSION EXAMPLE

**BASIC INPUT CODE**

```
################
# Test program #
################
10 Integer G,N,count
15 String name,str1,str2
20 Let N = 69
25 Let str1 = "What is your name ?"
30 Let str2 = str1
35 Print str2
40 Input name
45 Print "Hello ", name, ", can you guess a number ?"
50 Input G
60 Let count = count + 1
70 If G == N Then GoTo 110
80 If G>N Then PRINT "Lower !"
90 If G<N Then PRINT "Higher !"
100 GoTo 50
110 Gosub 130
120 End

###################
# subroutine test #
###################
130 Print "Good work ", name, ", you guessed it in ", count, " tries !!!"
140 Return
```

**ASSEMBLY OUTPUT CODE**

```
[CPU 8086]
[BITS 16]
[ORG 100h]

Line_10:
.Line_end:

Line_15:
.Line_end:

Line_20:
	mov [N],word 69
.Line_end:

Line_25:
	mov si,Pool_0
	mov di,str1
	mov cx,32
	rep movsb
.Line_end:

Line_30:
	mov si,str1
	mov di,str2
	mov cx,32
	rep movsb
.Line_end:

Line_35:
	mov dx,str2
	call PRINT_STRING
	call NEW_LINE
.Line_end:

Line_40:
	mov cx,32
	mov dx,name
	call READ_STRING
	call NEW_LINE
.Line_end:

Line_45:
	mov dx,Pool_1
	call PRINT_STRING
	mov dx,name
	call PRINT_STRING
	mov dx,Pool_2
	call PRINT_STRING
	call NEW_LINE
.Line_end:

Line_50:
.read_number:
	mov cx,32
	mov dx,STRING_BUFFER
	call READ_STRING
	call NEW_LINE
	mov cx,G
	mov dx,STRING_BUFFER
	call STRING_TO_NUMBER
	cmp al,0
	je .read_number
.Line_end:

Line_60:
	mov ax,[count]
	push ax
	mov ax,1
	push ax
	pop bx
	pop ax
	add ax,bx
	jo ARITHEMETIC_OVERFLOW
	push ax
	pop word [count]
.Line_end:

Line_70:
	mov ax,[G]
	push ax
	mov ax,[N]
	push ax
	pop bx
	pop ax
	cmp ax,bx
	jne .Line_end
	jmp Line_110
.Line_end:

Line_80:
	mov ax,[G]
	push ax
	mov ax,[N]
	push ax
	pop bx
	pop ax
	cmp ax,bx
	jng .Line_end
	mov dx,Pool_3
	call PRINT_STRING
	call NEW_LINE
.Line_end:

Line_90:
	mov ax,[G]
	push ax
	mov ax,[N]
	push ax
	pop bx
	pop ax
	cmp ax,bx
	jnl .Line_end
	mov dx,Pool_4
	call PRINT_STRING
	call NEW_LINE
.Line_end:

Line_100:
	jmp Line_50
.Line_end:

Line_110:
	call Line_130
.Line_end:

Line_120:
	call TERMINATE_PROGRAM
.Line_end:

Line_130:
	mov dx,Pool_5
	call PRINT_STRING
	mov dx,name
	call PRINT_STRING
	mov dx,Pool_6
	call PRINT_STRING
	mov dx,[count]
	call PRINT_NUMBER
	mov dx,Pool_7
	call PRINT_STRING
	call NEW_LINE
.Line_end:

Line_140:
	ret
.Line_end:

jmp PC_OUT_OF_RANGE

G: DW 0000h
N: DW 0000h
count: DW 0000h
name: TIMES 32 DB 00h
str1: TIMES 32 DB 00h
str2: TIMES 32 DB 00h

Pool_0: DB "What is your name ?",00h
Pool_1: DB "Hello ",00h
Pool_2: DB ", can you guess a number ?",00h
Pool_3: DB "Lower !",00h
Pool_4: DB "Higher !",00h
Pool_5: DB "Good work ",00h
Pool_6: DB ", you guessed it in ",00h
Pool_7: DB " tries !!!",00h

[...OMITTED BUILTIN ROUTINES...]
```

## REFERENCES

[Tiny Basic Language](http://www.ittybittycomputers.com/IttyBitty/TinyBasic/DDJ1/Design.html)

[Netwide Assembler](https://www.nasm.us/)