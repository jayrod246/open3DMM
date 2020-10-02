using System;
using System.Runtime.CompilerServices;

namespace Open3dmm
{
    public sealed partial class ButtonFSM
    {
        public delegate bool TransitionCallback(in Transition transition, bool isLastTransition);

        public ButtonFSM(TransitionCallback onTransition)
        {
            _onTransition = onTransition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EnsureTransitionsCompleted() => TransitionsCompleted || TransitionTable(DefaultTable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotifyMouseReleasedOn() => EvalTransition(new(States.UpOn, false));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotifyMousePressedOn() => TransitionTable(MousePressedTable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotifyMouseOn() => TransitionTable(MouseOnTable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotifyMouseOff() => TransitionTable(MouseOffTable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotifyMouseDownOn() => TransitionTable(MouseDownOnTable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotifyMouseDownOff() => TransitionTable(MouseDownOffTable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotifyMouseUpOn() => TransitionTable(MouseUpOnTable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NotifyMouseUpOff() => TransitionTable(MouseUpOffTable);

        public bool NotifyExternalStateChanged()
        {
            Span<uint> states = stackalloc uint[14];
            var x = 0;

            while (_state != 0u)
            {
                states[x++] = _state;
                _state = InverseTable[_state].DestinationState;
            }

            var from = 0u;
            Unsafe.SkipInit<Transition>(out var t);
            while (--x >= 0)
            {
                t.DestinationState = states[x];
                if (_state == from && !Step(t))
                {
                    return false;
                }
                from = t.DestinationState;
            }

            return EnsureTransitionsCompleted();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TransitionTable(Transition[] table)
        {
            while (true)
            {
                ref readonly var transition = ref table[_state];
                if (transition.DestinationState is 0 || transition.DestinationState == _state)
                {
                    if (table == DefaultTable)
                    {
                        return true;
                    }
                    table = DefaultTable;
                }
                else if (!Step(transition))
                {
                    return false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvalTransition(in Transition transition)
        {
            return Step(transition) && EnsureTransitionsCompleted();
        }

        private bool Step(in Transition transition)
        {
            _state = transition.DestinationState;
            return _onTransition(transition, TransitionsCompleted);
        }

        private bool TransitionsCompleted => DefaultTable[_state].DestinationState == _state;

        public uint State
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _state;
            }
        }

        private readonly TransitionCallback _onTransition;
        private uint _state = 0;
    }
}
