using System;
using System.Collections.Generic;
using System.IO;

namespace GameEngine.Engine
{
    public class ShaderManager : IDisposable
    {
        public static ShaderManager? Current { get; private set; }

        readonly Dictionary<string, Shader> shaders;

        public Shader Default { get; private set; }

        public ShaderManager()
        {
            shaders = new Dictionary<string, Shader>(StringComparer.OrdinalIgnoreCase);
        }

        public static void SetCurrent(ShaderManager manager)
        {
            Current = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public static void ClearCurrent()
        {
            Current = null;
        }

        public void InitializeDefaultShaders()
        {
            Default = Get(Util.GetDefaultVertPath(), Util.GetDefaultFragPath());
        }

        public Shader Get(string? vertexPath, string? fragmentPath)
        {
            if (string.IsNullOrWhiteSpace(vertexPath) || string.IsNullOrWhiteSpace(fragmentPath))
                return Default;

            string vertexAbs = ResolvePath(vertexPath);
            string fragmentAbs = ResolvePath(fragmentPath);
            string key = vertexAbs + "|" + fragmentAbs;

            if (shaders.TryGetValue(key, out var shader))
                return shader;

            shader = LoadFromFiles(vertexAbs, fragmentAbs);
            shaders[key] = shader;
            return shader;
        }

        public static Shader LoadFromFiles(string vertexPath, string fragmentPath)
        {
            string vertexSource = File.ReadAllText(vertexPath);
            string fragmentSource = File.ReadAllText(fragmentPath);

            return new Shader(vertexSource, fragmentSource);
        }

        public void Dispose()
        {
            foreach (Shader shader in shaders.Values)
            {
                shader.Dispose();
            }

            shaders.Clear();
        }

        static string ResolvePath(string path)
        {
            if (Path.IsPathRooted(path)) return Path.GetFullPath(path);

            if (ProjectContext.Current == null)
                return Path.GetFullPath(path);

            return ProjectContext.Current.Paths.ToAbsolute(path);
        }
    }
}
