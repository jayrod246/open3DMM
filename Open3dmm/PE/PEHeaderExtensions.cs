using PE;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    public static class PEHeaderExtensions
    {
        public static IMAGE_IMPORT_DESCRIPTOR[] GetImportDescriptors(this PeHeader hdr, IntPtr baseAddress)
        {
            var importTable = hdr.OptionalHeader32.ImportTable;
            var addr = (int)importTable.VirtualAddress;
            var list = new List<IMAGE_IMPORT_DESCRIPTOR>();
            while (true)
            {
                var importDesc = Marshal.PtrToStructure<IMAGE_IMPORT_DESCRIPTOR>(baseAddress + addr);
                if (importDesc == IMAGE_IMPORT_DESCRIPTOR.Empty)
                    break;
                list.Add(importDesc);
                addr += 20;
            }

            return list.ToArray();
        }
    }
}
