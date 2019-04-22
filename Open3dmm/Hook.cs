using Open3dmm.WinApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    public interface IHook
    {
        void Initialize();
    }

    abstract unsafe class HookBase<TDelegate> : IHook
    {
        private readonly IntPtr functionPointer;

        public TDelegate FunctionDelegate { get; private set; }

        private bool isInitialized;
        private long restore;

        protected HookBase(IntPtr functionPointer)
        {
            this.functionPointer = functionPointer;
        }

        public void Initialize()
        {
            if (isInitialized)
                throw new InvalidOperationException("Hook is already initialized");
            FunctionDelegate = GetDelegate();
            long b = 0;
            Buffer.MemoryCopy(functionPointer.ToPointer(), &b, 8, 5);
            restore = b;
            isInitialized = true;
            CallOriginalPost();
        }

        public void CallOriginalPre()
        {
            if (!isInitialized)
                throw new InvalidOperationException("Hook is not initialized");
            var hproc = Process.GetCurrentProcess().Handle;
            Kernel32.WriteProcessMemory(hproc, this.functionPointer, restore, 5, out _);
        }

        public void CallOriginalPost()
        {
            if (!isInitialized)
                throw new InvalidOperationException("Hook is not initialized");
            var hproc = Process.GetCurrentProcess().Handle;
            const byte JMP = 0xE9;
            Kernel32.WriteProcessMemory(hproc, this.functionPointer, JMP, 1, out _);
            Kernel32.WriteProcessMemory(hproc, this.functionPointer + 1, Marshal.GetFunctionPointerForDelegate(FunctionDelegate).ToInt32() - this.functionPointer.ToInt32() - 5, 4, out _);
        }

        public T CallOriginal<T>(Func<TDelegate, T> invoker)
        {
            CallOriginalPre();
            var result = invoker(Marshal.GetDelegateForFunctionPointer<TDelegate>(functionPointer));
            CallOriginalPost();
            return result;
        }

        public void CallOriginal(Action<TDelegate> invoker)
        {
            CallOriginalPre();
            invoker(Marshal.GetDelegateForFunctionPointer<TDelegate>(functionPointer));
            CallOriginalPost();
        }

        protected abstract TDelegate GetDelegate();
    }
    public class HookContext<TDelegate> where TDelegate : Delegate
    {
        private HookBase<TDelegate> hook;

        internal HookContext(HookBase<TDelegate> hook)
        {
            this.hook = hook;
        }

        public T CallOriginal<T>(Func<TDelegate, T> invoker)
        {
            return hook.CallOriginal(invoker);
        }

        public void CallOriginal(Action<TDelegate> invoker)
        {
            hook.CallOriginal(invoker);
        }

        public IntPtr ToIntPtr(in bool value)
        {
            return value ? new IntPtr(1) : IntPtr.Zero;
        }

        public IHook Hook => hook;
    }
    public static class Hook
    {
        static List<IHook> trackers = new List<IHook>();

        public static IHook Create<TDelegate>(IntPtr functionPointer, Func<HookContext<TDelegate>, TDelegate> getCallback) where TDelegate : Delegate
        {
            var hook = new DelegateHook<TDelegate>(functionPointer, getCallback);
            trackers.Add(hook);
            return hook;
        }

        private class DelegateHook<TDelegate> : HookBase<TDelegate> where TDelegate : Delegate
        {
            private readonly Func<HookContext<TDelegate>, TDelegate> getCallback;

            public DelegateHook(IntPtr functionPointer, Func<HookContext<TDelegate>, TDelegate> getCallback) : base(functionPointer)
            {
                this.getCallback = getCallback;
            }

            protected override TDelegate GetDelegate()
            {
                return getCallback(new HookContext<TDelegate>(this));
            }
        }
    }
}
