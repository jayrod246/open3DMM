using Microsoft.Xna.Framework;
using Open3dmm.Classes;
using Open3dmm.WinApi;
using System;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    partial class NativeAbstraction
    {
        static unsafe partial void SetNativeHooks()
        {
            InitRectangleHooks();
            InitGPTHooks();

            Hook.Create<ThisCall0>(AddressOfFunction(FunctionNames.BWLD_Render), ctx =>
            {
                return (ecx) =>
                {
                    var bwld = NativeObject.FromPointer<BWLD>(ecx);
                    ctx.CallOriginal(o => o(ecx));
                    bwld.RenderOne();
                    return new IntPtr(1);
                };
            }).Initialize();

            Hook.Create<StdCall1>(AddressOfFunction(FunctionNames.Malloc), ctx =>
            {
                return (size) =>
                {
                    return NativeHandle.Alloc((int)size).Address;
                };
            }).Initialize();

            Hook.Create<StdCall1>(AddressOfFunction(FunctionNames.Free), ctx =>
            {
                return (address) =>
                {
                    NativeHandle.Free(address);
                    return new IntPtr(1);
                };
            }).Initialize();

            Hook.Create<StdCall0>(AddressOfFunction(FunctionNames.__WinMainCRTStartup), ctx =>
            {
                return () =>
                {
                    GameTimer = new GameTimer();
                    var envStrings = PInvoke.Call(LibraryNames.KERNEL32, "GetEnvironmentStrings");
                    Marshal.WriteIntPtr((IntPtr)0x004EA1A4, envStrings);
                    UnmanagedFunctionCall.StdCall(AddressOfFunction(FunctionNames.__cinit));
                    UnmanagedFunctionCall.StdCall(AddressOfFunction(FunctionNames.WinMain), ModuleHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    return IntPtr.Zero;
                };
            }).Initialize();
        }

        #region Rectangle
        private static unsafe void InitRectangleHooks()
        {
            Hook.Create<ThisCall2>(AddressOfFunction(FunctionNames.Rectangle_CalculateIntersectionBetween), ctx =>
            {
                return (ecx, arg1, arg2) =>
                {
                    return ctx.ToIntPtr(((RECTANGLE*)ecx)->CalculateIntersection(in *(RECTANGLE*)arg1, in *(RECTANGLE*)arg2));
                };
            }).Initialize();

            Hook.Create<ThisCall1>(AddressOfFunction(FunctionNames.Rectangle_CalculateIntersection), ctx =>
            {
                return (ecx, arg1) =>
                {
                    return ctx.ToIntPtr(((RECTANGLE*)ecx)->CalculateIntersection(in *(RECTANGLE*)arg1));
                };
            }).Initialize();

            Hook.Create<ThisCall1>(AddressOfFunction(FunctionNames.Rectangle_SizeLimit), ctx =>
            {
                return (ecx, arg) =>
                {
                    fixed (RECTANGLE* result = &((RECTANGLE*)ecx)->SizeLimit(ref *(RECTANGLE*)arg))
                        return new IntPtr(result);
                };
            }).Initialize();

            Hook.Create<ThisCall1>(AddressOfFunction(FunctionNames.Rectangle_method00419F30), ctx =>
            {
                return (ecx, arg) =>
                {
                    ((RECTANGLE*)ecx)->Union(in *(RECTANGLE*)arg);
                    return IntPtr.Zero;
                };
            });//.Initialize();

            Hook.Create<ThisCall1>(AddressOfFunction(FunctionNames.Rectangle_Union), ctx =>
            {
                return (ecx, arg) =>
                {
                    ((RECTANGLE*)ecx)->Union(in *(RECTANGLE*)arg);
                    return IntPtr.Zero;
                };
            }).Initialize();

            Hook.Create<ThisCall3>(AddressOfFunction(FunctionNames.Rectangle_CopyAtOffset), ctx =>
            {
                return (ecx, src, x, y) =>
                {
                    ((RECTANGLE*)ecx)->Copy(in *(RECTANGLE*)src, x.ToInt32(), y.ToInt32());
                    return IntPtr.Zero;
                };
            }).Initialize();

            Hook.Create<ThisCall2>(AddressOfFunction(FunctionNames.Rectangle_Translate), ctx =>
            {
                return (ecx, x, y) =>
                {
                    ((RECTANGLE*)ecx)->Translate(x.ToInt32(), y.ToInt32());
                    return IntPtr.Zero;
                };
            }).Initialize();

            Hook.Create<ThisCall0>(AddressOfFunction(FunctionNames.Rectangle_TopLeftOrigin), ctx =>
            {
                return (ecx) =>
                {
                    ((RECTANGLE*)ecx)->TopLeftOrigin();
                    return IntPtr.Zero;
                };
            }).Initialize();

            Hook.Create<ThisCall2>(AddressOfFunction(FunctionNames.Rectangle_HitTest), ctx =>
            {
                return (ecx, arg1, arg2) =>
                {
                    return ctx.ToIntPtr(((RECTANGLE*)ecx)->HitTest(arg1.ToInt32(), arg2.ToInt32()));
                };
            }).Initialize();

            Hook.Create<ThisCall1>(AddressOfFunction(FunctionNames.Rectangle_Copy), ctx =>
            {
                return (ecx, arg1) =>
                {
                    ((RECTANGLE*)ecx)->Copy(in *(RECTANGLE*)arg1);
                    return IntPtr.Zero;
                };
            }).Initialize();
        }
        #endregion

        #region GPT

        private static unsafe void InitGPTHooks()
        {
            Hook.Create<ThisCall3>(AddressOfFunction(FunctionNames.GPT__BlitMBMP), ctx =>
            {
                return (ecx, arg1, arg2, arg3) =>
                {
                    var gpt = NativeObject.FromPointer<GPT>(ecx);
                    var mbmp = NativeObject.FromPointer<MBMP>(arg1);
                    gpt.BlitMBMP(mbmp, *(RECTANGLE*)arg2, (GPT_UnkStruct1*)arg3);
                    return IntPtr.Zero;
                };
            }).Initialize();
        }

        #endregion
    }
}