using Open3dmm.Core;
using System;
using System.Collections.Generic;

namespace Open3dmm
{
    [Flags]
    public enum ClockFlags
    {
        None = 0,
        /// <summary>
        /// Resets the clock each time input is received.
        /// </summary>
        ResetOnInput = 1,
        /// <summary>
        /// Invokes every alarm that was set to go off in the time between update calls.
        /// </summary>
        NoSlip = 2,
    }
    public partial class Clok : Component
    {
        const int PRIORITY = 32769;
        private static readonly List<Clok> _allClocks = new(8);

        public static Clok Get(int cid) => _allClocks.Find(c => c.Id == cid);

        private readonly List<Alarm> alarms = new();
        private readonly HashSet<Component> users = new();
        private readonly ClockFlags flags;

        struct Alarm : IComparable<Alarm>
        {
            public Component Component;
            public int Delay;
            public int Param;

            public int CompareTo(Alarm other)
                => Delay.CompareTo(other.Delay);

            public Message CreateMessage(int clockId) => new()
            {
                Id = (int)KnownMessageValues.Alarm,
                Component = Component,
                ParamA = clockId,
                ParamB = Delay,
                ParamC = Param,
            };
        }

        public Clok(int cid, ClockFlags flags) : base(cid)
        {
            _allClocks.Add(this);
            this.flags = flags;
            _delayUntilNextAlarm = -1;
        }

        private Message msg;

        public int Delay { get; private set; }

        private Usac _timer;
        private int _t;
        private int _delay;
        private int _lastAlarm;
        private int _delaySoFar;
        private int _timeOffset;
        private int _delayUntilNextAlarm;

        public void Start(int delay)
        {
            _timer = Application.Current.Usac;
            _delay = _timeOffset = delay;
            _lastAlarm = _timer.GetTime();
            Application.Current.Exchange.RemoveListener(this, PRIORITY);
            Application.Current.Exchange.AddListener(this, PRIORITY, MessageFlags.Self | MessageFlags.Broadcast | MessageFlags.Other);
        }

        private new bool Handle(Message message)
        {
            msg = message;
            Delay = 0;
            if (IsAlarmMessage())
            {
                return false;
            }

            if (DoesMessageRestartClock())
            {
                RestartClock();
            }
            else
            {
                AdvanceClock();
            }

            if (IsNextAlarmReady())
            {
                SetOffAlarms();
            }

            _delay = _delaySoFar;
            return false;
        }

        private bool IsAlarmMessage() => msg.Id is (int)KnownMessageValues.Alarm;

        private bool DoesMessageRestartClock() =>
            flags.HasFlag(ClockFlags.ResetOnInput)
            && msg.Id is (int)KnownMessageValues.MouseOver
            or (int)KnownMessageValues.Char
            or (int)KnownMessageValues.MouseDragMaybe
            or < (int)KnownMessageValues.ScrollBar;

        private void RestartClock()
        {
            _lastAlarm = _t = _timer.GetTime();
            _delaySoFar = 0;
            _timeOffset = 0;
        }

        private void AdvanceClock()
            => _delaySoFar = CalculateDelay();

        private bool IsNextAlarmReady()
            => _delaySoFar >= _delayUntilNextAlarm;

        private void SetOffAlarms()
        {
            _delayUntilNextAlarm = -1;
            while (alarms.Count > 0)
            {
                var alarm = alarms[0];
                if (_delaySoFar < alarm.Delay)
                {
                    _delayUntilNextAlarm = alarm.Delay;
                    break;
                }
                alarms.RemoveAt(0);
                _delay = alarm.Delay;

                if (alarm.Delay < _delaySoFar && !flags.HasFlag(ClockFlags.NoSlip))
                {
                    _delaySoFar = _timeOffset = alarm.Delay;
                    _lastAlarm = _t;
                }

                var newMessage = alarm.CreateMessage(Id);
                if (alarm.Component is null)
                {
                    Application.Current.Exchange.Enqueue(newMessage);
                }
                else if (alarm.Component is Component cmh)
                {
                    Delay = _delaySoFar - _delay;
                    cmh.Handle(newMessage);
                }
            }
        }

        public void RemoveAlarms(Component component)
        {
            if (!users.Remove(component))
            {
                return;
            }

            int i = alarms.Count;
            while (--i >= 0)
            {
                if (alarms[i].Component == component)
                    alarms.RemoveAt(i);
            }
            component.Disposed -= RemoveAlarms;
        }

        public void SetAlarm(int lwDelay, Component component, int param, bool recalculate)
        {
            lwDelay = Math.Max(1, lwDelay) + CalculateDelay(recalculate);
            if (users.Add(component))
            {
                component.Disposed += RemoveAlarms;
            }

            AddAlarm(new()
            {
                Component = component,
                Delay = lwDelay,
                Param = param,
            });

            if (lwDelay < _delayUntilNextAlarm)
                _delayUntilNextAlarm = lwDelay;
        }

        private void AddAlarm(Alarm alarm)
        {
            int index = alarms.BinarySearch(alarm);
            alarms.Insert(index < 0 ? ~index : index, alarm);
        }

        private int CalculateDelay(bool useRealtime = true)
        {
            if (useRealtime)
            {
                _t = _timer.GetTime();
                return _timeOffset + Usac.CalculateFrameDelay(_t - _lastAlarm, 60, 1000);
            }
            return _delay;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _allClocks.Remove(this);
            }
            base.Dispose(disposing);
        }
    }
}
