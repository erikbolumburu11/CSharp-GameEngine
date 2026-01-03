namespace GameEngine.Engine
{
    public class MaterialManager : IDisposable
    {
        Dictionary<string, Material> materials;

        public Material defaultMat;

        public void InitializeDefaultMaterials()
        {
            defaultMat = new Material();
        }

        public Material Get(string path)
        {
            if(path == null) return defaultMat;

            path = Path.GetFullPath(path);

            if (materials.TryGetValue(path, out var mat))
                return mat;

            // mat = LoadFromFile(path);
            // materials[path] = mat;
            // return mat;
            return defaultMat;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}