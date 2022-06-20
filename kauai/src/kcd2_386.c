/* Copyright (c) Microsoft Corporation.
   Licensed under the MIT License. */

/***************************************************************************
This c file is compiled to a command-line exe which when run produces
the 80386 asm version of the KCD2 decompression code.

Register usage:
    eax: source bytes
    ebx: source address
    edi: destination address
    edx: working space
    ecx: for rep movsb
    esi: for rep movsb
***************************************************************************/
#include <stdbool.h>
#include <stdlib.h>
#include <stddef.h>
#include <stdio.h>
#include "codkpri.h"

const int kcbitMinMoveByDword = 3;

const bool fFalse = 0;
const bool fTrue = 1;
static FILE *output = NULL;

void Setup(void);
void End(void);
void Advance(long cb);
void Test(long ibit);
void Block(long ibit);
void Literal(long ibit, bool fDword, bool fSingle);
void Offset(long ibit, bool fDword, long cbit, long cbitOH, long dibBase, long cbBase);
void Copy(long ibit, bool fDword);

int main(int argc, char *argv[])
{
    if (argc != 2)
    {
        fprintf(stderr, "usage: %s <filename>", argv[0]);
        return EXIT_FAILURE;
    }

    output = fopen(argv[1], "w");
    if (output == NULL)
    {
        fprintf(stderr, "could not open %s", argv[1]);
        return EXIT_FAILURE;
    }

    long ibit;

    Setup();

    for (ibit = 0; ibit < 8; ibit++)
    {
        if (kcbitMinMoveByDword <= kcbitMaxLenKcd2)
        {
            Literal(ibit, fTrue, fFalse);
            Offset(ibit, fTrue, kcbitKcd2_0, 2, kdibMinKcd2_0, 1);
            Offset(ibit, fTrue, kcbitKcd2_1, 3, kdibMinKcd2_1, 1);
            Offset(ibit, fTrue, kcbitKcd2_2, 4, kdibMinKcd2_2, 1);
            Offset(ibit, fTrue, kcbitKcd2_3, 4, kdibMinKcd2_3, 2);
            Copy(ibit, fTrue);
            fprintf(output, "	jmp LBlock%d\n", ibit);
        }
        Literal(ibit, fFalse, fTrue);
        Literal(ibit, fFalse, fFalse);
        Offset(ibit, fFalse, kcbitKcd2_0, 2, kdibMinKcd2_0, 1);
        Offset(ibit, fFalse, kcbitKcd2_1, 3, kdibMinKcd2_1, 1);
        Offset(ibit, fFalse, kcbitKcd2_2, 4, kdibMinKcd2_2, 1);
        Offset(ibit, fFalse, kcbitKcd2_3, 4, kdibMinKcd2_3, 2);
        Copy(ibit, fFalse);
        Block(ibit);
    }

    End();
    return 0;
}

void Setup(void)
{
    fprintf(output, "	// Setup\n"
                    "	long cbTot;\n"
                    "	byte *pbLimDst = (byte *)pvDst + cbDst;\n"
                    "	byte *pbLimSrc = (byte *)pvSrc + cbSrc - kcbTailKcd2;\n\n"
                    "__asm\n"
                    "	{\n"
                    "	mov edi,pvDst\n"
                    "	mov ebx,pvSrc\n"
                    "	inc ebx\n");

    Advance(4);

    fprintf(output, "	jmp LBlock0\n");
}

void Copy(long ibit, bool fDword)
{
    if (fDword)
        fprintf(output, "\n	// Copy Dword %d\nLCopyDword%d:\n", ibit, ibit);
    else
        fprintf(output, "\n	// Copy Byte %d\nLCopyByte%d:\n", ibit, ibit);

    fprintf(output, "#ifdef SAFETY\n"
                    "	push edx\n"
                    "	lea edx,[edi+ecx]\n"
                    "	cmp edx,pbLimDst\n"
                    "	pop edx\n"
                    "	ja LFail\n"
                    "#endif //SAFETY\n");

    fprintf(output, "	neg esi\n"
                    "	add esi,edi\n");

    fprintf(output, "#ifdef SAFETY\n"
                    "	cmp esi,pvDst\n"
                    "	jb LFail\n"
                    "#endif //SAFETY\n");

    if (fDword)
    {
        fprintf(output, "	mov edx,ecx\n"
                        "	shr ecx,2\n"
                        "	and edx,3\n"
                        "	rep movsd\n"
                        "	mov ecx,edx\n"
                        "	rep movsb\n");
    }
    else
        fprintf(output, "	rep movsb\n");
}

