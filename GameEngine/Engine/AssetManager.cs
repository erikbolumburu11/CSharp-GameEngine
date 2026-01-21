using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SharpGLTF.Schema2;

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

    public readonly record struct MeshPrimitiveRef(Guid ModelGuid, int MeshIndex, int PrimitiveIndex);

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
        private static readonly Dictionary<Guid, MeshPrimitiveRef> meshPrimitiveGuidToRef = new();
        private static readonly Dictionary<MeshPrimitiveRef, Guid> meshPrimitiveRefToGuid = new();
        private static readonly Dictionary<Guid, string> meshPrimitiveGuidToLabel = new();
        private static bool meshPrimitiveCacheBuilt;

        public static void RegisterVirtualAsset(Guid guid, Func<object> factory)
        {
            guidToBuiltIn[guid] = factory;
        }

        public static void ScanAssets(string assetsRoot)
        {
            guidToPath.Clear();
            pathToGuid.Clear();
            meshPrimitiveGuidToRef.Clear();
            meshPrimitiveRefToGuid.Clear();
            meshPrimitiveGuidToLabel.Clear();
            meshPrimitiveCacheBuilt = false;

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
            if (typeof(T) == typeof(ModelRoot))
            {
                return (T)(object)ModelRoot.Load(path);
            }

            throw new NotSupportedException(
                $"Asset type {typeof(T).Name} is not supported"
            );
        }

        public static Guid GetMeshPrimitiveGuid(Guid modelGuid, int meshIndex, int primitiveIndex)
        {
            var key = new MeshPrimitiveRef(modelGuid, meshIndex, primitiveIndex);
            if (meshPrimitiveRefToGuid.TryGetValue(key, out var existing))
                return existing;

            Guid guid = CreateMeshPrimitiveGuid(key);
            meshPrimitiveRefToGuid[key] = guid;
            meshPrimitiveGuidToRef[guid] = key;
            return guid;
        }

        public static bool TryGetMeshPrimitiveRef(Guid primitiveGuid, out MeshPrimitiveRef primitiveRef)
        {
            if (meshPrimitiveGuidToRef.TryGetValue(primitiveGuid, out primitiveRef))
                return true;

            EnsureMeshPrimitiveCache();
            return meshPrimitiveGuidToRef.TryGetValue(primitiveGuid, out primitiveRef);
        }

        public static void RegisterModelPrimitives(Guid modelGuid, ModelRoot model, string? modelPath = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            string displayPath = modelPath ?? (guidToPath.TryGetValue(modelGuid, out var path) ? path : modelGuid.ToString());
            string modelLabel = GetModelLabel(displayPath, model);

            foreach (var mesh in model.LogicalMeshes)
            {
                foreach (var primitive in mesh.Primitives)
                {
                    Guid primitiveGuid = GetMeshPrimitiveGuid(
                        modelGuid,
                        mesh.LogicalIndex,
                        primitive.LogicalIndex
                    );

                    string label = $"{modelLabel}:Prim[{primitive.LogicalIndex}]";
                    meshPrimitiveGuidToLabel[primitiveGuid] = label;
                }
            }
        }

        public static string GuidToPath(Guid guid)
        {
            if (guidToPath.TryGetValue(guid, out var path))
                return path;

            if (meshPrimitiveGuidToLabel.TryGetValue(guid, out var label))
                return label;

            throw new KeyNotFoundException($"Unknown asset GUID: {guid}");
        }

        public static bool TryGetPath(Guid guid, out string path)
        {
            return guidToPath.TryGetValue(guid, out path);
        }

        public static Guid PathToGuid(string path) => pathToGuid[path];

        private static Guid CreateMeshPrimitiveGuid(MeshPrimitiveRef key)
        {
            string payload = $"{key.ModelGuid:N}:{key.MeshIndex}:{key.PrimitiveIndex}";
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return new Guid(hash);
        }

        private static void EnsureMeshPrimitiveCache()
        {
            if (meshPrimitiveCacheBuilt)
                return;

            foreach (var entry in guidToPath)
            {
                string path = entry.Value;
                if (!IsModelAssetPath(path))
                    continue;

                if (!TryLoad(entry.Key, out ModelRoot model))
                    continue;

                RegisterModelPrimitives(entry.Key, model, path);
            }

            meshPrimitiveCacheBuilt = true;
        }

        private static bool IsModelAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            string ext = Path.GetExtension(path);
            return ext.Equals(".gltf", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".glb", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetModelLabel(string displayPath, ModelRoot model)
        {
            if (!string.IsNullOrWhiteSpace(displayPath))
            {
                try
                {
                    string name = Path.GetFileNameWithoutExtension(displayPath);
                    if (!string.IsNullOrWhiteSpace(name))
                        return name;
                }
                catch
                {
                    // Fall through to the model name.
                }
            }

            if (model.DefaultScene != null && !string.IsNullOrWhiteSpace(model.DefaultScene.Name))
                return model.DefaultScene.Name;

            return "Model";
        }
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
