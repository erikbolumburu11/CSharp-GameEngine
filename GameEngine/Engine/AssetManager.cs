using System.Text.Json;

namespace GameEngine.Engine
{
    public static class BuiltInGuids
    {
        public static readonly Guid WhiteTexture =
            new("00000000-0000-0000-0000-000000000001");

        public static readonly Guid GreyTexture =
            new("00000000-0000-0000-0000-000000000002");

        public static readonly Guid BlackTexture =
            new("00000000-0000-0000-0000-000000000003");

        public static readonly Guid FlatNormal =
            new("00000000-0000-0000-0000-000000000004");
    }

    public class AssetManager
    {
        TextureManager textureManager;
        MaterialManager materialManager;

        public AssetManager(TextureManager textureManager, MaterialManager materialManager)
        {
            this.textureManager = textureManager;
            this.materialManager = materialManager;
        }

        public void RegisterBuiltInTextures()
        {
            AssetDatabase.RegisterVirtualAsset(
                BuiltInGuids.WhiteTexture,
                () => textureManager.White
            );

            AssetDatabase.RegisterVirtualAsset(
                BuiltInGuids.GreyTexture,
                () => textureManager.Grey
            );

            AssetDatabase.RegisterVirtualAsset(
                BuiltInGuids.BlackTexture,
                () => textureManager.Black
            );

            AssetDatabase.RegisterVirtualAsset(
                BuiltInGuids.FlatNormal,
                () => textureManager.FlatNormal
            );
        }
    }

    public static class AssetDatabase
    {
        private static readonly Dictionary<Guid, object> cache = new();
        private static readonly Dictionary<Guid, string> guidToPath = new();
        private static readonly Dictionary<Guid, Func<object>> guidToBuiltIn = new();
        private static readonly Dictionary<string, Guid> pathToGuid = new();

        public static void RegisterVirtualAsset(Guid guid, Func<object> factory)
        {
            guidToBuiltIn[guid] = factory;
        }

        public static void ScanAssets(string assetsRoot)
        {
            guidToPath.Clear();
            pathToGuid.Clear();

            foreach
            (
                string assetPath in Directory.EnumerateFiles(
                assetsRoot, "*.*", SearchOption.AllDirectories)
            )
            {
                if (assetPath.EndsWith(".meta")) continue;

                AssetMeta meta = MetaFile.LoadOrCreate(assetPath);

                guidToPath[Guid.Parse(meta.guid)] = assetPath;
                pathToGuid[assetPath] = Guid.Parse(meta.guid);
            }
        }

        public static bool IsVirtual(Guid guid)
            => guidToBuiltIn.ContainsKey(guid);

        public static T Load<T>(Guid guid)
        {
            if(cache.TryGetValue(guid, out var cached))
                return (T)cached;

            if (guidToBuiltIn.TryGetValue(guid, out var factory))
                return (T)factory();

            if (!guidToPath.ContainsKey(guid))
                throw new KeyNotFoundException($"Unknown asset GUID: {guid}");

            string path = guidToPath[guid];
            return LoadFromDisk<T>(path);
        }

        public static bool TryLoad<T>(Guid guid, out T asset)
        {
            if (guid == Guid.Empty)
            {
                asset = default!;
                return false;
            }

            if (cache.TryGetValue(guid, out var cached))
            {
                if (cached is T cachedAsset)
                {
                    asset = cachedAsset;
                    return true;
                }

                asset = default!;
                return false;
            }

            if (guidToBuiltIn.TryGetValue(guid, out var factory))
            {
                object builtIn = factory();
                if (builtIn is T builtInAsset)
                {
                    asset = builtInAsset;
                    return true;
                }

                asset = default!;
                return false;
            }

            if (!guidToPath.TryGetValue(guid, out var path))
            {
                asset = default!;
                return false;
            }

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                asset = default!;
                return false;
            }

            try
            {
                asset = LoadFromDisk<T>(path);
                cache[guid] = asset!;
                return true;
            }
            catch
            {
                asset = default!;
                return false;
            }
        }

        private static T LoadFromDisk<T>(string path)
        {
            if (typeof(T) == typeof(Texture))
            {
                return (T)(object)TextureManager.LoadFromFile(path);
            }

            if (typeof(T) == typeof(Material))
            {
                return (T)(object)MaterialSerializer.LoadMaterial(path);
            }

            throw new NotSupportedException(
                $"Asset type {typeof(T).Name} is not supported"
            );
        }

        public static string GuidToPath(Guid guid) => guidToPath[guid];
        public static Guid PathToGuid(string path) => pathToGuid[path];
    }


    public class AssetMeta
    {
        public string guid { get; set; } = default!;
    }

    public static class MetaFile
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        public static AssetMeta LoadOrCreate(string assetPath)
        {
            string metaPath = assetPath + ".meta";

            if (File.Exists(metaPath))
            {
                string json = File.ReadAllText(metaPath);
                return JsonSerializer.Deserialize<AssetMeta>(json)!;
            }

            var meta = new AssetMeta
            {
                guid = Guid.NewGuid().ToString("N")
            };

            string metaJson = JsonSerializer.Serialize(meta, Options);
            File.WriteAllText(metaPath, metaJson);

            return meta;
        }
    }

}
