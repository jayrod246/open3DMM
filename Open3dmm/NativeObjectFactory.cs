﻿using Open3dmm.Classes;
using System;
using System.Collections.Generic;

namespace Open3dmm
{
    internal static class NativeObjectFactory
    {
        static readonly Dictionary<ClassID, Func<NativeObject>> factories;
        static NativeObjectFactory()
        {
            factories = new Dictionary<ClassID, Func<NativeObject>>()
            {
                { new ClassID("ACTR"), () => new ACTR() },
                { new ClassID("BACO"), () => new BACO() },
                { new ClassID("ACTN"), () => new ACTN() },
                { new ClassID("BKGD"), () => new BKGD() },
                { new ClassID("CABO"), () => new CABO() },
                { new ClassID("CAMS"), () => new CAMS() },
                { new ClassID("CMTL"), () => new CMTL() },
                { new ClassID("CURS"), () => new CURS() },
                { new ClassID("GOKD"), () => new GOKD() },
                { new ClassID("GKDS"), () => new GKDS() },
                { new ClassID("MBMP"), () => new MBMP() },
                { new ClassID("MDWS"), () => new MDWS() },
                { new ClassID("MIDS"), () => new MIDS() },
                { new ClassID("MODL"), () => new MODL() },
                { new ClassID("MSND"), () => new MSND() },
                { new ClassID("MTRL"), () => new MTRL() },
                { new ClassID("SCPT"), () => new SCPT() },
                { new ClassID("TDF"), () => new TDF() },
                { new ClassID("TMAP"), () => new TMAP() },
                { new ClassID("TMPL"), () => new TMPL() },
                { new ClassID("TDT"), () => new TDT() },
                { new ClassID("ZBMP"), () => new ZBMP() },
                { new ClassID("BCL"), () => new BCL() },
                { new ClassID("BCLS"), () => new BCLS() },
                { new ClassID("BLCK"), () => new BLCK() },
                { new ClassID("BLL"), () => new BLL() },
                { new ClassID("CFL"), () => new CFL() },
                { new ClassID("FIL"), () => new FIL() },
                { new ClassID("BODY"), () => new BODY() },
                { new ClassID("BRCN"), () => new BRCN() },
                { new ClassID("BRCL"), () => new BRCL() },
                { new ClassID("BSF"), () => new BSF() },
                { new ClassID("BSM"), () => new BSM() },
                { new ClassID("BWLD"), () => new BWLD() },
                { new ClassID("CEX"), () => new CEX() },
                { new ClassID("CGE"), () => new CGE() },
                { new ClassID("CLIP"), () => new CLIP() },
                { new ClassID("CMH"), () => new CMH() },
                { new ClassID("APPB"), () => new APPB() },
                { new ClassID("APP"), () => new APP() },
                { new ClassID("CLOK"), () => new CLOK() },
                { new ClassID("DOCB"), () => new DOCB() },
                { new ClassID("ACLP"), () => new ACLP() },
                { new ClassID("MVIE"), () => new MVIE() },
                { new ClassID("TXTB"), () => new TXTB() },
                { new ClassID("TXRD"), () => new TXRD() },
                { new ClassID("TBOX"), () => new TBOX() },
                { new ClassID("TXHD"), () => new TXHD() },
                { new ClassID("GOB"), () => new GOB() },
                { new ClassID("APE"), () => new APE() },
                { new ClassID("CTL"), () => new CTL() },
                { new ClassID("SCB"), () => new SCB() },
                { new ClassID("WSB"), () => new WSB() },
                { new ClassID("DDG"), () => new DDG() },
                { new ClassID("MVU"), () => new MVU() },
                { new ClassID("TXTG"), () => new TXTG() },
                { new ClassID("TXRG"), () => new TXRG() },
                { new ClassID("TBXG"), () => new TBXG() },
                { new ClassID("TXHG"), () => new TXHG() },
                { new ClassID("DMD"), () => new DMD() },
                { new ClassID("DMW"), () => new DMW() },
                { new ClassID("DSG"), () => new DSG() },
                { new ClassID("DSSM"), () => new DSSM() },
                { new ClassID("DSSP"), () => new DSSP() },
                { new ClassID("EDCB"), () => new EDCB() },
                { new ClassID("EDPL"), () => new EDPL() },
                { new ClassID("EDSL"), () => new EDSL() },
                { new ClassID("SNE"), () => new SNE() },
                { new ClassID("GOK"), () => new GOK() },
                { new ClassID("BRWD"), () => new BRWD() },
                { new ClassID("BRWL"), () => new BRWL() },
                { new ClassID("BRWB"), () => new BRWB() },
                { new ClassID("BRWC"), () => new BRWC() },
                { new ClassID("BRWN"), () => new BRWN() },
                { new ClassID("BRWM"), () => new BRWM() },
                { new ClassID("BRWP"), () => new BRWP() },
                { new ClassID("MP"), () => new MP() },
                { new ClassID("BRWR"), () => new BRWR() },
                { new ClassID("BRWT"), () => new BRWT() },
                { new ClassID("BRWA"), () => new BRWA() },
                { new ClassID("MPFT"), () => new MPFT() },
                { new ClassID("ESL"), () => new ESL() },
                { new ClassID("ESLA"), () => new ESLA() },
                { new ClassID("ESLL"), () => new ESLL() },
                { new ClassID("ESLR"), () => new ESLR() },
                { new ClassID("ESLT"), () => new ESLT() },
                { new ClassID("HBAL"), () => new HBAL() },
                { new ClassID("HBTN"), () => new HBTN() },
                { new ClassID("SCRT"), () => new SCRT() },
                { new ClassID("SPLT"), () => new SPLT() },
                { new ClassID("GOMP"), () => new GOMP() },
                { new ClassID("STIO"), () => new STIO() },
                { new ClassID("TBXB"), () => new TBXB() },
                { new ClassID("TGOB"), () => new TGOB() },
                { new ClassID("WOKS"), () => new WOKS() },
                { new ClassID("KWA"), () => new KWA() },
                { new ClassID("GVID"), () => new GVID() },
                { new ClassID("GVDS"), () => new GVDS() },
                { new ClassID("GVDW"), () => new GVDW() },
                { new ClassID("MSQ"), () => new MSQ() },
                { new ClassID("TATR"), () => new TATR() },
                { new ClassID("CODC"), () => new CODC() },
                { new ClassID("KCDC"), () => new KCDC() },
                { new ClassID("CODM"), () => new CODM() },
                { new ClassID("COST"), () => new COST() },
                { new ClassID("DTE"), () => new DTE() },
                { new ClassID("ERS"), () => new ERS() },
                { new ClassID("FNE"), () => new FNE() },
                { new ClassID("FNET"), () => new FNET() },
                { new ClassID("FNI"), () => new FNI() },
                { new ClassID("GNV"), () => new GNV() },
                { new ClassID("GORP"), () => new GORP() },
                { new ClassID("GORB"), () => new GORB() },
                { new ClassID("GORF"), () => new GORF() },
                { new ClassID("GORT"), () => new GORT() },
                { new ClassID("GORV"), () => new GORV() },
                { new ClassID("GPT"), () => new GPT() },
                { new ClassID("GRPB"), () => new GRPB() },
                { new ClassID("GGB"), () => new GGB() },
                { new ClassID("GG"), () => new GG() },
                { new ClassID("DLG"), () => new DLG() },
                { new ClassID("GLB"), () => new GLB() },
                { new ClassID("AL"), () => new AL() },
                { new ClassID("GL"), () => new GL() },
                { new ClassID("GSTB"), () => new GSTB() },
                { new ClassID("GST"), () => new GST() },
                { new ClassID("GTE"), () => new GTE() },
                { new ClassID("MIDO"), () => new MIDO() },
                { new ClassID("MISI"), () => new MISI() },
                { new ClassID("OMS"), () => new OMS() },
                { new ClassID("WMS"), () => new WMS() },
                { new ClassID("MSMX"), () => new MSMX() },
                { new ClassID("MSTP"), () => new MSTP() },
                { new ClassID("MUB"), () => new MUB() },
                { new ClassID("NTL"), () => new NTL() },
                { new ClassID("RCA"), () => new RCA() },
                { new ClassID("CRF"), () => new CRF() },
                { new ClassID("CRM"), () => new CRM() },
                { new ClassID("REGN"), () => new REGN() },
                { new ClassID("RGSC"), () => new RGSC() },
                { new ClassID("RND"), () => new RND() },
                { new ClassID("SFL"), () => new SFL() },
                { new ClassID("SCEB"), () => new SCEB() },
                { new ClassID("SCEG"), () => new SCEG() },
                { new ClassID("SCEN"), () => new SCEN() },
                { new ClassID("SMCC"), () => new SMCC() },
                { new ClassID("SNDV"), () => new SNDV() },
                { new ClassID("SNDM"), () => new SNDM() },
                { new ClassID("SNMQ"), () => new SNMQ() },
                { new ClassID("MDPS"), () => new MDPS() },
                { new ClassID("SDAM"), () => new SDAM() },
                { new ClassID("SNQU"), () => new SNQU() },
                { new ClassID("AMQU"), () => new AMQU() },
                { new ClassID("MSQU"), () => new MSQU() },
                { new ClassID("SREC"), () => new SREC() },
                { new ClassID("SSCB"), () => new SSCB() },
                { new ClassID("STRG"), () => new STRG() },
                { new ClassID("TAGL"), () => new TAGL() },
                { new ClassID("TAGM"), () => new TAGM() },
                { new ClassID("UNDB"), () => new UNDB() },
                { new ClassID("MUNB"), () => new MUNB() },
                { new ClassID("AUND"), () => new AUND() },
                { new ClassID("MUNS"), () => new MUNS() },
                { new ClassID("SUNA"), () => new SUNA() },
                { new ClassID("SUNC"), () => new SUNC() },
                { new ClassID("SUNK"), () => new SUNK() },
                { new ClassID("SUNP"), () => new SUNP() },
                { new ClassID("SUNS"), () => new SUNS() },
                { new ClassID("SUNX"), () => new SUNX() },
                { new ClassID("TUNC"), () => new TUNC() },
                { new ClassID("TUND"), () => new TUND() },
                { new ClassID("TUNH"), () => new TUNH() },
                { new ClassID("TUNS"), () => new TUNS() },
                { new ClassID("TUNT"), () => new TUNT() },
                { new ClassID("RTUN"), () => new RTUN() },
                { new ClassID("USAC"), () => new USAC() },
            };
        }

        public static NativeObject Create(ClassID classID)
        {
            return factories[classID].Invoke();
        }
    }
}
