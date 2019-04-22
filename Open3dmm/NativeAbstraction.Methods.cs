using Open3dmm.WinApi;
using PE;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    partial class NativeAbstraction
    {
        public static StdCall0 WinMainCRTStartup;

        static partial void InitMethods()
        {
            WinMainCRTStartup = Marshal.GetDelegateForFunctionPointer<StdCall0>(AddressOfFunction(FunctionNames.__WinMainCRTStartup));
        }
    }
}
