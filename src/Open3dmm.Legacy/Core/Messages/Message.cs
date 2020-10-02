using System;

namespace Open3dmm
{
    [Flags]
    public enum MessageFlags
    {
        None = 0,
        Self = 1,
        Broadcast = 2,
        Other = 4,
    }

    public delegate bool HandleMessageDelegate(Component component, Message message);
    public delegate bool CanExecuteDelegate(Component component, Message message, out bool canExecute);

    /// <summary>
    /// A descriptor for a message callback function.
    /// </summary>
    public struct MessageCallbackDescriptor
    {
        public MessageCallbackDescriptor(int message, MessageFlags flags, HandleMessageDelegate handleDelegate, CanExecuteDelegate canExecuteDelegate = null)
        {
            Message = message;
            Flags = flags;
            Delegate = handleDelegate;
            CanExecuteDelegate = canExecuteDelegate;
        }

        /// <summary>
        /// Id of the messages to handle. 0 == Fallback to use when no match is found.
        /// </summary>
        public int Message;

        /// <summary>
        /// Flags to further specify what messages are handled.
        /// </summary>
        public MessageFlags Flags;

        /// <summary>
        /// A delegate to the actual method.
        /// </summary>
        public HandleMessageDelegate Delegate;

        /// <summary>
        /// A delegate to the can execute method. It is used by the UI to enable/disable menu items.
        /// </summary>
        public CanExecuteDelegate CanExecuteDelegate;
    }

    public class SenderInfo
    {
        public object Sender { get; set; }
        public string Tag { get; set; }
        public int Number { get; set; }
        public int LineNumber { get; set; }
    }

    public struct Message
    {
        public Component Component;
        public int Id;
        public int ParamA;
        public int ParamB;
        public int ParamC;
        public int ParamD;
        public SenderInfo SenderInfo { get; set; }

        public Message(int id, Component cmh) : this(id, cmh, 0, 0, 0, 0)
        {
        }

        public Message(int id, Component cmh, int paramA) : this(id, cmh, paramA, 0, 0, 0)
        {
        }

        public Message(int id, Component cmh, int paramA, int paramB) : this(id, cmh, paramA, paramB, 0, 0)
        {
        }

        public Message(int id, Component cmh, int paramA, int paramB, int paramC) : this(id, cmh, paramA, paramB, paramC, 0)
        {
        }

        public Message(int id, Component cmh, int paramA, int paramB, int paramC, int paramD)
        {
            Component = cmh;
            Id = id;
            ParamA = paramA;
            ParamB = paramB;
            ParamC = paramC;
            ParamD = paramD;
            SenderInfo = null;
        }

        public MessageFlags ComputeFlags(Component component)
        {
            return Component is null ? MessageFlags.Broadcast : Component == component ? MessageFlags.Self : MessageFlags.Other;
        }
    }
}