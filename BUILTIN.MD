# BUILTIN ASSEMBLY FUNCTIONS

## PROGRAM COUNTER OUT OF RANGE

```
;===================================
; Program counter out of range error
;===================================
PC_OUT_OF_RANGE:

    mov dx,.msg
    call PRINT_STRING
    call TERMINATE_PROGRAM
.msg: DB ""Program counter out of range"",00h
```

## DIVISION BY ZERO ERROR

```
;=======================
; Division by zero error
;=======================
DIVISION_BY_ZERO:

    mov dx,.msg
    call PRINT_STRING
    call TERMINATE_PROGRAM
.msg: DB ""Division by zero"",00h
```

## ARITHMETIC OVERFLOW ERROR

```
;==========================
; Arithmetic overflow error
;==========================
ARITHEMETIC_OVERFLOW:

    mov dx,.msg
    call PRINT_STRING
    call TERMINATE_PROGRAM
.msg: DB ""Arithmetic overflow"",00h
```

## TERMINATE PROGRAM

```
;==================
; Terminate program
;================== 
TERMINATE_PROGRAM:

    mov ah,4Ch      ;dos exit
    mov al,00h
    int 21h
```

## PRINT NEW LINE

```
;====================
; New line as CR + LF
;==================== 
NEW_LINE:

    mov ah,0Eh      ;teletype print interupt 
    mov bh,00h      ;page
    
    mov al,0Dh      ;CR
    int 10h         ;call interrupt

    mov al,0Ah      ;LF 
    int 10h         ;call interrupt

    ret
```

## PRINT STRING

```
;=========================================
; Expect in DX the beginning of the string
;=========================================
PRINT_STRING:                       

    mov si,dx       ;copy the start of string in SI
    mov ah,0Eh      ;interrupt 10h code(teletype print)
    mov bh,00h      ;interrupt 10h page 0

.next: 
    mov al,[si]     ;copy in al the current byte to print
    cmp al,00h      ;compare al with 0x00 (string end)
    jz  .done       ;if zero, go to finish
    int 10h         ;call interrupt
    inc si          ;move forward
    jmp .next       ;repeat

.done:
    ret
```

## PRINT NUMBER IN DECIMAL FORMAT

```
;==========================================================
; Print a number in decimal format, expect in DX the number
; NOTE : we use the stack to 'decompose' the number
;==========================================================
PRINT_NUMBER:

    mov ax,dx       ;copy the number in AX
    cmp ax,00h      ;it is zero?
    je  .zero       ;jmp to special case

    or  ax,ax       ;check if negative
    jns .positive   ;no, continue
    
.negative:                                                    
    mov ah,0Eh      ;print the minus sign
    mov al,2Dh
    mov bh,00h
    int 10h

    mov ax,dx       ;put in AX the absolute value
    not ax
    inc ax

.positive:    
    mov bx,0Ah      ;we use base 10
    xor cx,cx       ;cx will be counter for stack
    xor dx,dx       ;will hold remainder of division

.pusher:            
    cmp ax,00h      ;if here finished
    je  .popper     ;go to popper to print the numbers

    div bx          ;divide AX by 10
            
    push dx         ;push the remainder of division on stack

    inc cx          ;increase the stack counter

    xor dx,dx       ;clear DX for next division

    jmp .pusher     ;repeat pusher cycle

.popper:            
    cmp cx,00h      ;still have elements on stack?
    je  .done       ;no, exit

    pop ax          ;get the number from the stack

    mov ah,0Eh      ;set ah for teletype print
    add al,30h      ;make the number an ascii value
    mov bh,00h      ;page 00h
    int 10h         ;call interrupt

    dec cx          ;decrease the counter

    jmp .popper     ;repeat pop item

.zero:              ;special case for 0
    mov ah,0EH
    mov al,30h
    mov bh,00h
    int 10h

.done:
    ret
```

## READ INPUT STRING

```
;====================================================
; Read from keyboard until ENTER is pressed
; Expect in CX the size, in DX the destination buffer
;====================================================
READ_STRING:

    mov al,00h      ;value to initialize the buffer
    mov	di,dx       ;move destination buffer in DI
    rep stosb       ;repeat

    dec di          ;reserve last byte for EOF (0x00)
    mov si,dx       ;copy buffer start position in SI

.cycle:
    xor ax,ax       ;keyboard read function
    int 16h         ;ah scancode, al ascii code

    cmp al,0Dh      ;is enter pressed ?
    je  .done       ;ok finished

    cmp	al,08h      ;is backspace ?
    je  .backspace

    cmp si,di       ;do we have any space left ?
    jge .cycle      ;if not skip insertion

    cmp	al,20h      ;is printable ?
    jb  .cycle
    cmp al,7Eh
    ja  .cycle

    mov [si],al     ;store character 
    inc si          ;advance to next position

    mov ah,0Eh      ;teletype print interupt
    mov bh,00h      ;page
    int 10h 

    jmp .cycle

.backspace:
    cmp si,dx       ;we are at beginning of buffer
    jle .cycle      ;skip backspace

    dec si          ;dec SI and store 0x00 in buffer
    mov [si],byte 00h
    
    mov bh,00h      ;set page number for all operations

    push dx         ;save buffer start

    mov ah,03h      ;cursor position in DH,DL (row/col)
    int 10h

    dec dl          ;dec the col
    mov ah,02h      ;set cur pos
    int	10h

    mov ah,0Ah      ;write char at current cursor pos
    mov al,20h      ;we use empty space
    int 10h

    pop dx          ;restore buffer start in DX

    jmp .cycle
    
.done:
    ret
```

## STRING TO NUMBER CONVERSION

```
;====================================================
; Convert null terminated string into number, expect:
; - CX address of variable
; - DX address of string
; NOTE: on success AL = 1, on error AL = 0
;====================================================
STRING_TO_NUMBER:

    mov di,cx           ;use DI as var pointer
    mov si,dx           ;use SI as string pointer

    xor ax,ax           ;clear AX
    xor cx,cx           ;clear CX
    
.check_sign:

    cmp [si],byte 00h   ;empty string ?
    je  .err

    cmp [si],byte 2Bh   ;check '+'
    je .cycle

    cmp [si],byte 2Dh   ;check '-'
    jne .number

    mov bx,01h          ;save sign to BX

.cycle:
    inc si              ;advance to next character

.number:

    mov cl,[si]         ;get the ascii char in CX
    cmp cl,00h          ;EOL, done
    je .adjust_sign

    cmp cl,30h          ;is less than 0 ?
    jl  .err

    cmp cl,39h          ;is greater than 9 ?
    jg .err
    
    sub cl,30h          ;ascii to number

    mov dx,0Ah          ;DX multiplier by 10
    mul dx
    jo  .err            ;exit on overflow

    add ax,cx           ;add the number
    jo  .err            ;exit on overflow

    jmp .cycle          ;repeat

.adjust_sign:           
    cmp bx,01h          ;was negative ?
    jne .done           ;no, skip
    neg ax              ;ye, negate it

.done:
    mov [di],ax         ;store result
    mov al,01h          ;return 1 on success
    ret

.err:
    xor al,al           ;return 0 on error
    ret
```
