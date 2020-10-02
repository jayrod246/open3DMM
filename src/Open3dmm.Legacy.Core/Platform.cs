using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Open3dmm.Core
{
    internal class WindowsPlatform : Platform
    {
        private static readonly string launchDirectory = Environment.CurrentDirectory;

        public override bool TryFind3dmm(out string searchDirectory, out string searchSubDirectory, out string exePath)
        {
            searchDirectory = launchDirectory;
            searchSubDirectory = "3D Movie Maker\\";
            if (!CheckFor3dmmExe(searchDirectory, searchSubDirectory, out exePath))
            {
                bool querySuccess = WinRegistry.QueryValue(RegistryHives.HKLM, "Software\\Microsoft\\Microsoft Kids\\3D Movie Maker\\InstallDirectory", out searchDirectory)
                                 && WinRegistry.QueryValue(RegistryHives.HKLM, "Software\\Microsoft\\Microsoft Kids\\3D Movie Maker\\InstallSubDir", out searchSubDirectory);
                if (!querySuccess || !CheckFor3dmmExe(searchDirectory, searchSubDirectory, out exePath))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CheckFor3dmmExe(string searchDirectory, string searchSubDirectory, out string path)
        {
            var success = File.Exists(path = Path.Combine(searchDirectory, searchSubDirectory, "3DMOVIE.EXE"));
#if DEBUG
            Console.WriteLine($"Searching: \"{path}\" - {(success ? "Success" : "Failed")}");
#endif
            return success;
        }

        public enum RegistryHives : int
        {
            HKCR = -2147483648,
            HKCC = -2147483643,
            HKCU = -2147483647,
            HKDD = -2147483642,
            HKLM = -2147483646,
            HKPD = -2147483644,
            HKU = -2147483645
        }

        public enum RegistryValueKind
        {
            String = 1,
            ExpandString = 2,
            Binary = 3,
            DWord = 4,
            MultiString = 7,
            QWord = 11,
            None = 0
        }

        private static class WinRegistry
        {
            private static Dictionary<string, IntPtr> handles = new Dictionary<string, IntPtr>();

            [DllImport("ADVAPI32", SetLastError = true)]
            static extern IntPtr RegQueryValueExA(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind lpType, byte[] lpData, ref int lpcbData);

            [DllImport("ADVAPI32", SetLastError = true)]
            static extern unsafe IntPtr RegQueryValueExA(IntPtr hKey, string lpValueName, IntPtr lpReserved, out RegistryValueKind lpType, void* lpData, ref int lpcbData);

            [DllImport("ADVAPI32", SetLastError = true)]
            static extern IntPtr RegOpenKeyEx(IntPtr hKey, string lpSubKey, int ulOptions, int samDesired, out IntPtr phkResult);

            public static bool QueryValue(RegistryHives hive, string valueName, out int value)
            {
                return MarshalValue(hive, valueName, (p, n) => Marshal.ReadInt32(p), out value);
            }

            public static bool QueryValue(RegistryHives hive, string valueName, out string str)
            {
                return MarshalValue(hive, valueName, (p, n) => Marshal.PtrToStringAnsi(p, n - 1), out str);
            }

            static readonly IntPtr ERROR_MORE_DATA = new IntPtr(0xEA);

            private static bool MarshalValue<T>(RegistryHives hive, string valueName, Func<IntPtr, int, T> marshaller, out T value)
            {
                var subKey = Path.GetDirectoryName(valueName);
                var hKey = OpenKey(hive, subKey);
                var name = Path.GetFileName(valueName);
                int size = 0;

                if (hKey != IntPtr.Zero && RegQueryValueExA(hKey, name, IntPtr.Zero, out _, Array.Empty<byte>(), ref size) == ERROR_MORE_DATA)
                {
                    unsafe
                    {
                        void* st = stackalloc byte[size];
                        if (RegQueryValueExA(hKey, name, IntPtr.Zero, out _, st, ref size) == IntPtr.Zero)
                        {
                            value = marshaller(new IntPtr(st), size);
                            return true;
                        }
                    }
                }
                value = default;
                return false;
            }

            private static IntPtr OpenKey(RegistryHives hive, string subKey)
            {
                if (!handles.TryGetValue(subKey, out var handle))
                {
                    if (RegOpenKeyEx((IntPtr)hive, subKey, 0, 0x20019, out handle) != IntPtr.Zero)
                        return IntPtr.Zero;
                    handles[subKey] = handle;
                }
                return handle;
            }
        }
    }

    public abstract class Platform
    {
        public static Platform Current { get; } = new WindowsPlatform();

        public abstract bool TryFind3dmm(out string searchDirectory, out string searchSubDirectory, out string exePath);

    }
}
