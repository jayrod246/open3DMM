using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Open3dmm.WinApi
{
    public delegate TDelegate DetourCallback<TDelegate>(TDelegate originalFunction) where TDelegate : Delegate;
    public static class ApiDetour
    {
        static Dictionary<IntPtr, (Delegate original, Delegate hook)> hookedFunctions = new Dictionary<IntPtr, (Delegate original, Delegate hook)>();

        static byte[] restoreBuf = new byte[]
        {
            0x8B, 0xFF
        };

        static byte[] hookStub = new byte[]
        {
            0xE9, 0x00, 0x00, 0x00, 0x00,
            0xEB, 0xF9
        };

        static int[] intBuf = new int[1];

        public static TDelegate Hook<TDelegate>(string moduleName, string functionName, DetourCallback<TDelegate> detourCallback) where TDelegate : Delegate
        {
            try
            {
                return Hook(ModuleFunctionPairToPointer(moduleName, functionName), detourCallback);
            }
            catch (InvalidOperationException e)
            {
                e.Data["ModuleName"] = moduleName;
                e.Data["FunctionName"] = functionName;
                throw;
            }
        }

        private static IntPtr ModuleFunctionPairToPointer(string moduleName, string functionName)
        {
            return Kernel32.GetProcAddress(Kernel32.GetModuleHandle(moduleName), functionName);
        }

        public static TDelegate Hook<TDelegate>(IntPtr originalFunctionPointer, DetourCallback<TDelegate> detourCallback) where TDelegate : Delegate
        {
            if (!TryHook(originalFunctionPointer, detourCallback, out var hook))
                throw new InvalidOperationException($"Detour could not be assigned at address: {originalFunctionPointer}");
            return hook;
        }

        public static bool TryHook<TDelegate>(string moduleName, string functionName, DetourCallback<TDelegate> detourCallback, out TDelegate hook) where TDelegate : Delegate
        {
            return TryHook(ModuleFunctionPairToPointer(moduleName, functionName), detourCallback, out hook);
        }

        public static bool TryHook<TDelegate>(IntPtr originalFunctionPointer, DetourCallback<TDelegate> detourCallback, out TDelegate hook) where TDelegate : Delegate
        {
            hook = default;
            if (originalFunctionPointer.ToInt32() < 6)
                return false;

            if (hookedFunctions.ContainsKey(originalFunctionPointer))
                return false;

            TDelegate originalFunction;
            if (Marshal.ReadInt16(originalFunctionPointer) == 0x25FF)
            {
                originalFunction = Marshal.GetDelegateForFunctionPointer<TDelegate>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(originalFunctionPointer, 2)));
            }
            else
            {
                originalFunction = Marshal.GetDelegateForFunctionPointer<TDelegate>(originalFunctionPointer + 2);
            }
            hook = detourCallback.Invoke(originalFunction);
            hookedFunctions.Add(originalFunctionPointer, (originalFunction, hook));
            var hookPointer = Marshal.GetFunctionPointerForDelegate(hook);
            intBuf[0] = (int)hookPointer - (int)(originalFunctionPointer);
            Buffer.BlockCopy(intBuf, 0, hookStub, 1, 4);
            Kernel32.WriteProcessMemory(Process.GetCurrentProcess().Handle, originalFunctionPointer - 5, hookStub, 7, out _);
            return true;
        }

        public static bool TryGetHook<TDelegate>(string moduleName, string functionName, out TDelegate hook) where TDelegate : Delegate
        {
            if (hookedFunctions.TryGetValue(ModuleFunctionPairToPointer(moduleName, functionName), out var hookedFunction))
            {
                hook = (TDelegate)hookedFunction.hook;
                return true;
            }
            hook = default;
            return false;
        }

        public static bool TryGetOriginalFunction<TDelegate>(string moduleName, string functionName, out TDelegate originalFunction) where TDelegate : Delegate
        {
            if (hookedFunctions.TryGetValue(ModuleFunctionPairToPointer(moduleName, functionName), out var hookedFunction))
            {
                originalFunction = (TDelegate)hookedFunction.original;
                return true;
            }
            originalFunction = default;
            return false;
        }

        public static bool RemoveHook(IntPtr originalFunctionPointer)
        {
            if (hookedFunctions.Remove(originalFunctionPointer))
            {
                Kernel32.WriteProcessMemory(Process.GetCurrentProcess().Handle, originalFunctionPointer, restoreBuf, 2, out _);
                return true;
            }
            return false;
        }
        public static bool RemoveHook(string moduleName, string functionName)
        {
            return RemoveHook(ModuleFunctionPairToPointer(moduleName, functionName));
        }
    }
}