void End(void)
{
    fprintf(output, "\nLDone:\n"
                    "	sub edi,pvDst\n"
                    "	mov cbTot,edi\n"
                    "	}\n\n");
}

void Advance(long cb)
{
    switch (cb)
    {
    case 0:
        break;
    case 1:
        fprintf(output, "	mov eax,[ebx-3]\n"
                        "	inc ebx\n");
        break;
    case 2:
        fprintf(output, "	mov eax,[ebx-2]\n"
                        "	add ebx,2\n");
        break;
    case 3:
        fprintf(output, "	mov eax,[ebx-1]\n"
                        "	add ebx,3\n");
        break;
    case 4:
        fprintf(output, "	mov eax,[ebx]\n"
                        "	add ebx,4\n");
        break;
    default:
        fprintf(output, "*** BUG\n");
        break;
    }
}

void Test(long ibit)
{
    if (ibit < 8)
        fprintf(output, "	test al,%d\n", 1 << ibit);
    else if (ibit < 16)
        fprintf(output, "	test ah,%d\n", 1 << (ibit - 8));
    else
        fprintf(output, "	test eax,%d\n", 1 << ibit);
}

void Block(long ibit)
{
    long cbit, ibitT;

    fprintf(output, "\n	// Block %d\n", ibit);
    fprintf(output, "LBlock%d:\n", ibit);

    for (cbit = 0; cbit <= kcbitMaxLenKcd2; cbit++)
    {
        Test(ibit + cbit);
        fprintf(output, "	jz LLen%d_%d\n", ibit, cbit);
    }
    fprintf(output, "	jmp LDone\n");

    for (cbit = 0; cbit <= kcbitMaxLenKcd2; cbit++)
    {
        fprintf(output, "LLen%d_%d:\n", ibit, cbit);
        if (cbit == 0)
            fprintf(output, "	mov ecx,1\n");
        else
        {
            fprintf(output, "	mov ecx,eax\n");
            fprintf(output, "	shr ecx,%d\n", ibit + cbit + 1);
            fprintf(output, "	and ecx,%d\n", (1 << cbit) - 1);
            fprintf(output, "	or ecx,%d\n", (1 << cbit));
        }

        ibitT = ibit + cbit + cbit + 1;
        Advance(ibitT / 8);
        ibitT &= 0x07;
        Test(ibitT);
        if (cbit == 0)
            fprintf(output, "	jz LLitSingle%d\n", ibitT);
        else if (cbit < kcbitMinMoveByDword)
            fprintf(output, "	jz LLitByte%d\n", ibitT);
        else
            fprintf(output, "	jz LLitDword%d\n", ibitT);

        Test(ibitT + 1);
        if (cbit < kcbitMinMoveByDword)
        {
            fprintf(output, "	jz L%dBitOffByte%d\n", kcbitKcd2_0, ibitT);
            Test(ibitT + 2);
            fprintf(output, "	jz L%dBitOffByte%d\n", kcbitKcd2_1, ibitT);
            Test(ibitT + 3);
            fprintf(output, "	jz L%dBitOffByte%d\n", kcbitKcdc2, ibitT);
            fprintf(output, "	jmp L%dBitOffByte%d\n", kcbitKcdc3, ibitT);
        }
        else
        {
            fprintf(output, "	jz L%dBitOffDword%d\n", kcbitKcd2_0, ibitT);
            Test(ibitT + 2);
            fprintf(output, "	jz L%dBitOffDword%d\n", kcbitKcd2_1, ibitT);
            Test(ibitT + 3);
            fprintf(output, "	jz L%dBitOffDword%d\n", kcbitKcdc2, ibitT);
            fprintf(output, "	jmp L%dBitOffDword%d\n", kcbitKcdc3, ibitT);
        }
    }
}

