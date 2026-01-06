using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class Shader : IDisposable
    {
        public int Handle { get; private set; }

        readonly Dictionary<string, int> uniformLocations;

        public Shader(int handle)
        {
            Handle = handle;

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                var location = GL.GetUniformLocation(Handle, key);

                uniformLocations.Add(key, location);
            }
        }

        int GetUniformLocation(string name)
        {
            if (uniformLocations.TryGetValue(name, out int location))
                return location;

            location = GL.GetUniformLocation(Handle, name);
            uniformLocations[name] = location; // cache (may be -1 if optimized out)
            return location;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Use(Matrix4 transform)
        {
            GL.UseProgram(Handle);

            int location = GetUniformLocation("transform");
            if (location == -1) return;

            GL.UniformMatrix4(location, true, ref transform);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            int location = GetUniformLocation(name);
            if (location == -1) return;
            GL.Uniform1(location, data);
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            int location = GetUniformLocation(name);
            if (location == -1) return;
            GL.Uniform1(location, data);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            int location = GetUniformLocation(name);
            if (location == -1) return;
            GL.UniformMatrix4(location, true, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            int location = GetUniformLocation(name);
            if (location == -1) return;
            GL.Uniform3(location, data);
        }

        public void SetVector2(string name, Vector2 data)
        {
            GL.UseProgram(Handle);
            int location = GetUniformLocation(name);
            if (location == -1) return;
            GL.Uniform2(location, data);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            if (disposedValue == false)
            {
                Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
