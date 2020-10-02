using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Open3dmm
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class ComponentPlatformAttribute : Attribute
    {
        public ComponentPlatformAttribute(Type type) => Type = type;

        public Type Type { get; }
    }

    public interface IComponentPlatform
    {
        IErrorService Errors { get; }
        Cex ComponentExchange { get; }
        Component FindComponent(int cid);
        int GenerateComponentIds(int count);
    }

    public static class Errors
    {
        static readonly IErrorService _service;

        public static void Throw(int error) => _service.Throw(error);
        public static bool LastError(out int error)
        {
            error = 0;
            return false;
        }
        public static bool IsThrown(int error) => _service.IsThrown(error);
        public static void Flush() { }
        public static IEnumerable<int> Enumerate() => _service.Enumerate();
    }

    public interface IErrorService
    {
        void Throw(int errorCode);
        bool LastError(out int errorCode);
        bool IsThrown(int errorCode);
        void Flush();
        IEnumerable<int> Enumerate();
    }
}