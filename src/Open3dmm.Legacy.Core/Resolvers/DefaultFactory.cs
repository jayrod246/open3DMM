using Open3dmm.Core.Data;
using System;
using System.Linq;

namespace Open3dmm.Core.Resolvers
{
    public class DefaultFactory : IFactory
    {
        Type[] PrivateConstructors = new[]
        {
            typeof(GenericStrings),
            typeof(GenericStrings<>),
            typeof(GenericGroup),
            typeof(GenericGroup<>),
            typeof(GenericList),
        };

        public T Create<T>(CacheMetadata info)
        {
            T obj;
            if (PrivateConstructors.Any(t => t.IsAssignableFrom(typeof(T))))
                obj = (T)Activator.CreateInstance(typeof(T), true);
            else
                obj = Activator.CreateInstance<T>();
            (obj as IResolvableObject).Initialize(info);
            return obj;
        }

        //private readonly Dictionary<Type, Func<ResolveInfo, object>> factoryCache = new Dictionary<Type, Func<ResolveInfo, object>>();

        //public T Create<T>(ResolveInfo info) where T : IResolvable
        //{
        //    if (!factoryCache.TryGetValue(typeof(T), out var factoryFunc))
        //        factoryCache[typeof(T)] = factoryFunc = GetFactoryDelegateFromType(typeof(T));
        //    return (T)factoryFunc(info);
        //}

        //private static Func<ResolveInfo, object> GetFactoryDelegateFromType(Type type)
        //{
        //    var factoryMethod = GetFactoryMethodFromType(type);
        //    if (factoryMethod == null)
        //    {
        //        if (!type.IsInterface || !TryGetFactoryMethodFromInterface(type, out factoryMethod))
        //            foreach (var impl in type.GetInterfaces())
        //                if (TryGetFactoryMethodFromInterface(impl, out factoryMethod))
        //                    break;

        //        if (factoryMethod == null)
        //            throw ThrowHelper.NoFactoryMethod(type);
        //    }
        //    return (Func<ResolveInfo, object>)factoryMethod.CreateDelegate(typeof(Func<ResolveInfo, object>));
        //}

        //private static bool TryGetFactoryMethodFromInterface(Type impl, out MethodInfo factoryMethod)
        //{
        //    if (impl.IsGenericType && impl.GetGenericTypeDefinition() == typeof(IList<>))
        //    {
        //        factoryMethod = GetFactoryMethodFromType(typeof(GenericList<>).MakeGenericType(impl.GetGenericArguments()));
        //        return true;
        //    }
        //    factoryMethod = null;
        //    return false;
        //}

        //private static MethodInfo GetFactoryMethodFromType(Type type)
        //{
        //    return type.GetMethod("Factory", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod);
        //}
    }
}
