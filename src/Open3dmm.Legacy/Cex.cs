using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    public class MessageQueue
    {
        Message _lastMessage;
        Message[] _arr;
        int _slot;
        int _nextFree;
        int _size;

        public ref Message LastMessage => ref _lastMessage;

        public MessageQueue(int capacity)
        {
            _arr = new Message[capacity];
        }

        public void Enqueue(Message message)
        {
            if (_size == _arr.Length)
                throw new InvalidOperationException("Message queue is full");

            _arr[_nextFree++] = message;
            _size++;

            if (_nextFree >= _arr.Length)
            {
                _nextFree = 0;
            }
        }

        public bool TryDequeue()
        {
            if (_size == 0)
                return false;

            int ret = _slot++;
            _size--;

            if (_slot >= _arr.Length)
            {
                _slot = 0;
            }


            LastMessage = _arr[ret];

            return true;
        }

        public void RemoveReferences(Component component)
        {
            if (LastMessage.Component == component)
            {
                LastMessage.Component = null;
            }

            var backupLastMessage = LastMessage;
            int n = _size;

            while (--n >= 0)
            {
                TryDequeue();

                if (LastMessage.Component != component)
                {
                    Enqueue(LastMessage);
                }
            }

            LastMessage = backupLastMessage;
        }

        public bool HasMessage(int id)
        {
            return _arr.Any(m => m.Id == id);
        }

        public void RemoveAll(int id)
        {
            int n = _size;

            while (--n >= 0)
            {
                TryDequeue();
                if (LastMessage.Id != id)
                    Enqueue(LastMessage);
            }
        }
    }

    internal class MessageLogger
    {
        private readonly ICollection<string> output;
        private Queue<(Component cmh, Message msg, bool handled)> queue;
        private DateTime lastLogTime;

        public MessageLogger(ICollection<string> output)
        {
            this.output = output;
            this.queue = new Queue<(Component cmh, Message msg, bool handled)>();
            lastLogTime = DateTime.Now;
        }

        public void Log(Component cmh, Message msg, bool handled)
        {
            var logTime = DateTime.Now;
            queue.Enqueue((cmh, msg, handled));
            if ((logTime - lastLogTime).TotalSeconds >= 2)
            {
                output.Add(lastLogTime.ToLongTimeString());
                while (queue.Count > 0)
                {
                    (cmh, msg, handled) = queue.Dequeue();
                    output.Add($"\t\t{(handled ? "TRUE" : "FALSE")}\t:: {msg.Id},\t\tCMH: {(cmh is null ? "NULL" : cmh.GetType().Name + " " + cmh.Id)},\t\tPARAMS: {msg.ParamA}, {msg.ParamB}, {msg.ParamC}, {msg.ParamD}");
                }
                output.Add(Environment.NewLine);
                lastLogTime = logTime;
            }
        }
    }

    public class Cex
    {
        private Component _capture;
        bool _isDispatching;

        private List<Listener> _listeners;
        private MessageQueue _queue;
        public int _currentListener;
        public Component Scope { get; set; }
        public int Field_0x8 { get; set; }

#if DEBUG
        public static List<string> MessageLog { get; } = new List<string>();
        private static MessageLogger logger = new MessageLogger(MessageLog);
#endif
        public struct Listener : IComparable<int>
        {
            public int Priority;
            public MessageFlags Kinds;
            public Component Component;

            public int CompareTo(int other)
            {
                return Priority.CompareTo(other);
            }
        }

        public IEnumerable<Listener> GetListeners() => _listeners;

        private IOpen3dmmMouse Mouse => Application.Current.Window.Input.Mouse;

        public Cex()
        {
            _listeners = new List<Listener>(16);
            _queue = new(32);
        }

        public virtual void Enqueue(Message message)
        {
            _queue.Enqueue(message);
        }

        // VirtualFunc2
        public virtual bool FindIndexOfPriority(int priority, out int index) => BinarySearchUtils.First(CollectionsMarshal.AsSpan(_listeners), priority, out index);

        // VirtualFunc3
        public virtual bool InScope(Component component)
        {
            return Scope is null || Scope == component || Scope is Gob gob && gob.IsAnscestorOf(component as Gob);
        }

        public bool GetMessageHandler(Component component, out Component messageHandler)
        {
            if (Scope != null)
            {
                var gob = component as Gob;

                while (gob != Scope)
                {
                    if (gob == null)
                    {
                        messageHandler = null;
                        return false;
                    }
                    gob = gob.Parent;
                }
            }

            return (messageHandler = component as Component) is not null;
        }

        // VirtualFunc8
        public virtual bool AddListener(Component obj, int id, MessageFlags flags)
        {
            if ((flags & (MessageFlags.Self | MessageFlags.Broadcast | MessageFlags.Other)) == MessageFlags.None || !InScope(obj))
            {
                return false;
            }

            FindIndexOfPriority(id, out int index);
            _listeners.Insert(index, new Listener() { Priority = id, Component = obj, Kinds = flags });

            if (index <= _currentListener)
            {
                _currentListener++;
            }

            return true;
        }

        public virtual void RemoveReferences(Component component)
        {
            if (Scope == component)
            {
                Scope = null;
            }

            if (_capture == component)
            {
                _capture = null;
            }

            int n = _listeners.Count;

            while (--n >= 0)
            {
                if (_listeners[n].Component == component)
                {
                    RemoveListenerAt(n);
                }
            }

            _queue.RemoveReferences(component);
        }

        public Component Capture
        {
            get => _capture;
            set
            {
                _capture = value;
                if (!Mouse.SetCapture(_capture is not null))
                    throw new InvalidOperationException("Failed to capture the mouse.");
            }
        }

        public virtual void ReleaseCapture()
        {
            _capture = null;
            Mouse.SetCapture(false);
        }

        // VirtualFunc9
        public virtual bool RemoveListener(Component component, int priority)
        {
            if (!FindIndexOfPriority(priority, out int i))
            {
                return false;
            }

            while (true)
            {
                if (_listeners[i].Component == component)
                {
                    RemoveListenerAt(i);
                    return true;
                }

                if (++i >= _listeners.Count || _listeners[i].Priority != priority)
                {
                    return false;
                }
            }
        }

        private void RemoveListenerAt(int index)
        {
            _listeners.RemoveAt(index);
            if (_currentListener > index)
            {
                _currentListener--;
            }
        }

        public virtual bool Dispatch()
        {
            if (_isDispatching)
            {
                return false;
            }

            try
            {
                _isDispatching = true;
                return DoDispatch();
            }
            finally
            {
                _isDispatching = false;
            }
        }

        private bool DoDispatch()
        {
            int moveNext = MoveNext();

            if (moveNext != 1)
            {
                return moveNext != 0;
            }

            _currentListener = 0;

            while (_currentListener < _listeners.Count)
            {
                var listener = _listeners[_currentListener++];
                var component = listener.Component;
                var isValid = (_queue.LastMessage.ComputeFlags(component) & listener.Kinds) != 0;

                if (isValid && TryDispatchCurrent(component))
                {
                    return true;
                }
            }

            if (_queue.LastMessage.Component != null)
            {
                TryDispatchCurrent(_queue.LastMessage.Component);
            }

            return true;
        }

        static bool mouseButtonToggle;

        public virtual int MoveNext()
        {
            // Gets current Message
            if (!_queue.TryDequeue())
            {
                if (_capture is not Gob gob)
                    return 0;
                var mpos = gob.GetCursorPosition();
                var input = Application.Current.InputState;
                if (!Application.Current.AppState.HasFlag(AppState.Focused))
                {
                    if (mouseButtonToggle = !mouseButtonToggle)
                        input &= ~InputState.LeftButton;
                    else
                        input ^= InputState.LeftButton;
                }
                _queue.LastMessage = new((int)KnownMessageValues.MouseDragMaybe, gob, mpos.X, mpos.Y, (int)input);
            }

            if (Field_0x8 == 2 && _queue.LastMessage.Id != (int)KnownMessageValues.App_UnknownTerminator && !FUN_0041ea00(_queue.LastMessage))
                return 2;

            if (!InScope(_queue.LastMessage.Component))
            {
                Application.Current.PromoteMessage(_queue.LastMessage);
                return 2;
            }
            return 1;
        }

        private bool FUN_0041ea00(in Message message)
        {
            throw new NotImplementedException();
        }

        protected virtual bool TryDispatchCurrent(Component component)
        {
            var success = GetMessageHandler(component, out var messageHandler) && messageHandler.Handle(_queue.LastMessage);
            if (!success && component == _queue.LastMessage.Component)
                logger.Log(component, _queue.LastMessage, success);
            return success;
        }

        public virtual bool Exists(int id)
        {
            return _queue.HasMessage(id);
        }

        public virtual void Clear(int id)
        {
            _queue.RemoveAll(id);
        }
    }
}
