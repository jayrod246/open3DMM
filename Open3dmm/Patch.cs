using Open3dmm.WinApi;
using System;
using System.Diagnostics;

namespace Open3dmm
{
    public abstract class Patch
    {
        public abstract bool ApplyPatch();

        protected bool ReplaceMemory(int address, byte[] source)
        {
            return Kernel32.WriteProcessMemory(Process.GetCurrentProcess().Handle, (IntPtr)address, source, source.Length, out _);
        }
    }
}
