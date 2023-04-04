#--- $(SOC_ROOT)\makefile.mak

!INCLUDE $(KAUAI_ROOT)\makefile.def

!INCLUDE $(SOC_ROOT)\version.def


.SILENT:

ALL: ENSURE_OBJ_DIR ALL_BREN ALL_SRC ALL_TOOLS


ALL_BREN:
    cd $(SOC_ROOT)\bren
    @echo Making Bren All...
    $(MAKE) /NOLOGO all
    cd $(SOC_ROOT)


ALL_SRC:
    cd $(SOC_ROOT)\src
    @echo Making Soc\src All...
    $(MAKE) /NOLOGO all
    cd $(SOC_ROOT)


ALL_TOOLS:
    cd $(SOC_ROOT)\tools
    @echo Making Soc\tools All...
    $(MAKE) /NOLOGO all
    cd $(SOC_ROOT)

SOC_OBJ_ROOT_DIR = $(SOC_ROOT)\obj
SOC_OBJ_DIR = $(SOC_OBJ_ROOT_DIR)\$(BLD_TYPE_DIR)
DIST_ROOT = $(SOC_ROOT)\DIST

ENSURE_OBJ_DIR:
    if not exist $(SOC_OBJ_ROOT_DIR)/nul mkdir $(SOC_OBJ_ROOT_DIR)
    if not exist $(SOC_OBJ_DIR)/nul mkdir $(SOC_OBJ_DIR)

DIST: ALL_SRC
    -mkdir $(DIST_ROOT) 2>nul
    -mkdir $(DIST_ROOT)\"Microsoft Kids"
    -mkdir $(DIST_ROOT)\"Microsoft Kids"\"3D Movie Maker"
    -mkdir $(DIST_ROOT)\"Microsoft Kids"\Users"
    -mkdir $(DIST_ROOT)\"Microsoft Kids"\Users\Melanie"
    copy /Y $(SOC_ROOT)\cd9\*.* $(DIST_ROOT)\"Microsoft Kids"\"3D Movie Maker"
    copy /Y $(SOC_OBJ_DIR)\*.chk $(DIST_ROOT)\"Microsoft Kids"\"3D Movie Maker"
    copy /Y $(SOC_OBJ_DIR)\3dmovie.exe $(DIST_ROOT)\

ZIP: DIST
    cd $(DIST_ROOT)
    7z a -r 3DMMForever_$(RELEASE_VERSION).zip 3dmovie.exe "Microsoft Kids"

CLEAN: CLEAN_REST CLEAN_BREN BUILD_REST


CLEAN_BREN:
    cd $(SOC_ROOT)\bren
    @echo Making Bren Clean...
    $(MAKE) /NOLOGO clean
    cd $(SOC_ROOT)


CLEAN_REST:
    @echo Cleaning $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\ directory
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.obj 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.pch 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.pdb 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.exe 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.res 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.chk 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.cht 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.cod 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.map 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.thd 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.lib 2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.i   2>nul
    del /q $(SOC_ROOT)\obj\$(BLD_TYPE_DIR)\*.lnk 2>nul


BUILD_REST:
    cd $(SOC_ROOT)\src
    @echo Making Src Clean...
    $(MAKE) /NOLOGO
    cd $(SOC_ROOT)\tools
    @echo Making Tools Clean...
    $(MAKE) /NOLOGO
    cd $(SOC_ROOT)

