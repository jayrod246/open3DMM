using Open3dmm.Core;
using Open3dmm.Core.Data;
using Open3dmm.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using static Open3dmm.OpCodeValues;

namespace Open3dmm
{
    public unsafe sealed class ScriptEngine
    {
        public unsafe class VirtualStack
        {
            private int* _sp;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(int* sp)
            {
                _sp = sp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RotateStack(int n, int amount)
            {
                if (n > 1)
                {
                    amount %= n;
                    if (amount < 0)
                        amount += n;
                    if (amount != 0)
                    {
                        ReverseStack(amount);
                        ReverseStack(n - amount);
                        ReverseStack(n);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ReverseStack(int n)
            {
                if (n > 1)
                    new Span<int>(_sp - n, n).Reverse();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Peek()
            {
                return _sp[-1];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Pop()
            {
                return _sp--[-1];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int SelectList(int n, int i)
            {
                var value = _sp[-1 - i];
                PopList(n);
                return value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void PopList(int n)
            {
                _sp -= n;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<int> PopList(Span<int> dest)
            {
                var n = dest.Length;
                new Span<int>(_sp -= n, n).CopyTo(dest);
                dest.Reverse();
                return dest;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Push(int value)
            {
                _sp++[0] = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DupList(int n)
            {
                new Span<int>(_sp - n, n).CopyTo(new(_sp, n));
                _sp += n;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int ComputeStackPointer(int* stackBase)
            {
                return (int)(_sp - stackBase);
            }
        }

        public unsafe class OpCodeFunctionPointers
        {
            private Dictionary<OpCodeValues, nuint> _functionPointers = new();

            public delegate* managed<ScriptEngine, void> this[OpCodeValues opCode]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (delegate* managed<ScriptEngine, void>)_functionPointers[opCode];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _functionPointers[opCode] = (nuint)value;
            }
        }

        private static readonly OpCodeFunctionPointers s_functionPointers = new()
        {
            [Exit] = &OnExit,
            [Pause] = &OnPause,
            [Add] = &OnAdd,
            [Sub] = &OnSub,
            [Mul] = &OnMul,
            [Div] = &OnDiv,
            [Mod] = &OnMod,
            [Neg] = &OnNeg,
            [Inc] = &OnInc,
            [Dec] = &OnDec,
            [Shr] = &OnBRShft,
            [Shl] = &OnBLShft,
            [BOr] = &OnBOr,
            [BAnd] = &OnBAnd,
            [BXor] = &OnBXor,
            [BNot] = &OnBNot,
            [LXor] = &OnLXor,
            [LNot] = &OnLNot,
            [Eq] = &OnEq,
            [Ne] = &OnNe,
            [Gt] = &OnGt,
            [Lt] = &OnLt,
            [Ge] = &OnGe,
            [Le] = &OnLe,
            [Abs] = &OnAbs,
            [Rnd] = &OnRnd,
            [MulDiv] = &OnMulDiv,
            [Dup] = &OnDup,
            [Swap] = &OnSwap,
            [Rot] = &OnRot,
            [Rev] = &OnRev,
            [DupList] = &OnDupList,
            [PopList] = &OnPopList,
            [RndList] = &OnRndList,
            [Select] = &OnSelect,
            [GoEq] = &OnGoEq,
            [GoNe] = &OnGoNe,
            [GoGt] = &OnGoGt,
            [GoLt] = &OnGoLt,
            [GoGe] = &OnGoGe,
            [GoLe] = &OnGoLe,
            [GoZ] = &OnGoZ,
            [GoNz] = &OnGoNz,
            [Go] = &OnGo,
            [Return] = &OnReturn,
            [SetReturn] = &OnSetReturn,
            [Pop] = &OnPop,
            [NumToStr] = &OnNumToStr,
            [CopyStr] = &OnCopyStr,
            [CopySubStr] = &OnCopySubStr,
            [LenStr] = &OnLenStr,
            [ConcatStrs] = &OnConcatStrs,
            [Match] = &OnMatch,
            [CreateChildGob] = &OnCreateChildGob,
            [CreateChildThis] = &OnCreateChildThis,
            [RunScriptGob] = &OnRunScriptGob,
            [RunScriptThis] = &OnRunScriptThis,
            [RunScriptCnoGob] = &OnRunScriptCnoGob,
            [RunScriptCnoThis] = &OnRunScriptCnoThis,
            [SetProp] = &OnSetProp,
            [GetProp] = &OnGetProp,
            [FilterCmdsGob] = &OnFilterCmdsGob,
            [FilterCmdsThis] = &OnFilterCmdsThis,
            [EnqueueCid] = &OnEnqueueCid,
            [GidParGob] = &OnGidParGob,
            [GidParThis] = &OnGidParThis,
            [FlushUserEvents] = &OnFlushUserEvents,
            [EndLongOp] = &OnEndLongOp,
            [SetColorTable] = &OnSetColorTable,
            [CreateHelpGob] = &OnCreateHelpGob,
            [CreateHelpThis] = &OnCreateHelpThis,
            [PlaySoundGob] = &OnPlaySoundGob,
            [PlaySoundThis] = &OnPlaySoundThis,
            [ChangeStateGob] = &OnChangeStateGob,
            [ChangeStateThis] = &OnChangeStateThis,
            [Cell] = &OnCell,
            [CellNoPause] = &OnCellNoPause,
            [SetNoSlipGob] = &OnSetNoSlipGob,
            [SetNoSlipThis] = &OnSetNoSlipThis,
            [StopSound] = &OnStopSound,
            [DestroyGob] = &OnDestroyGob,
            [DestroyThis] = &OnDestroyThis,
            [StartLongOp] = &OnStartLongOp,
            [FGobExists] = &OnFGobExists,
            [PlayingGob] = &OnPlayingGob,
            [PlayingThis] = &OnPlayingThis,
            [SetRepGob] = &OnSetRepGob,
            [SetRepThis] = &OnSetRepThis,
            [Transition] = &OnTransition,
            [StateGob] = &OnStateGob,
            [StateThis] = &OnStateThis,
            [GetMasterVolume] = &OnGetMasterVolume,
            [MoveAbsGob] = &OnMoveAbsGob,
            [MoveAbsThis] = &OnMoveAbsThis,
            [SetZGob] = &OnSetZGob,
            [SetZThis] = &OnSetZThis,
            [StopSoundClass] = &OnStopSoundClass,
            [GidThis] = &OnGidThis,
            [XGob] = &OnXGob,
            [YGob] = &OnYGob,
            [XThis] = &OnXThis,
            [YThis] = &OnYThis,
            [WidthGob] = &OnWidthGob,
            [HeightGob] = &OnHeightGob,
            [WidthThis] = &OnWidthThis,
            [HeightThis] = &OnHeightThis,
            [XMouseGob] = &OnXMouseGob,
            [YMouseGob] = &OnYMouseGob,
            [XMouseThis] = &OnXMouseThis,
            [YMouseThis] = &OnYMouseThis,
            [GetModifierState] = &OnGetModifierState,
            [FIsDescendent] = &OnFIsDescendent,
            [SetMasterVolume] = &OnSetMasterVolume,
        };

        private Gob _target;
        private readonly int _gid;
        private readonly IResolver resolver;
        private readonly Woks owner;
        private int _stackPointer;
        private readonly int[] _stackArray = new int[128];
        private readonly VirtualStack _stack = new();
        private readonly ParameterList _parameters;
        private readonly IDictionary<int, string> loadedStrings;

        public ScriptEngine(Woks owner, IResolver resolver, Gob target)
        {
            this.owner = owner;
            this.resolver = resolver;
            _target = target;
            _gid = _target.Id;
            target.Disposed += _ => _target = null;
            _parameters = new ParameterList();
            loadedStrings = new Dictionary<int, string>();
        }

        private Script script;
        private bool _isPaused;
        private int current;
        private int returnValue;
        private uint nextStrKey;

        public bool Run(Script script, out int result, out bool isPaused) => Run(script, ReadOnlySpan<int>.Empty, out result, out isPaused);

        public bool Run(Script script, ReadOnlySpan<int> parameters, out int result, out bool isPaused)
        {
            result = default;
            isPaused = default;
            return Load(script, parameters) && Run(out result, out isPaused);
        }

        public bool Load(Script script) => Load(script, ReadOnlySpan<int>.Empty);

        public bool Load(Script script, ReadOnlySpan<int> parameters)
        {
            Unload();
            if (0 < script.Instructions.Count)
            {
                this.script = script;
                if (!parameters.IsEmpty)
                    InitParameters(parameters);
                if (script.Strings != null)
                    LoadStrings(script.Strings);
                current = 1;
                _isPaused = true;
                return true;
            }

            Unload();
            return false;
        }

        private bool EvalPopOrPush(OpCodeValues opCode, ref ulong parameterId)
        {
            if (opCode is >= PushLocal_I and <= PopGob_I)
            {
                var idx = _stack.Pop();
                parameterId &= ~(ulong)0xffff0000;
                parameterId |= (uint)(idx << 16);
                opCode -= 0x7f;
            }
            ParameterList parameters;
            switch (opCode)
            {
                case PushLocal:
                    LoadArg(_parameters, parameterId);
                    break;
                case PopLocal:
                    StoreArg(_parameters, parameterId);
                    break;
                case PushThis:
                    parameters = _target.GetParameters();
                    LoadArg(parameters, parameterId);
                    break;
                case PopThis:
                    parameters = _target.GetParameters(true);
                    StoreArg(parameters, parameterId);
                    break;
                case PushGlobal:
                    parameters = owner.GetParameters();
                    LoadArg(parameters, parameterId);
                    break;
                case PopGlobal:
                    parameters = owner.GetParameters(true);
                    StoreArg(parameters, parameterId);
                    break;
                case PushGob:
                    parameters = Find(_stack.Pop()).GetParameters();
                    LoadArg(parameters, parameterId);
                    break;
                case PopGob:
                    parameters = Find(_stack.Pop()).GetParameters(true);
                    StoreArg(parameters, parameterId);
                    break;
                default: throw new ArgumentException("Bad opCode", nameof(opCode));
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StoreArg(ParameterList parameters, ulong parameterId)
        {
            parameters[parameterId] = _stack.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LoadArg(ParameterList parameters, ulong parameterId)
        {
            _stack.Push(parameters?.GetValueOrDefault(parameterId) ?? default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Gob Find(int cmhId)
        {
            if (_gid == cmhId)
                return _target;
            return owner.Find(cmhId);
        }

        public void Unload()
        {
            _stackPointer = 0;
            _parameters.Clear();
            loadedStrings.Clear();
            script = null;
            _isPaused = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LoadStrings(IList<string> strings)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                nextStrKey += 1U;
                nextStrKey |= 2147483648;
                _parameters[(ulong)i << 32] = unchecked((int)nextStrKey);
                loadedStrings.Add(unchecked((int)nextStrKey), strings[i]);
                nextStrKey++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitParameters(ReadOnlySpan<int> parameters)
        {
            _parameters.Set("_cparm", parameters.Length);
            for (int i = 0; i < parameters.Length; i++)
                _parameters.Set("_parm", parameters[i], i);
        }

        public bool Run(out int result, out bool state)
        {
            int length = script.Instructions.Count;

            if (!_isPaused)
            {
                Error();
            }
            else
            {
                fixed (int* s = _stackArray)
                {
                    _isPaused = false;
                    _stack.Reset(s + _stackPointer);
                    while (current < length)
                    {
                        if (_isPaused) break;
                        var instr = script.Instructions[current++];
                        var numArgs = instr.Args;
                        var opCode = (OpCodeValues)instr.Code;
                        int lastCurrent = current;

                        if (opCode != Push)
                        {
                            if (opCode > PopGob_I)
                            {
                                EvalOpCode(opCode);
                            }
                            else
                            {
                                if (numArgs == 0)
                                {
                                    Error();
                                    break;
                                }
                                --numArgs;
                                ulong parameterId = ((ulong)script.Instructions[current++].Literal << 32) | (uint)(instr.Literal & 0xffff);
                                lastCurrent = current;

                                EvalPopOrPush(opCode, ref parameterId);
                            }
                        }

                        if (lastCurrent == current)
                        {
                            PushStackArgs(numArgs);
                        }
                    }
                    _stackPointer = _stack.ComputeStackPointer(s);
                }
            }

            if (length <= current)
                _isPaused = false;
            if (!_isPaused)
                Unload();
            result = returnValue;
            state = _isPaused;
            return true;
        }

        private void PushStackArgs(int count)
        {
            var instructions = script.Instructions;

            while (--count >= 0)
            {
                _stack.Push(instructions[current++].Literal);
            }
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Error(Exception exception = null)
        {
            throw new InvalidOperationException("Script failure!", exception);
        }

        private unsafe void EvalOpCode(OpCodeValues opCode)
        {
            try
            {
                s_functionPointers[opCode](this);
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        private static void OnYMouseThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.YMouse(ctx._target));
        }

        private static void OnXMouseThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.XMouse(ctx._target));
        }

        private static void OnYMouseGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.YMouse(ctx.Find(ctx._stack.Pop())));
        }

        private static void OnXMouseGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.XMouse(ctx.Find(ctx._stack.Pop())));
        }

        private static void OnHeightThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetHeight(ctx._target));
        }

        private static void OnWidthThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetWidth(ctx._target));
        }

        private static void OnHeightGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetHeight(ctx.Find(ctx._stack.Pop())));
        }

        private static void OnWidthGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetWidth(ctx.Find(ctx._stack.Pop())));
        }

        private static void OnXThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetX(ctx._target));
        }

        private static void OnYThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetY(ctx._target));
        }

