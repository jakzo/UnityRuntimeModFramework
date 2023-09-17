bits 64

GetProcAddress dq 0x123456789abc0001 ; uint64 pointer
hModule dq 0x123456789abc0002 ; uint64
procName dq 0x123456789abc0003 ; uint64 pointer to null-terminated string
result dq 0x123456789abc0004 ; uint64 pointer

mov rax, [GetProcAddress]
mov rdx, [hModule]
mov rcx, [procName]
call rax
mov [result], rax
ret
