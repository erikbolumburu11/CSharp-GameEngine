namespace GameEngine.Engine
{
    public class MaterialManager : IDisposable
    {
        readonly Dictionary<string, Material> materials;

        public Material defaultMat;

        public MaterialManager()
        {
            materials = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
        }

        public void InitializeDefaultMaterials()
        {
            defaultMat = new Material();
        }

        public Material Get(Guid guid)
        {
            if (guid == Guid.Empty) return defaultMat;

            if(AssetDatabase.TryLoad(guid, out Material mat)){
                return mat;
            }

            return defaultMat;

            // string relPath = path;
            // if (Path.IsPathRooted(relPath))
            // {
            //     if (ProjectContext.Current == null)
            //         throw new InvalidOperationException("Cannot resolve material path without an active project.");

            //     relPath = ProjectContext.Current.Paths.ToProjectRelative(relPath);
            // }

            // relPath = NormalizeRelPath(relPath);

            // if (materials.TryGetValue(relPath, out var mat))
            //     return mat;

            // mat = MaterialSerializer.LoadMaterial(relPath);
            // materials[relPath] = mat;
            // return mat;
        }

        public void Add(string relPath, Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));
            if (string.IsNullOrWhiteSpace(relPath))
                throw new ArgumentException("Material path is empty.", nameof(relPath));

            relPath = NormalizeRelPath(relPath);
            material.relPath = relPath;
            materials[relPath] = material;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private static string NormalizeRelPath(string relPath)
        {
            relPath = relPath.Replace('\\', '/').Trim();
            while (relPath.StartsWith("./", StringComparison.Ordinal)) relPath = relPath.Substring(2);
            while (relPath.StartsWith("/", StringComparison.Ordinal)) relPath = relPath.Substring(1);
            return relPath;
        }
    }
}
