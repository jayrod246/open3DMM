using Open3dmm.WinApi;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    public class Program
    {
        static int Main(string[] args)
        {
            NativeAbstraction.WinMainCRTStartup();
            return 0;
        }

        public static int Bootstrap(string pwzArgument)
        {
            var argStr = Marshal.PtrToStringUni(PInvoke.Call(LibraryNames.KERNEL32, "GetCommandLineW"));
            return Main(argStr.Substring(argStr.IndexOf(' ') + 1).Split(' '));
        }
    }
}
