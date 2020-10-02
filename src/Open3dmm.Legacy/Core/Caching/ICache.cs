//using System;
//using System.Collections.Generic;

//namespace Open3dmm.Caching
//{
//    public interface ICache
//    {
//        bool Resolve3cn<TValue>(int product, Tag type, int number, IResolvable<TValue> resolvable, out TValue value);
//        bool Resolve3cn<TValue>(int product, Tag type, int number, int childId, int childType, IResolvable<TValue> resolvable, out TValue value);
//        bool Resolve3th<TValue>(int product, Tag type, int number, int childId, int childType, IResolvable<TValue> resolvable, out TValue value);

//        bool Resolve3cn<TValue>(int product, Tag type, int number, out TValue value) where TValue : IResolvable;
//        bool Resolve3th<TValue>(int product, Tag type, int number, out TValue value) where TValue : IResolvable; bool Resolve3th<TValue>(int product, Tag type, int number, IResolvable<TValue> resolvable, out TValue value);

//        bool Resolve3cn<TValue>(int product, Tag type, int number, int childId, Tag childType, out TValue value) where TValue : IResolvable;
//        bool Resolve3th<TValue>(int product, Tag type, int number, int childId, Tag childType, out TValue value) where TValue : IResolvable;
//    }

//    public interface IResolvable
//    {
//        bool Resolve(IResolver resolver, (Tag tag, int number) key);
//    }

//    public interface IResolvable<TValue>
//    {
//        bool Resolve(IResolver resolver, (Tag tag, int number) key, out TValue value);
//    }

//    public interface IResolver
//    {
//        ICache Cache { get; }

//        bool ScopeOf(Tag type, int number, out IScopedResolver scopedResolver);
//        bool Resolve<TValue>(Tag type, int number, IResolvable<TValue> resolvable, out TValue value);
//        bool Resolve<TValue>(Tag type, int number, out TValue value) where TValue : IResolvable;
//        bool Resolve<TValue>(Tag type, int number, int childId, Tag childType, IResolvable<TValue> resolvable, out TValue value);
//        bool Resolve<TValue>(Tag type, int number, int childId, Tag childType, out TValue value) where TValue : IResolvable;
//    }

//    public static class ResolverExtensions
//    {
//        public static bool Resolve<TValue>(this IResolver r, (Tag type, int number) parent, (int id, Tag type) child, IResolvable<TValue> resolvable, out TValue value)
//        {
//            return r.Resolve(parent.type, parent.number, child.id, child.type, resolvable, out value);
//        }

//        public static bool Resolve<TValue>(this IResolver r, (Tag type, int number) key, IResolvable<TValue> resolvable, out TValue value)
//        {
//            return r.Resolve(key.type, key.number, resolvable, out value);
//        }

//        public static bool Resolve<TValue>(this IResolver r, (Tag type, int number) key, out TValue value) where TValue : IResolvable
//        {
//            return r.Resolve(key.type, key.number, out value);
//        }

//        public static bool Resolve<TValue>(this IResolver r, (Tag type, int number) parent, (int id, Tag type) child, out TValue value) where TValue : IResolvable
//        {
//            return r.Resolve(parent.type, parent.number, child.id, child.type, out value);
//        }
//    }

//    public interface IScopedResolver : IResolver
//    {
//    }

//    public static class Text3D
//    {
//        public class Template : Actor.Template
//        {
//            public Font Font { get; }
//        }

//        public class Font : IResolvable
//        {
//        }
//    }

//    public class Actor
//    {
//        public class Template : IResolvable
//        {
//        }

//        private CacheMetadataToken _templateToken;
//        private Template _template;



//        public Actor(Template template)
//        {
//            Update(CacheMetadata.GetOrThrow(template));
//        }

//        private void EnsureUpdated()
//        {
//            if (_templateToken.IsOutdated)
//                Update(_templateToken.Metadata);
//        }

//        private void Update(CacheMetadata m)
//        {
//            _templateToken = new(m);
//            m.Resolver.Resolve(m.Key, out _template);
//        }
//    }

//    public class CacheMetadata
//    {
//        public int ProductId { get; init; }
//        public IResolver Resolver { get; init; }
//        public (Tag, int) Key { get; init; }



//        internal int _version;

//        public void Invalidate()
//        {
//            _version++;
//        }



//        private static readonly Dictionary<object, CacheMetadata> _allMetadata = new(128);

//        public static void Register(object cacheObject, CacheMetadata cacheMetadata)
//        {
//            _allMetadata[cacheObject] = cacheMetadata;
//        }

//        public static bool TryGet(object cacheObject, out CacheMetadata metadata)
//        {
//            return _allMetadata.TryGetValue(cacheObject, out metadata);
//        }

//        public static CacheMetadata GetOrThrow(object cacheObject)
//        {
//            return TryGet(cacheObject, out var metadata) ? metadata : throw new InvalidOperationException("Missing cache metadata for object.");
//        }
//    }

//    public readonly struct CacheMetadataToken
//    {
//        private readonly int _version;


//        public CacheMetadata Metadata { get; }
//        public bool IsOutdated => _version < Metadata._version;


//        public CacheMetadataToken(CacheMetadata metadata)
//        {
//            Metadata = metadata;
//            _version = metadata._version;
//        }
//    }
//}
