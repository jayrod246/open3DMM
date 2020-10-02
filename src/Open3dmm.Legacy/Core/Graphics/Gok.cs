using Open3dmm.Core;
using Open3dmm.Core.GUI;
using Open3dmm.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    public partial class Gok : Gob
    {
        private readonly ButtonFSM _fsm;
        private int _z;
        private readonly List<Command> _commands;
        private Gokd _gokd;
        private int _toolTipSource;
        private ChunkIdentifier _stateIdentifier;
        private ChunkIdentifier _soundIdentifier;
        private int _lastClickCount;
        private int _lastModifierState;
        private int _soundPlayCounter;
        private ScriptEngine _interpreter;
        private int _cell;
        private PT _layoutOffset;
        private int _delay;
        private SoundManager _soundManager;

        public Gok(GobOptions options, SoundManager soundManager) : base(options)
        {
            _fsm = new(OnTransition);
            _commands = new();
            _cell = -1;
            _toolTipSource = this.Id;
            _soundManager = soundManager;
        }

        struct Command
        {
            public int Message;
            public int Cid;
            public int Script;
        }

        private static readonly int[] BaseCells =
        {
            -1, 16, 0, 17, 20, 1, 24,
            23, 29, 3, 30, 27, 2, -1
        };

        public int StateFlags;
        public Woks Owner;
        public IResolver Resolver;

        public int ZIndex => _z;
        public GokdMiscFlags GokdFlags;
        public PT GorpSize;
        public Gorp Gorp;

        public bool SetZIndex(int value)
        {
            if (_z == value)
            {
                return false;
            }
            var other = FindChildByZIndex(Parent, value);
            if (other != this && Previous != other)
            {
                MoveToSibling(other);
            }
            _z = value;
            return true;
        }

        public override bool OnMouseEvent(Message m)
        {
            var input = (InputState)m.ParamC;

            if (m.Id == (int)KnownMessageValues.MousePressed)
            {
                var state = Owner.GetStateWithKeys(input);
                if (GetStateBlob(state, StateFlags, out _))
                {
                    _lastModifierState = state;
                    _lastClickCount = (short)m.ParamD;
                    if (_fsm.NotifyMousePressedOn())
                    {
                        ChangeCursorArrow(state);
                    }
                }
            }
            else
            {
                var down = input.HasFlag(InputState.LeftButton) ? 1 : 0;
                var on = HitTest(new(m.ParamA, m.ParamB), true) ? 2 : 0;
                _ = (down | on) switch
                {
                    1 => _fsm.NotifyMouseDownOff(),
                    2 => _fsm.NotifyMouseUpOn(),
                    3 => _fsm.NotifyMouseDownOn(),
                    _ => _fsm.NotifyMouseUpOff(),
                };
            }

            return true;
        }

        public override bool OnMouseOver(Message m)
        {
            if (m.Id == (int)KnownMessageValues.MouseExit)
            {
                _ = _fsm.NotifyMouseOff();
            }
            else
            {
                var state = Owner.ModifierState | m.ParamC;
                if (GetStateBlob(state, StateFlags, out _) ? _fsm.NotifyMouseOn() : _fsm.NotifyMouseOff())
                {
                    ChangeCursorArrow(state);
                }
            }
            return true;
        }

        public override void ChangeCursorArrow(int ownerState)
        {
            var gok = this;

            while (true)
            {
                if (gok.GetStateBlob(ownerState, StateFlags, out var blob) && blob.Cursor != -1)
                {
                    // Replace cursor with our own.
                    AppWindow.SetCursorImage(gok.Resolver, blob.Cursor, CursorType.Arrow);
                    return;
                }

                var gob = gok as Gob;

                do
                {
                    if ((gob = gob.Parent) is null)
                    {
                        // Return cursor to standard.
                        AppWindow.SetCursorImage(null, CursorType.Arrow);
                        return;
                    }

                    gok = gob as Gok;
                }
                while (gok is null);

                // Add a flag indicating that the source of a potential cursor change is no longer the original source.
                ownerState |= 256;
            }
        }

        public virtual bool VirtualFunc39(Message m)
        {
            var clockId = m.ParamA;
            if (Owner.Clock1.Id == clockId || Owner.Clock2.Id == clockId)
            {
                _ = TickScript();
            }
            else
            {
                var glopIndex = m.ParamC;
                if (glopIndex == -1 || glopIndex == 0)
                    glopIndex = StateFlags << 16 | 258;
                RunScript(glopIndex, stackalloc int[] { clockId, m.ParamB, m.ParamC }, out _, out _);
            }
            return true;
        }

        public virtual bool OnClick(Message m)
        {
            if (_fsm.State != ButtonFSM.States.Click || _fsm.NotifyMouseReleasedOn())
            {
                BeginSound();
                BubbleRouteClickMessage(this, m);
                EndSound();
            }
            return true;
        }

        private void BubbleRouteClickMessage(Gok originalSource, Message message)
        {
            if (GetStateBlob(message.ParamC, StateFlags, out var stateBlob))
            {
                if (RunScript(stateBlob.Script, stackalloc int[] { message.ParamC, Id, message.ParamD }, out int result, out int resultCode))
                {
                    if (stateBlob.ClickMessage != 0
                        && resultCode != 1
                        && result != 0
                        && ComponentGraph.TryGetExchange(out var cex))
                    {
                        cex.Enqueue(new(stateBlob.ClickMessage, null, Id, originalSource.Id));
                    }
                }
            }
            else
            {
                (Parent as Gok)?.BubbleRouteClickMessage(originalSource, message);
            }
        }

        private bool Exists()
        {
            return Owner.FindByRuntimeId(RuntimeId) == this;
        }

        private bool GetStateBlob(int modifier, int state, out GokdStateBlob stateBlob)
        {
            return _gokd.TryGetStateBlob(modifier, state, out stateBlob);
        }

        public virtual bool VirtualFunc41(Message m)
        {
            if (_commands.Count == 0)
            {
                return false;
            }

            bool success;
            Command cmd;

            if (m.Component is not null)
            {
                if (GetCommand(m.Id, m.Component.Id, out cmd, out _) && HandleMessageWithGlopReference(m, cmd.Script, out success))
                    return success;

                if (GetCommand(0, m.Component.Id, out cmd, out _) && HandleMessageWithGlopReference(m, cmd.Script, out success))
                    return success;
            }

            if (GetCommand(m.Id, 0, out cmd, out _) && HandleMessageWithGlopReference(m, cmd.Script, out success))
                return success;

            return false;
        }

        private bool TryCommand(Message actualMessage, int message, int cid, out bool success)
        {
            if (GetCommand(message, cid, out var command, out _))
            {
                return HandleMessageWithGlopReference(actualMessage, command.Script, out success);
            }

            success = false;
            return false;
        }

        public bool HandleMessageWithGlopReference(Message m, int glopIndex, out bool success)
        {
            var id = _gokd.Metadata.Key;
            var gobStillExists = RunScript(glopIndex, stackalloc int[] { m.Component?.Id ?? 0, m.Id, m.ParamA, m.ParamB, m.ParamC, m.ParamD }, out var result, out var resultCode);
            success = result != 0 && resultCode == 1;
            Debug.Assert(success == !(result == 0 || resultCode != 1));
            return success || !gobStillExists;
        }

        public PT GetPositionWithLayout(CoordinateSpace coordinateSpace)
        {
            var (x, y) = GetPosition(coordinateSpace);
            return new(x + _layoutOffset.X, y + _layoutOffset.Y);
        }

        public bool LoadGokd(Woks owner, int number, IResolver resolver)
        {
            return resolver.TryResolve<Gokd>(new ChunkIdentifier(Tags.GOKD, number), out var gokd)
                && LoadGokd(owner, gokd, resolver);
        }

        public bool LoadGokd(Woks owner, Gokd gokd, IResolver resolver)
        {
            Owner = owner;
            _gokd = gokd;
            Resolver = resolver;
            var layout = gokd.GetLayout(Parent.Id);
            _layoutOffset = new(layout.X, layout.Y);
            _z = int.MaxValue;
            GokdFlags = gokd.MiscFlags;
            _ = SetZIndex(layout.Z);
            return true;
        }

        private bool OnTransition(in ButtonFSM.Transition transition, bool isLastTransition)
        {
            if (ComponentGraph.TryGetExchange(out var cex))
            {
                if (transition.DestinationState is >= 7 and < 14)
                {
                    cex.Capture ??= this;
                }
                else if (cex.Capture == this)
                {
                    cex.Capture = null;
                }

                if (transition.RaisesClickEvent)
                {
                    cex.Enqueue(new()
                    {
                        Id = (int)KnownMessageValues.Click,
                        Component = this,
                        ParamC = _lastModifierState,
                        ParamD = _lastClickCount,
                    });
                }
            }

            var cell = ComputeCell(transition.DestinationState, StateFlags);
            return cell == -1 || EvalCell(cell, EvalCellFlags.StopRunningScript | EvalCellFlags.PlaySound, isLastTransition ? 0 : Tags.GLSC, 0, 0);
        }

        private static int ComputeCell(uint buttonState, int flags)
        {
            return BaseCells[buttonState] | (flags << 16);
        }

        [Flags]
        public enum EvalCellFlags
        {
            StopRunningScript = 1,
            NoScript = 2,
            ForceLoad = 4,
            PlaySound = 8,
        }

        public bool EvalCell(int cell, EvalCellFlags flags, Tags type, int offsetX, int offsetY)
        {
            if (cell != -1)
            {
                var forceLoad = (flags & EvalCellFlags.ForceLoad) != 0;
                var stopRunningScript = (flags & EvalCellFlags.StopRunningScript) != 0;

                bool loadScript(int cell)
                {
                    return ResolverUtils.ResolveChildren(_gokd.Metadata.Resolver, _gokd.Metadata.Key, cell, Tags.GLSC,
                            (scope, id) =>
                            {
                                if (scope.TryResolve<Script>(id, out var script))
                                {
                                    var sceg = Owner.CreateScriptEngine(Resolver, this);
                                    if (sceg.Load(script))
                                    {
                                        if (_interpreter != null)
                                        {
                                            Owner.Clock1.RemoveAlarms(this);
                                            Owner.Clock2.RemoveAlarms(this);
                                        }
                                        _interpreter = sceg;
                                        return true;
                                    }
                                }
                                return false;
                            }
                        );
                }
                var ignoreScript = (flags & EvalCellFlags.NoScript) != 0;
                var shouldLoadScript = _cell != cell || forceLoad;
                if (!ignoreScript && type is 0 or Tags.GLSC && shouldLoadScript && loadScript(cell))
                {
                    _cell = cell;
                    if (!TickScript())
                    {
                        return false;
                    }
                }
                else if (type is not Tags.GLSC)
                {
                    var gorpChanged = ResolverUtils.ResolveChildren(_gokd.Metadata.Resolver, _gokd.Metadata.Key, cell, type,
                        (scope, id) =>
                        {
                            if (id.Tag == Tags.GLSC)
                            {
                                return false;
                            }

                            if (!forceLoad
                                && Gorp != null
                                && _stateIdentifier == id)
                            {
                                return true;
                            }
                            var gorp = CreateGorp(_gokd.Metadata.Resolver, id);
                            if (gorp == null)
                            {
                                return false;
                            }
                            SetGorp(gorp, offsetX, offsetY);
                            _stateIdentifier = id;
                            offsetX = offsetY = 0;
                            return true;
                        });

                    if (gorpChanged && stopRunningScript)
                    {
                        _interpreter = null;
                        Owner.Clock1.RemoveAlarms(this);
                        Owner.Clock2.RemoveAlarms(this);
                        _cell = -1;
                    }
                }
            }

            Offset(offsetX, offsetY);
            bool playSound = (flags & EvalCellFlags.PlaySound) != 0;
            if (playSound)
            {
                PlaySoundReference(cell);
            }

            return true;
        }

        private void PlaySoundReference(int childID)
        {
            if (_gokd.Metadata.TryGetChild((childID, Tags.WAVE), out var c) || _gokd.Metadata.TryGetChild((childID, Tags.MIDS), out c))
            {
                PlaySound(c.Identifier);
            }
        }

        public virtual bool PlaySound(ChunkIdentifier identifier)
        {
            if (identifier.Number == -1)
            {
                return false;
            }

            _soundIdentifier = identifier;

            if (_soundPlayCounter == 0)
            {
                _soundManager.PlaySound(Resolver, identifier);
            }
            return true;
        }

        private void BeginSound()
        {
            _soundIdentifier = default;
            _soundPlayCounter++;
        }

        private void EndSound()
        {
            if (_soundPlayCounter > 0
                && --_soundPlayCounter == 0
                && (GokdFlags & GokdMiscFlags.SoundEnabled) != 0)
            {
                if (Exists())
                {
                    PlaySound(_soundIdentifier);
                }
            }
        }

        // Virtual35
        private bool TickScript()
        {
            if (_interpreter is null)
                return true;

            var delay = 0;
            var interpreter = _interpreter;

            while (true)
            {
                var iStack12 = interpreter.Run(out _, out var canContinue);

                if (FindByRuntimeId(RuntimeId) != this) // No longer exists!
                    return false;

                if (!iStack12 || !canContinue || _interpreter is null)
                {
                    if (interpreter != _interpreter)
                    {
                        return true;
                    }

                    _interpreter = null;

                    Owner.Clock1.RemoveAlarms(this);
                    Owner.Clock2.RemoveAlarms(this);

                    return _fsm.EnsureTransitionsCompleted();
                }

                if ((GokdFlags & GokdMiscFlags.NoSlip) == 0)
                {
                    Owner.Clock1.SetAlarm(_delay, this, 0, false);
                    return true;
                }

                delay += Math.Max(1, _delay);

                if (delay > Owner.Clock2.Delay)
                {
                    break;
                }
            }

            Owner.Clock2.SetAlarm(delay, this, 0, false);
            return true;
        }

        public virtual void SetGorp(Gorp gorp, int offsetX, int offsetY)
        {
            LTRB rc;
            if (gorp == null)
                rc = default;
            else
            {
                // gorp.VirtualFunc14
                gorp.VirtualFunc3(GorpSize.X, GorpSize.Y);
                rc = gorp.GetRect();
            }
            Gorp = gorp;
            var newRect = GetRectangle(CoordinateSpace.Local);
            newRect.Left += rc.Left + _layoutOffset.X + offsetX;
            newRect.Top += rc.Top + _layoutOffset.Y + offsetY;
            newRect.Right = newRect.Left + rc.Width;
            newRect.Bottom = newRect.Top + rc.Height;
            _layoutOffset = new(-rc.Left, -rc.Top);
            UpdateRectangle(newRect, null);
        }

        private Gorp CreateGorp(IScopedResolver scope, ChunkIdentifier identifier)
        {
            return (Tags)identifier.Tag switch
            {
                Tags.MBMP or Tags.MASK => new Gorb(scope, identifier),
                Tags.FILL => new Gorf(scope, identifier),
                Tags.TILE => new Gort(scope, identifier),
                Tags.VIDE => null, // TODO: Implement Gorv (videos)
                _ => null,
            };
        }

        public static Gok FindChildByZIndex(Gob item, int z)
        {
            item = item?.FirstChild;
            Gok result = null;
            while (item != null)
            {
                if (item is Gok gok)
                {
                    if (z >= gok._z)
                        break;
                    result = gok;
                }
                item = item.Next;
            }
            return result;
        }

        public bool RunScriptCno(int number, ReadOnlySpan<int> parameters, out int result, out int resultCode)
        {
            if (_gokd.Metadata.Resolver.TryResolve<Script>(new ChunkIdentifier(Tags.GLOP, number), out var script))
            {
                var sceg = Owner.CreateScriptEngine(Resolver, this);
                resultCode = sceg.Run(script, parameters, out result, out _) ? 1 : 2;
                return Exists();
            }
            result = 0;
            resultCode = 0; // script simply was not found
                            // TODO: resultCode = 2; if errors while deserializing.
            return true;
        }

        public bool RunScript(int glopIndex, ReadOnlySpan<int> parameters, out int result, out int resultCode)
        {
            if (_gokd.Metadata.Resolver.TryGetIdentifier(_gokd.Metadata.Key, Tags.GLOP, glopIndex, out var reference))
                return RunScriptCno(reference.Number, parameters, out result, out resultCode);

            result = 0;
            resultCode = 0;
            return true;
        }

        /// <returns>True for when a new command is added, otherwise false when an existing command gets updated.</returns>
        private bool AddCommand(int message, int cid, int script)
        {
            if (GetCommand(message, cid, out var cmd, out int i))
            {
                cmd.Script = script;
                _commands[i] = cmd;
                return false;
            }

            _commands.Insert(i, new()
            {
                Message = message,
                Cid = cid,
                Script = script,
            });

            return true;
        }

        private bool RemoveCommand(int message, int cid)
        {
            if (!GetCommand(message, cid, out _, out int i))
                return false;

            _commands.RemoveAt(i);
            return true;
        }

        private bool GetCommand(int message, int cid, out Command command, out int index)
        {
            Unsafe.SkipInit(out command);
            var span = CollectionsMarshal.AsSpan(_commands);

            int lo, mid, hi;

            lo = 0;
            hi = span.Length;

            while (lo < hi)
            {
                mid = (lo + hi) / 2;
                if (span[mid].Message < message || span[mid].Cid < cid)
                    lo = mid + 1;
                else
                    hi = mid;
            }

            index = lo;

            if (index < span.Length)
            {
                command = span[index];
                if (command.Message == message && command.Cid == cid)
                    return true;
            }

            return false;
        }

        public bool FilterCmds(int message, int cid, int script)
        {
            const int PRIORITY_COMMAND = -10000;

            if (!ComponentGraph.TryGetExchange(out var cex))
                throw new InvalidOperationException("Can't filter commands when there is no component exchange instance.");

            if (HasScript(script))
            {
                if (message == 0 && cid == 0)
                    return false;

                if (AddCommand(message, cid, script))
                {
                    // Reinserting the listener ensures that it will be the first
                    // one to react to messages (at the given priority level).
                    cex.RemoveListener(this, PRIORITY_COMMAND);
                    return cex.AddListener(this, PRIORITY_COMMAND, MessageFlags.Self | MessageFlags.Broadcast | MessageFlags.Other);
                }

                return true;
            }

            if (message == 0 && cid == 0)
            {
                _commands.Clear();
            }
            else
            {
                RemoveCommand(message, cid);
            }

            if (_commands.Count == 0)
            {
                cex.RemoveListener(this, PRIORITY_COMMAND);
            }

            return true;
        }

        private bool HasScript(int script)
        {
            return script != -1 && _gokd.Metadata.TryGetChild((script, Tags.GLOP), out _);
        }

        public override void Draw(Veldrid.CommandList commandList, in RectangleF dest)
        {
            if (Gorp == null)
                return;
            Gorp.Draw(commandList, in dest);
        }

        public bool ChangeState(int value)
        {
            StateFlags = value;
            return _fsm.NotifyExternalStateChanged();
        }

        public virtual bool Cell(int state, EvalCellFlags flags, Tags tag, int offsetX, int offsetY, int lwDelay)
        {
            if (lwDelay != 0)
                _delay = lwDelay;
            var stopRunningScript = (flags & EvalCellFlags.StopRunningScript) != 0;
            if (stopRunningScript && !_fsm.EnsureTransitionsCompleted())
            {
                return true;
            }
            return EvalCell(state, flags, tag, offsetX, offsetY);
        }

        public void SetNoSlip(bool disabled)
        {
            GokdFlags = FlagHelper.SetFlag(GokdFlags, GokdMiscFlags.NoSlip, disabled);
        }

        public virtual bool PlayVideoMaybe()
        {
            return Gorp?.VirtualFunc9() ?? false;
        }

        public override void MoveAbs(int x, int y)
        {
            var pt = GetPositionWithLayout(CoordinateSpace.None);
            x -= pt.X;
            y -= pt.Y;
            base.MoveAbs(x, y);
        }

        public override bool HitTest(PT pt, bool precise)
        {
            if (precise)
            {
                if (Gorp == null || (GokdFlags & GokdMiscFlags.NoHitPrecise) != 0 || Id == 17 || !base.HitTest(pt, true))
                    return false;
                if ((GokdFlags & GokdMiscFlags.NoHitGraphic) != 0)
                    return true;
                return Gorp.HitTest(pt);
            }
            else
            {
                if ((GokdFlags & GokdMiscFlags.NoHit) != 0 || Id == 17)
                    return false;
                return base.HitTest(pt, false);
            }
        }

        public override bool ShowToolTip(ref Hbal hbal, int x, int y)
        {
            hbal?.Dispose();
            hbal = null;
            var source = this;

            if (Id != _toolTipSource)
            {
                source = Find(_toolTipSource) as Gok;

                if (source is null)
                    return false;
            }

            var htopNum = source.GetHtopNumber();

            if (htopNum == -1)
                return false;

            if (htopNum != -2)
            {
                hbal = Owner.CreateHbalChild(null, Resolver, htopNum, new HtopValues
                {
                    RelativeId = Id,
                    CnoGraphics = -1,
                    CnoScript = -1,
                    CnoSound = -1,
                    Id = 17,
                });
            }

            return true;
        }

        private int GetHtopNumber()
        {
            if (GetStateBlob(Owner.ModifierState, StateFlags, out var blob))
                return blob.Help;
            return -1;
        }
    }
}
