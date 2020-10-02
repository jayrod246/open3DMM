using Open3dmm.Core.Resolvers;
using System;
using System.Runtime.CompilerServices;

namespace Open3dmm
{
    public static class ResolverUtils
    {
        public static bool ResolveChildren(IResolver resolver, ChunkIdentifier id, int chid, Tags tag, Func<IScopedResolver, ChunkIdentifier, bool> resolveDelegate) 
            => tag is 0 ? ResolveChildren(resolver, id, chid, resolveDelegate) : ResolveChildrenSpecified(resolver, id, chid, tag, resolveDelegate);

        public static bool ResolveChildren(IResolver resolver, ChunkIdentifier id, int chid, Func<IScopedResolver, ChunkIdentifier, bool> resolveDelegate)
        {
            var scope = resolver.ScopeOf(id);
            var c = scope.File.GetChunk(id);

            foreach (var child in c.Children)
            {
                if (child.Chid == chid)
                {
                    if (resolveDelegate(scope, child.Identifier))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ResolveChildrenSpecified(IResolver resolver, ChunkIdentifier id, int chid, Tags tag, Func<IScopedResolver, ChunkIdentifier, bool> resolveDelegate)
        {
            var scope = resolver.ScopeOf(id);
            var c = scope.File.GetChunk(id);

            foreach (var child in c.Children)
            {
                if (child.Chid == chid && child.Identifier.Tag == tag)
                {
                    if (resolveDelegate(scope, child.Identifier))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
