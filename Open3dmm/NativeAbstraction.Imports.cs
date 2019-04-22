using Open3dmm.WinApi;
using PE;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    partial class NativeAbstraction
    {
        static partial void LoadImports()
        {
            var hdr = GetPEHeader();
            foreach (var importDesc in hdr.GetImportDescriptors(ModuleHandle))
            {
                var dllName = Marshal.PtrToStringAnsi(ModuleHandle + importDesc.Name.ToInt32());
                var hmodule = Kernel32.LoadLibrary(dllName);
                if (hmodule == IntPtr.Zero)
                    throw new InvalidOperationException($"Library could not be loaded: {dllName}");
                var addr = (int)importDesc.OriginalFirstThunk;
                var linkAddr = (int)importDesc.FirstThunk;
                while (true)
                {
                    // Get procedure name
                    var importByNameOffset = Marshal.ReadInt32(ModuleHandle + addr);
                    if (importByNameOffset == 0)
                        break; // End
                    var importByName = Marshal.PtrToStructure<IMAGE_IMPORT_BY_NAME>(ModuleHandle + importByNameOffset);
                    var procName = Marshal.PtrToStringAnsi(ModuleHandle + importByNameOffset + 2);
                    // Get procedure address
                    var procAddr = Kernel32.GetProcAddress(hmodule, procName);
                    if (procAddr == IntPtr.Zero)
                        throw new InvalidOperationException($"Couldn't find procedure: {dllName}::{procName}");
                    Marshal.WriteIntPtr(ModuleHandle + linkAddr, procAddr);
                    // Procedure is now linked
                    addr += IntPtr.Size;
                    linkAddr += IntPtr.Size;
                }
            }
        }

        private unsafe static PeHeader GetPEHeader()
        {
            return PeHeader.ReadFrom(new UnmanagedMemoryStream((byte*)ModuleHandle, 4194304));
        }
    }
}
