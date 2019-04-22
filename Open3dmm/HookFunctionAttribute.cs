using System;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class HookFunctionAttribute : Attribute
    {
        public CallingConvention CallingConvention { get; set; } = CallingConvention.StdCall;
        public IntPtr HookAddress { get => this.hookAddress; }

        private readonly IntPtr hookAddress;

        public HookFunctionAttribute(IntPtr hookAddress)
        {
            this.hookAddress = hookAddress;
        }

        public HookFunctionAttribute(int hookAddress) : this(new IntPtr(hookAddress))
        {
        }

        public HookFunctionAttribute(FunctionNames functionName) : this((IntPtr)functionName)
        {
        }
    }
}