void Offset(long ibit, bool fDword, long cbit, long cbitOH, long dibBase, long cbBase)
{
    ibit = (ibit - cbit - cbitOH) & 0x07;
    if (fDword)
        fprintf(output, "\nL%dBitOffDword%d:\n", cbit, ibit);
    else
        fprintf(output, "\nL%dBitOffByte%d:\n", cbit, ibit);

    fprintf(output, "	mov esi,eax\n");
    if (cbBase == 1)
        fprintf(output, "	inc ecx\n");
    else
        fprintf(output, "	add ecx,%d\n", cbBase);
    fprintf(output, "	shr esi,%d\n", ibit + cbitOH);
    fprintf(output, "	and esi,%d\n", (1 << cbit) - 1);
    if (1 == dibBase)
        fprintf(output, "	inc esi\n");
    else
        fprintf(output, "	add esi,%d\n", dibBase);

    ibit += cbitOH + cbit;
    Advance(ibit / 8);
    ibit &= 0x07;

    if (fDword)
    {
        if (dibBase < 4 && fDword)
        {
            fprintf(output, "	cmp esi,4\n");
            fprintf(output, "	jb LCopyByte%d\n", ibit);
        }
        fprintf(output, "	jmp LCopyDword%d\n", ibit);
    }
    else
        fprintf(output, "	jmp LCopyByte%d\n", ibit);
}

void Literal(long ibit, bool fDword, bool fSingle)
{
    ibit = (ibit - 1) & 0x07;
    if (fDword)
        fprintf(output, "\nLLitDword%d:\n", ibit);
    else if (fSingle)
        fprintf(output, "\nLLitSingle%d:\n", ibit);
    else
        fprintf(output, "\nLLitByte%d:\n", ibit);

    ibit++;
    if (fSingle)
    {
        fprintf(output, "#ifdef SAFETY\n"
                        "	cmp edi,pbLimDst\n"
                        "	jae LFail\n"
                        "#endif //SAFETY\n");

        if (ibit == 8)
        {
            fprintf(output, "	mov [edi],ah\n"
                            "	inc edi\n");
        }
        else
        {
            fprintf(output,
                    "	mov edx,eax\n"
                    "	shr edx,%d\n"
                    "	mov [edi],dl\n"
                    "	inc edi\n",
                    ibit);
        }

        ibit += 8;
        Advance(ibit / 8);
        fprintf(output, "	jmp LBlock%d\n", ibit & 0x07);
        return;
    }

    fprintf(output, "#ifdef SAFETY\n"
                    "	lea edx,[edi+ecx]\n"
                    "	cmp edx,pbLimDst\n"
                    "	ja LFail\n"
                    "#endif //SAFETY\n");

    if (ibit != 8)
    {
        // get the low bits of the last byte
        fprintf(output, "	dec ecx\n");
        fprintf(output, "	mov edx,eax\n");
        fprintf(output, "	shr edx,%d\n", ibit);
        fprintf(output, "	and edx,%d\n", (1 << (8 - ibit)) - 1);
    }

    fprintf(output, "#ifdef SAFETY\n"
                    "	lea esi,[ebx-3+ecx]\n"
                    "	cmp esi,pbLimSrc\n"
                    "	ja LFail\n"
                    "	sub esi,ecx\n"
                    "#else //!SAFETY\n"
                    "	lea esi,[ebx-3]\n"
                    "#endif //!SAFETY\n");

    if (fDword)
    {
        fprintf(output, "	mov eax,ecx\n"
                        "	shr ecx,2\n"
                        "	and eax,3\n"
                        "	rep movsd\n"
                        "	mov ecx,eax\n"
                        "	rep movsb\n");
    }
    else
        fprintf(output, "	rep movsb\n");

    fprintf(output, "	lea ebx,[esi+4]\n"
                    "	mov eax,[esi]\n");

    if (ibit != 8)
    {
        fprintf(output, "	mov esi,eax\n");
        fprintf(output, "	shl esi,%d\n", 8 - ibit);
        fprintf(output, "	or edx,esi\n"
                        "	mov [edi],dl\n"
                        "	inc edi\n");
    }

    fprintf(output, "	jmp LBlock%d\n", ibit & 0x07);
}
