using Open3dmm.WinApi;
using System;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    internal static partial class NativeAbstraction
    {
        public static readonly IntPtr ModuleHandle = (IntPtr)0x00400000;

        static NativeAbstraction()
        {
            var handle = Kernel32.GetModuleHandle("3dmovie.exe");
            if (ModuleHandle != handle)
            {
                Console.WriteLine($"3dmovie.exe was loaded at the wrong address {handle}, aborting.");
                Environment.Exit(1);
            }

            LoadImports();
            ApplyPatches();
            SetWindowsApiHooks();
            SetNativeHooks();
            InitMethods();
        }

        static partial void LoadImports();
        static partial void ApplyPatches();
        static partial void SetWindowsApiHooks();
        static unsafe partial void SetNativeHooks();
        static partial void InitMethods();
        public static IntPtr AddressOfFunction(FunctionNames functionName)
        {
            return (IntPtr)functionName;
        }

        public static string GetFunctionName(IntPtr addressOfFunction)
        {
            return Enum.GetName(typeof(FunctionNames), (int)addressOfFunction);
        }
    }
}