        private static void OnXGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetX(ctx.Find(ctx._stack.Pop())));
        }

        private static void OnYGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetY(ctx.Find(ctx._stack.Pop())));
        }

        private static void OnSetMasterVolume(ScriptEngine ctx)
        {
            Application.Current.SoundManager.MasterVolume = ctx._stack.Pop() / 65536f;
        }

        private static void OnGetModifierState(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.owner.GetState());
        }

        private static void OnFIsDescendent(ScriptEngine ctx)
        {
            var child = ctx.Find(ctx._stack.Pop());
            var parent = ctx.Find(ctx._stack.Pop());
            var result = 0;
            if (parent != null && child != null)
            {
                while (child != null && parent != child)
                    child = ctx.owner.IsAnscestorOf(child.Parent) ? child.Parent : null;
                result = Convert.ToInt32(child == parent);
            }

            ctx._stack.Push(result);
        }

        private static void OnGidThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._gid);
        }

        private static void OnStopSoundClass(ScriptEngine ctx)
        {
            ctx.StopSoundByClass(ctx._stack.Pop(), ctx._stack.Pop());
        }

        private static void OnSetZGob(ScriptEngine ctx)
        {
            ctx.SetZ(ctx.Find(ctx._stack.Pop()) as Gok, ctx._stack.Pop());
        }

        private static void OnSetZThis(ScriptEngine ctx)
        {
            ctx.SetZ(ctx._target as Gok, ctx._stack.Pop());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetZ(Gok gok, int value)
        {
            _ = gok?.SetZIndex(value);
        }

        private static void OnMoveAbsGob(ScriptEngine ctx)
        {
            ctx.MoveAbs(ctx.Find(ctx._stack.Pop()), ctx._stack.Pop(), ctx._stack.Pop());
        }

        private static void OnMoveAbsThis(ScriptEngine ctx)
        {
            ctx.MoveAbs(ctx._target, ctx._stack.Pop(), ctx._stack.Pop());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveAbs(Gob gob, int x, int y)
        {
            gob?.MoveAbs(x, y);
        }

        private static void OnGetMasterVolume(ScriptEngine ctx)
        {
            ctx._stack.Push((int)(Application.Current.SoundManager.MasterVolume * 65536));
        }

        private static void OnStateGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetState(ctx.Find(ctx._stack.Pop()) as Gok));
        }

        private static void OnStateThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetState(ctx._target as Gok));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetState(Gok gok)
        {
            return gok?.StateFlags ?? 0;
        }

        private static void OnTransition(ScriptEngine ctx)
        {
            transition(ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop());
            void transition(int unk1, int unk2, int delay, int unk3, int paletteCno)
            {
                ctx.OnSetColorTable(paletteCno);
            }
        }

        private static void OnSetRepGob(ScriptEngine ctx)
        {
            ctx.SetRep(ctx.Find(ctx._stack.Pop()) as Gok, ctx._stack.Pop());
        }

        private static void OnSetRepThis(ScriptEngine ctx)
        {
            ctx.SetRep(ctx._target as Gok, ctx._stack.Pop());
        }

        private static void OnPlayingGob(ScriptEngine ctx)
        {
            ctx._stack.Push(OnPlaying(ctx.Find(ctx._stack.Pop()) as Gok));
        }

        private static void OnPlayingThis(ScriptEngine ctx)
        {
            ctx._stack.Push(OnPlaying(ctx._target as Gok));
        }

        private static void OnFGobExists(ScriptEngine ctx)
        {
            ctx._stack.Push(Convert.ToInt32(ctx.Find(ctx._stack.Pop()) != null));
        }

        private static void OnStartLongOp(ScriptEngine ctx)
        {
            Application.Current.StartLongOp();
        }

        private static void OnDestroyGob(ScriptEngine ctx)
        {
            ctx.Destroy(ctx.Find(ctx._stack.Pop()));
        }

        private static void OnDestroyThis(ScriptEngine ctx)
        {
            ctx.Destroy(ctx._target);
        }

        private static void OnStopSound(ScriptEngine ctx)
        {
            Application.Current.SoundManager.StopSound(ctx._stack.Pop());
        }

        private static void OnSetNoSlipGob(ScriptEngine ctx)
        {
            OnSetNoSlip(ctx.Find(ctx._stack.Pop()) as Gok, ctx._stack.Pop());
        }

        private static void OnSetNoSlipThis(ScriptEngine ctx)
        {
            OnSetNoSlip(ctx._target as Gok, ctx._stack.Pop());
        }

        private static void OnCell(ScriptEngine ctx)
        {
            ctx._isPaused = true;
            OnCellNoPause(ctx);
        }

        private static void OnCellNoPause(ScriptEngine ctx)
        {
            var args = ctx._stack.PopList(stackalloc int[4]);
            (ctx._target as Gok)?.Cell(args[0], Gok.EvalCellFlags.NoScript, 0, args[1], args[2], args[3]);
        }

        private static void OnChangeStateGob(ScriptEngine ctx)
        {
            OnChangeState(ctx.Find(ctx._stack.Pop()) as Gok, ctx._stack.Pop());
        }

        private static void OnChangeStateThis(ScriptEngine ctx)
        {
            OnChangeState(ctx._target as Gok, ctx._stack.Pop());
        }

        private static void OnPlaySoundGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.OnPlaySound(ctx.Find(ctx._stack.Pop()), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop()));
        }

        private static void OnPlaySoundThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.OnPlaySound(ctx._target, ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop()));
        }

        private static void OnCreateHelpGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.OnCreateHelp(ctx.Find(ctx._stack.Pop()), ctx._stack.Pop()));
        }

        private static void OnCreateHelpThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.OnCreateHelp(ctx._target, ctx._stack.Pop()));
        }

        private static void OnSetColorTable(ScriptEngine ctx)
        {
            ctx.OnSetColorTable(ctx._stack.Pop());
        }

        private static void OnEndLongOp(ScriptEngine ctx)
        {
            Application.Current.EndLongOp(ctx._stack.Pop() != 0);
        }

        private static void OnFlushUserEvents(ScriptEngine ctx)
        {
            Application.Current.FlushUserEvents((InputTypes)ctx._stack.Pop());
        }

        private static void OnGidParGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetParentGid(ctx.Find(ctx._stack.Pop())));
        }

        private static void OnGidParThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.GetParentGid(ctx._target));
        }

        private static void OnEnqueueCid(ScriptEngine ctx)
        {
            ctx.EnqueueMessage(ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop());
        }

        private static void OnFilterCmdsGob(ScriptEngine ctx)
        {
            ctx.FilterCmds(ctx.Find(ctx._stack.Pop()) as Gok, ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop());
        }

        private static void OnFilterCmdsThis(ScriptEngine ctx)
        {
            ctx.FilterCmds(ctx._target as Gok, ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop());
        }

        private static void OnGetProp(ScriptEngine ctx)
        {
            Application.Current.GetProp(ctx._stack.Pop(), out int value);
            ctx._stack.Push(value);
        }

        private static void OnSetProp(ScriptEngine ctx)
        {
            Application.Current.SetProp(ctx._stack.Pop(), ctx._stack.Pop());
        }

        private static void OnRunScriptGob(ScriptEngine ctx)
        {
            Span<int> parameters = stackalloc int[ctx._stack.Pop()];
            ctx._stack.Push(OnRunScript(ctx.Find(ctx._stack.Pop()) as Gok, ctx._stack.Pop(), ctx._stack.PopList(parameters)));
        }

        private static void OnRunScriptThis(ScriptEngine ctx)
        {
            Span<int> parameters = stackalloc int[ctx._stack.Pop()];
            ctx._stack.Push(OnRunScript(ctx._target as Gok, ctx._stack.Pop(), ctx._stack.PopList(parameters)));
        }

        private static void OnRunScriptCnoGob(ScriptEngine ctx)
        {
            Span<int> parameters = stackalloc int[ctx._stack.Pop()];
            ctx._stack.Push(OnRunScriptCno(ctx.Find(ctx._stack.Pop()) as Gok, ctx._stack.Pop(), ctx._stack.PopList(parameters)));
        }

        private static void OnRunScriptCnoThis(ScriptEngine ctx)
        {
            Span<int> parameters = stackalloc int[ctx._stack.Pop()];
            ctx._stack.Push(OnRunScriptCno(ctx._target as Gok, ctx._stack.Pop(), ctx._stack.PopList(parameters)));
        }

        private static void OnCreateChildGob(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.OnCreateChild(ctx.Find(ctx._stack.Pop()), ctx._stack.Pop(), ctx._stack.Pop()));
        }

        private static void OnCreateChildThis(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.OnCreateChild(ctx._target, ctx._stack.Pop(), ctx._stack.Pop()));
        }

        private static void OnMatch(ScriptEngine ctx)
        {
            int num = ctx._stack.Pop();
            int toMatch = ctx._stack.Pop();
            int value = ctx._stack.Pop();
            while (--num >= 0)
            {
                int key = ctx._stack.Pop();
                if (key == toMatch)
                {
                    value = ctx._stack.Pop();
                    break;
                }
                ctx._stack.Pop();
            }
            while (--num >= 0)
            {
                ctx._stack.Pop();
                ctx._stack.Pop();
            }
            ctx._stack.Push(value);
        }

        private static void OnConcatStrs(ScriptEngine ctx)
        {
            ctx.OnConcatStrs(ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop());
        }

        private static void OnLenStr(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx.loadedStrings[ctx._stack.Pop()].Length);
        }

        private static void OnCopySubStr(ScriptEngine ctx)
        {
            ctx.OnCopySubStr(ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop(), ctx._stack.Pop());
        }

        private static void OnCopyStr(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            int b = ctx._stack.Pop();

            ctx.OnCopySubStr(x, 0, -1, b);
        }

        private static void OnNumToStr(ScriptEngine ctx)
        {
            int n = ctx._stack.Pop();
            ctx.loadedStrings[ctx._stack.Peek()] = n.ToString();
        }

        private static void OnPop(ScriptEngine ctx)
        {
            ctx._stack.Pop();
        }

        private static void OnReturn(ScriptEngine ctx)
        {
            ctx.current = ctx.script.Instructions.Count;
            OnSetReturn(ctx);
        }

        private static void OnSetReturn(ScriptEngine ctx)
        {
            ctx.returnValue = ctx._stack.Pop();
        }

        private static void OnGo(ScriptEngine ctx)
        {
            ctx.current = ctx.ParseBranchDestination(ctx._stack.Pop()); ;
        }

        private static void OnGoZ(ScriptEngine ctx)
        {
            var x = ctx._stack.Pop();
            var dest = ctx.ParseBranchDestination(ctx._stack.Pop());
            ctx.OnConditionalBranch(x == 0, dest);
        }

        private static void OnGoLe(ScriptEngine ctx)
        {
            var y = ctx._stack.Pop();
            var x = ctx._stack.Pop();
            var dest = ctx.ParseBranchDestination(ctx._stack.Pop());
            ctx.OnConditionalBranch(x <= y, dest);
        }

        private static void OnGoGe(ScriptEngine ctx)
        {
            var y = ctx._stack.Pop();
            var x = ctx._stack.Pop();
            var dest = ctx.ParseBranchDestination(ctx._stack.Pop());
            ctx.OnConditionalBranch(x >= y, dest);
        }

        private static void OnGoLt(ScriptEngine ctx)
        {
            var y = ctx._stack.Pop();
            var x = ctx._stack.Pop();
            var dest = ctx.ParseBranchDestination(ctx._stack.Pop());
            ctx.OnConditionalBranch(x < y, dest);
        }

        private static void OnGoGt(ScriptEngine ctx)
        {
            var y = ctx._stack.Pop();
            var x = ctx._stack.Pop();
            var dest = ctx.ParseBranchDestination(ctx._stack.Pop());
            ctx.OnConditionalBranch(x > y, dest);
        }

        private static void OnGoNe(ScriptEngine ctx)
        {
            var y = ctx._stack.Pop();
            var x = ctx._stack.Pop();
            var dest = ctx.ParseBranchDestination(ctx._stack.Pop());
            ctx.OnConditionalBranch(x != y, dest);
        }

        private static void OnGoEq(ScriptEngine ctx)
        {
            var y = ctx._stack.Pop();
            var x = ctx._stack.Pop();
            var dest = ctx.ParseBranchDestination(ctx._stack.Pop());
            ctx.OnConditionalBranch(x == y, dest);
        }

        private static void OnGoNz(ScriptEngine ctx)
        {
            var value = ctx._stack.Pop();
            var dest = ctx.ParseBranchDestination(ctx._stack.Pop());
            ctx.OnConditionalBranch(value != 0, dest);
        }

        private static void OnSelect(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._stack.SelectList(ctx._stack.Pop(), ctx._stack.Pop()));
        }

        private static void OnRndList(ScriptEngine ctx)
        {
            var n = ctx._stack.Pop();
            ctx._stack.Push(ctx._stack.SelectList(n, Random.Shared.Next(0, n)));
        }

        private static void OnPopList(ScriptEngine ctx)
        {
            ctx._stack.PopList(ctx._stack.Pop());
        }

        private static void OnDupList(ScriptEngine ctx)
        {
            ctx._stack.DupList(ctx._stack.Pop());
        }

        private static void OnRev(ScriptEngine ctx)
        {
            ctx._stack.ReverseStack(ctx._stack.Pop());
        }

        private static void OnRot(ScriptEngine ctx)
        {
            int amount = ctx._stack.Pop();
            int n = ctx._stack.Pop();
            ctx._stack.RotateStack(n, amount);
        }

        private static void OnSwap(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            int y = ctx._stack.Pop();
            ctx._stack.Push(x);
            ctx._stack.Push(y);
        }

        private static void OnDup(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._stack.Peek());
        }

        private static void OnMulDiv(ScriptEngine ctx)
        {
            int number = ctx._stack.Pop();
            int numerator = ctx._stack.Pop();
            var denominator = ctx._stack.Pop();
            if (denominator == 0)
                Error();
            ctx._stack.Push(checked((int)((long)number * numerator / denominator)));
        }

        private static void OnRnd(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            if (x <= 0)
                Error();
            ctx._stack.Push(Random.Shared.Next(x));
        }

        private static void OnAbs(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            if (x < 0)
                x = -x;
            ctx._stack.Push(x);
        }

        private static void OnLe(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            ctx._stack.Push(Convert.ToInt32(ctx._stack.Pop() <= x));
        }

        private static void OnGe(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            ctx._stack.Push(Convert.ToInt32(ctx._stack.Pop() >= x));
        }

        private static void OnLt(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            ctx._stack.Push(Convert.ToInt32(ctx._stack.Pop() < x));
        }

        private static void OnGt(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            ctx._stack.Push(Convert.ToInt32(ctx._stack.Pop() > x));
        }

        private static void OnNe(ScriptEngine ctx)
        {
            ctx._stack.Push(Convert.ToInt32(ctx._stack.Pop() != ctx._stack.Pop()));
        }

        private static void OnEq(ScriptEngine ctx)
        {
            ctx._stack.Push(Convert.ToInt32(ctx._stack.Pop() == ctx._stack.Pop()));
        }

        private static void OnLNot(ScriptEngine ctx)
        {
            ctx._stack.Push(Convert.ToInt32(ctx._stack.Pop() == 0));
        }

        private static void OnLXor(ScriptEngine ctx)
        {
            ctx._stack.Push(Convert.ToInt32(ctx._stack.Pop() != 0 != (ctx._stack.Pop() != 0)));
        }

        private static void OnBNot(ScriptEngine ctx)
        {
            ctx._stack.Push(~ctx._stack.Pop());
        }

        private static void OnBXor(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._stack.Pop() ^ ctx._stack.Pop());
        }

        private static void OnBAnd(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._stack.Pop() & ctx._stack.Pop());
        }

        private static void OnBOr(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._stack.Pop() | ctx._stack.Pop());
        }

        private static void OnBLShft(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            ctx._stack.Push(ctx._stack.Pop() << x);
        }

        private static void OnBRShft(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            ctx._stack.Push(ctx._stack.Pop() >> x);
        }

        private static void OnDec(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._stack.Pop() - 1);
        }

        private static void OnInc(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._stack.Pop() + 1);
        }

        private static void OnNeg(ScriptEngine ctx)
        {
            ctx._stack.Push(-ctx._stack.Pop());
        }

        private static void OnMod(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            if (x == 0)
                Error();
            ctx._stack.Push(ctx._stack.Pop() % x);
        }

        private static void OnDiv(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            ctx._stack.Push(ctx._stack.Pop() / x);
        }

        private static void OnMul(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._stack.Pop() * ctx._stack.Pop());
        }

        private static void OnSub(ScriptEngine ctx)
        {
            int x = ctx._stack.Pop();
            ctx._stack.Push(ctx._stack.Pop() - x);
        }

        private static void OnAdd(ScriptEngine ctx)
        {
            ctx._stack.Push(ctx._stack.Pop() + ctx._stack.Pop());
        }

        private static void OnPause(ScriptEngine ctx)
        {
            ctx._isPaused = true;
        }

        private static void OnExit(ScriptEngine ctx)
        {
            ctx.current = ctx.script.Instructions.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int XMouse(Gob gob)
        {
            return gob?.GetCursorPosition().X ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int YMouse(Gob gob)
        {
            return gob?.GetCursorPosition().Y ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWidth(Gob gob)
        {
            return gob?.GetRectangle(CoordinateSpace.None).Width ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHeight(Gob gob)
        {
            return gob?.GetRectangle(CoordinateSpace.None).Height ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetX(Gob gob)
        {
            if (gob is Gok gok)
                return gok.GetPositionWithLayout(CoordinateSpace.Local).X;
            return gob?.GetPosition(CoordinateSpace.Local).X ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetY(Gob gob)
        {
            if (gob is Gok gok)
                return gok.GetPositionWithLayout(CoordinateSpace.Local).Y;
            return gob?.GetPosition(CoordinateSpace.Local).Y ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetRep(Gok gok, int value)
        {
            gok?.Cell(value, Gok.EvalCellFlags.StopRunningScript, 0, 0, 0, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int OnPlaying(Gok gok)
        {
            return Convert.ToInt32(gok?.PlayVideoMaybe() ?? false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void OnChangeState(Gok gok, int newState)
        {
            if (newState >= 0 && newState < 0x7fff)
            {
                gok?.ChangeState(newState);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StopSoundByClass(int unk1, int unk2)
        {
            // TODO: Implement StopSoundClass
            //App.Instance.SoundManager.VirtualFunc8(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Destroy(Gob gob)
        {
            if (gob != null)
            {
                if (owner != gob)
                    gob.Dispose();
                else
                    while (gob.FirstChild != null)
                        gob.FirstChild.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void OnSetNoSlip(Gok gok, int value)
        {
            gok?.SetNoSlip(value != 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int OnPlaySound(Gob gob, int ctgSound, int cnoSound, int unk1, int unk2, int unk3, int unk4, int unk5)
        {
            if (gob == null)
                return 0;
            var identifier = new ChunkIdentifier(new(ctgSound), cnoSound);
            if (identifier == default)
                return 0;
            int track = Random.Shared.Next(int.MaxValue);
            Application.Current.SoundManager.StopSound(track);
            Application.Current.SoundManager.PlaySound(resolver, identifier, track);
            return track;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int OnCreateHelp(Gob gob, int htopCno)
        {
            return gob != null ?
                owner.CreateHbalChild(gob, resolver, htopCno, default)?.Id ?? 0 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetParentGid(Gob gob)
        {
            return owner.IsAnscestorOf(gob?.Parent) ?
                gob.Parent.Id : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnqueueMessage(int message, int cid, int paramA, int paramB, int paramC, int paramD)
        {
            var target = cid == 0 ? null : owner.FindComponent(cid);

            Application.Current.Exchange.Enqueue(new(message, target, paramA, paramB, paramC, paramD)
            {
                SenderInfo = new()
                {
                    Sender = _target,
                    Tag = script.Metadata.Key.Tag.ToString(),
                    Number = script.Metadata.Key.Number,
                    LineNumber = current,
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FilterCmds(Gok gok, int message, int cid, int script)
        {
            gok?.FilterCmds(message, cid, script);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int OnRunScriptCno(Gok gok, int scriptCno, Span<int> parameters)
        {
            if (gok?.RunScriptCno(scriptCno, parameters, out int result, out int resultCode) != null && resultCode == 1)
            {
                return result;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int OnRunScript(Gok gok, int script, Span<int> parameters)
        {
            if (gok?.RunScript(script, parameters, out int result, out int resultCode) != null && resultCode == 1)
            {
                return result;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int OnCreateChild(Gob gob, int cid, int gokdCno)
        {
            return gob != null ?
                owner.CreateGokChild(gob, cid, gokdCno, resolver)?.Id ?? 0 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ParseBranchDestination(int value)
        {
            if ((value & 0xff000000) == 0xcc000000)
            {
                var dest = value & 0xffffff;
                if(dest <= script.Instructions.Count)
                {
                    return dest;
                }
            }
            Error();
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnConditionalBranch(bool condition, int dest)
        {
            if (condition)
                current = dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnConcatStrs(int str1, int str2, int dest)
        {
            loadedStrings[dest] = string.Concat(loadedStrings[str1], loadedStrings[str2]);
            _stack.Push(dest);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnCopySubStr(int src, int startIndex, int length, int dest)
        {
            if (length < 0)
                loadedStrings[dest] = loadedStrings[src].Substring(startIndex);
            else
                loadedStrings[dest] = loadedStrings[src].Substring(startIndex, length);
            _stack.Push(dest);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnSetColorTable(int paletteCno)
        {
            if (resolver.TryResolve<GenericList<ColorBgra>>(new ChunkIdentifier(Tags.GLCR, paletteCno), out var pal))
            {
                (Application.Current as App).Palette = pal;
            }
        }
    }
}
