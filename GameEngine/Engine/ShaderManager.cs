using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Engine
{
    public class ShaderManager : IDisposable
    {
        readonly Dictionary<string, Shader> shaders;

        public Shader? Default { get; private set; }

        public ShaderManager()
        {
            shaders = new Dictionary<string, Shader>(StringComparer.OrdinalIgnoreCase);
        }

        public void InitializeDefaultShaders()
        {
            Default = Get(Util.GetDefaultVertPath(), Util.GetDefaultFragPath());
        }

        public Shader Get(string? vertexPath, string? fragmentPath)
        {
            if (string.IsNullOrWhiteSpace(vertexPath) || string.IsNullOrWhiteSpace(fragmentPath))
            {
                if (Default != null) return Default;
                Default = Get(Util.GetDefaultVertPath(), Util.GetDefaultFragPath());
                return Default;
            }

            string vertexAbs = ResolvePath(vertexPath);
            string fragmentAbs = ResolvePath(fragmentPath);
            string key = BuildKey(vertexAbs, fragmentAbs);

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

            int vertexShader = CompileShader(ShaderType.VertexShader, vertexSource, vertexPath);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource, fragmentPath);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine(infoLog);
            }

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            return new Shader(program);
        }

        static int CompileShader(ShaderType type, string source, string path)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"[{type}] {path} compile error: {infoLog}");
            }

            return shader;
        }

        static string ResolvePath(string path)
        {
            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);

            if (ProjectContext.Current != null)
                return Path.GetFullPath(Path.Combine(ProjectContext.Current.RootPath, path));

            return Path.GetFullPath(path);
        }

        static string BuildKey(string vertexPath, string fragmentPath)
        {
            return vertexPath + "|" + fragmentPath;
        }

        public void Dispose()
        {
            foreach (var shader in shaders.Values)
                shader.Dispose();

            shaders.Clear();
        }
    }
}
