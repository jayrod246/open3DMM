using Open3dmm;
using System;
using System.Reflection;

namespace Open3dmm.Core.Resolvers
{
    public static class Resolvable<TValue>
    {
        public static IResolvable<TValue> Default { get; }

        static Resolvable()
        {
            var type = typeof(TValue).GetNestedType("Resolvable", BindingFlags.Public | BindingFlags.NonPublic);

            if (type != null)
                Default = Activator.CreateInstance(type) as IResolvable<TValue>;
        }
    }
}
