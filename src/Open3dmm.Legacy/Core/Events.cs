using System;
using System.Runtime.CompilerServices;

namespace Open3dmm
{
    internal static class Events
    {
        internal static MessageCallbackCollection GetCallbacks(Type type)
        {
            return (MessageCallbackCollection)typeof(Events<>).MakeGenericType(type).GetField("Callbacks").GetValue(null);
        }
    }

    public static class Events<T>
    {
        public static MessageCallbackCollection Callbacks;

        static Events()
        {
            if (typeof(T).IsAssignableTo(typeof(Component)))
            {
                Callbacks = new(Events.GetCallbacks(typeof(T).BaseType));
                RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
            }
        }

        public static void Init(params MessageCallbackDescriptor[] descriptors) => Callbacks.Descriptors = descriptors;
    }
}
