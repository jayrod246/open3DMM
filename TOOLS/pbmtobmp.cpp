
#include <stdio.h>
#include "frame.h"
ASSERTNAME

void FrameMain(void)
{
}


/***************************************************************************
	Main routine.  Returns non-zero iff there's an error.
***************************************************************************/
int __cdecl main(int cpszs, char *prgpszs[])
{
	FNI fniSrc, fniDst;
	FNI fniPalette;
	STN stn;
	FLO floSrc;
	long lwSig;
	BLCK blck;
	bool fPacked;
	bool fCompress = fFalse;
	long cfni = 0;
	long cfmt = vpcodmUtil->CfmtDefault();
	long lwSwapped;
	PMBMP mbmp;
	PGL pglclrSrc;


#ifdef UNICODE
	fprintf(stderr, "\nPBM to MBMP Utility (Unicode; "
		Debug("Debug; ") __DATE__ "; " __TIME__ ")\n");
#else //!UNICODE
	fprintf(stderr, "\nPBM to MBMPUtility (Ansi; "
		Debug("Debug; ") __DATE__ "; " __TIME__ ")\n");
#endif //!UNICODE


	floSrc.pfil = pvNil;
	if (cpszs != 4){
		fprintf(stderr, "Wrong number of file names\n\n");
		goto LUsage;
	}

	stn.SetSzs(prgpszs[1]);
	if (!fniSrc.FBuildFromPath(&stn)){
		fprintf(stderr, "Bad source file name\n\n");
		goto LUsage;
	}
	stn.SetSzs(prgpszs[2]);
	if (!fniDst.FBuildFromPath(&stn)){
		fprintf(stderr, "Bad dest file name\n\n");
		goto LUsage;
	}
	stn.SetSzs(prgpszs[3]);
	if (!fniPalette.FBuildFromPath(&stn)){
		fprintf(stderr, "Bad palette file name\n\n");
		goto LUsage;
	}

	if (pvNil == (floSrc.pfil = FIL::PfilOpen(&fniSrc)))
		{
		fprintf(stderr, "Can't open source file\n\n");
		goto LFail;
		}
	floSrc.fp = 0;
	floSrc.cb = floSrc.pfil->FpMac();



	if (floSrc.cb < size(long)){
		fprintf(stderr, "floSrc.cb too small!\n\n");
		goto LBadSrc;
	}

	if (!floSrc.FReadRgb(&lwSig, size(long), 0))
		{
		fprintf(stderr, "Reading source file failed\n\n");
		goto LFail;
		}

	lwSwapped = lwSig;
	SwapBytesRglw(&lwSwapped, 1);
	if (lwSig == klwSigPackedFile || lwSwapped == klwSigPackedFile)
		fPacked = fTrue;
	else if (lwSig == klwSigUnpackedFile || lwSwapped == klwSigUnpackedFile)
		fPacked = fFalse;
	else
		{
LBadSrc:
		fprintf(stderr, "Source file is not packed\n\n");
		goto LFail;
		}

	blck.Set(floSrc.pfil, size(long), floSrc.cb - size(long), fPacked);

	{
			byte *prgb = pvNil;
			
			long dxp, dyp;
			bool fUpsideDown;

			if(!FReadBitmap(&fniPalette, &prgb, &pglclrSrc, &dxp, &dyp, &fUpsideDown)){
				fprintf(stderr, "Reading palette BMP failed\n\n");
				goto LFail;
			}
			FreePpv((void **)&prgb);
	}

	fprintf(stderr, "Reading MBMP...\n");

	mbmp = MBMP::PmbmpRead(&blck);
	if(mbmp==pvNil){
		fprintf(stderr, "Reading MBMP failed\n\n");
		goto LFail;
	}
	fprintf(stderr, "Read MBMP.\n");
	if(1){
		RC size;
		int buffer_size;
		mbmp->GetRc(&size);
		fprintf(stderr, "Size is left=%d top=%d right=%d bottom=%d.\n",size.xpLeft,size.ypTop,size.xpRight,size.ypBottom);
		buffer_size=size.xpRight*size.ypBottom;
		byte *temp_buffer = new byte [buffer_size];
		mbmp->Draw(temp_buffer, size.xpRight, size.ypBottom, 0, 0);
		fprintf(stderr, "Drew %d bytes\n",buffer_size);
		if(!FWriteBitmap(&fniDst, temp_buffer, pglclrSrc, size.xpRight, size.ypBottom)){
			fprintf(stderr, "writing BMP failed\n\n");
			goto LFail;
		}
		fprintf(stderr, "wrote BMP\n\n");
		
		
	}

	ReleasePpo(&floSrc.pfil);
	FIL::ShutDown();
	return 0;

LUsage:
	//print usage
	fprintf(stderr, "%s",
		"Usage:  pbmtobmp <srcFile> <dstFile> <palette.bmp>\n\n");

LFail:
	ReleasePpo(&floSrc.pfil);

	FIL::ShutDown();
	return 1;
}

