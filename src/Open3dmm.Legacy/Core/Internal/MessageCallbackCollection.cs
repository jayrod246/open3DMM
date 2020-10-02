using System;
using System.Runtime.CompilerServices;

namespace Open3dmm
{
    public sealed class MessageCallbackCollection
    {
        public readonly MessageCallbackCollection Base;
        public MessageCallbackDescriptor[] Descriptors = Array.Empty<MessageCallbackDescriptor>();

        internal MessageCallbackCollection(MessageCallbackCollection @base)
        {
            Base = @base;
        }

        internal bool Invoke(Component component, Message message)
        {
            ref var cb = ref GetDescriptor(message, message.ComputeFlags(component), out var exists);
            var del = exists ? cb.Delegate : null;
            return del?.Invoke(component, message) ?? false;
        }

        internal ref MessageCallbackDescriptor GetDescriptor(Message message, MessageFlags flags, out bool exists)
        {
            exists = false;
            MessageCallbackCollection callbacks = this;
            ref var fallback = ref Unsafe.NullRef<MessageCallbackDescriptor>();

            while (callbacks != null)
            {
                foreach (ref var cb in callbacks.Descriptors.AsSpan())
                {
                    if (cb.Message is 0)
                    {
                        if (!exists)
                        {
                            fallback = ref cb;
                            exists = true;
                        }
                    }
                    else if (cb.Message == message.Id && (cb.Flags & flags) != 0)
                    {
                        exists = true;
                        return ref cb;
                    }
                }

                callbacks = callbacks.Base;
            }

            return ref fallback;
        }
    }
}
