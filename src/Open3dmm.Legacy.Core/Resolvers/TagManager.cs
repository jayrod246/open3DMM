using Open3dmm;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Open3dmm.Core.Resolvers
{
    public class TagManager
    {
        public class Product
        {
            public int Key;
            private DataFileGroup _3th;
            private DataFileGroup _3cn;

            public Product(int key, IFactory factory, string rootDir, string alias1, string alias2)
            {
                this.Key = key;
                rootDir = Directory.GetParent(rootDir).FullName;
                string path = Path.Combine(rootDir, alias1);
                if (Directory.Exists(path) || Directory.Exists(path = Path.Combine(rootDir, alias2)))
                {
                    _3th = CollectFiles(path, ".3th", factory, key);
                    _3cn = CollectFiles(path, ".3cn", factory, key);
                }
                else
                {
                    _3th = new DataFileGroup();
                    _3cn = new DataFileGroup();
                }
            }

            public DataFileGroup Get3thModule() => _3th;
            public DataFileGroup Get3cnModule() => _3cn;

            private DataFileGroup CollectFiles(string path, string ext, IFactory factory, int productKey)
            {
                var module = new DataFileGroup();
                int i = 0;
                foreach (var filename in Directory.EnumerateFiles(path))
                {
                    if (Path.GetExtension(filename).ToLowerInvariant() != ext)
                        continue;
                    var file = new ChunkyFile(filename);
                    module.AddFile(new DataFile(file, factory, i++, productKey));
                }
                return module;
            }
        }

        public string RootDirectory { get; }

        public bool TryGet3thIdentifier(GlobalReference reference, Tag refTag, int index, out ChunkIdentifier result)
        {
            result = default;
            return TryGetProduct(reference.ProductKey, out var product) && product.Get3thModule().TryGetIdentifier(reference.Identifier, refTag, index, out result);
        }

        public bool TryGet3cnIdentifier(GlobalReference reference, Tag refTag, int index, out ChunkIdentifier result)
        {
            result = default;
            return TryGetProduct(reference.ProductKey, out var product) && product.Get3cnModule().TryGetIdentifier(reference.Identifier, refTag, index, out result);
        }

        public IEnumerable<int> Keys => products.Values.Select(product => product.Key);
        public IFactory Factory { get; }

        private readonly Dictionary<int, Product> products;
        private readonly Func<string, bool> notFoundHandler;

        public static TagManager Default { get; } = new TagManager(Find3dmmInstallDirectory(), null, 0x200000, new DefaultFactory());

        private static string Find3dmmInstallDirectory()
        {
            if (!Platform.Current.TryFind3dmm(out _, out _, out var exePath))
                throw new InvalidOperationException("3dmm directory could not be found!");
            return Path.GetDirectoryName(exePath);
        }

        public TagManager(string dir, Func<string, bool> notFoundHandler, int unk, IFactory factory)
        {
            RootDirectory = dir;
            this.notFoundHandler = notFoundHandler;
            products = new Dictionary<int, Product>();
            this.Factory = factory;
        }

        public bool TryGetThumIdentifier<T>(GlobalReference reference, out T identifier) where T : unmanaged
        {
            identifier = default;
            if (!TryGetItem(reference, true, out var chunk))
                return false;

            if (Unsafe.SizeOf<T>() != 8)
                return false;

            using var block = BinaryStream.Create(chunk.Section.Memory.Span).Decompress();
            if (!block.MagicNumber())
                return false;

            identifier = block.Read<T>();
            return true;
        }

        public bool RegisterProduct(string aliases, int key)
        {
            aliases = aliases.ToLowerInvariant();
            int separator = aliases.IndexOf('/');
            if (separator < 0)
                throw new ArgumentException("Product should have two aliases.");
            var alias1 = aliases.Substring(0, separator);
            var alias2 = aliases.Substring(separator + 1);
            var product = new Product(key, Factory, RootDirectory, alias1, alias2);
            products.Add(key, product);
            return true;
        }

        public bool TryGetProduct(int key, out Product product)
        {
            return products.TryGetValue(key, out product);
        }

        public bool TryGetItem(GlobalReference reference, bool thum, out Chunk item)
        {
            item = null;
            if (!TryGetProduct(reference.ProductKey, out var product))
                return false;
            var module = thum ? product.Get3thModule() : product.Get3cnModule();
            item = module.ScopeOf(reference.Identifier)?.File.GetChunk(reference.Identifier);
            return item != null;
        }

        public bool TryResolve<T>(GlobalReference reference, bool thum, out T item) where T : IResolvableObject
        {
            if (thum)
                return TryResolve3th(reference, out item);
            return TryResolve3cn(reference, out item);
        }

        public bool TryResolve3th<T>(GlobalReference reference, out T item) where T : IResolvableObject
        {
            return TryResolve3th(reference.ProductKey, reference.Identifier.Tag, reference.Identifier.Number, out item);
        }

        public bool TryResolve3cn<T>(GlobalReference reference, out T item) where T : IResolvableObject
        {
            return TryResolve3cn(reference.ProductKey, reference.Identifier.Tag, reference.Identifier.Number, out item);
        }

        public bool TryResolve3th<T>(GlobalReference reference, Tag refTag, int index, out T item) where T : IResolvableObject
        {
            return TryResolve3th(reference.ProductKey, reference.Identifier.Tag, reference.Identifier.Number, refTag, index, out item);
        }

        public bool TryResolve3cn<T>(GlobalReference reference, Tag refTag, int index, out T item) where T : IResolvableObject
        {
            return TryResolve3cn(reference.ProductKey, reference.Identifier.Tag, reference.Identifier.Number, refTag, index, out item);
        }

        public bool TryResolve3th<T>(int productKey, Tag tag, int number, Tag refTag, int index, out T item) where T : IResolvableObject
        {
            item = default;
            return TryGetProduct(productKey, out var product) && product.Get3thModule().TryResolve(new ChunkIdentifier(tag, number), refTag, index, out item);
        }

        public bool TryResolve3cn<T>(int productKey, Tag tag, int number, Tag refTag, int index, out T item) where T : IResolvableObject
        {
            item = default;
            return TryGetProduct(productKey, out var product) && product.Get3cnModule().TryResolve(new ChunkIdentifier(tag, number), refTag, index, out item);
        }

        public bool TryResolve3th<T>(int productKey, Tag tag, int number, out T item) where T : IResolvableObject
        {
            item = default;
            return TryGetProduct(productKey, out var product) && product.Get3thModule().TryResolve(new ChunkIdentifier(tag, number), out item);
        }

        public bool TryResolve3cn<T>(int productKey, Tag tag, int number, out T item) where T : IResolvableObject
        {
            item = default;
            return TryGetProduct(productKey, out var product) && product.Get3cnModule().TryResolve(new ChunkIdentifier(tag, number), out item);
        }

        public IEnumerable<Product> Products => products.Values;
    }
}
