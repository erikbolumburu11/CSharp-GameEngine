using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace GameEngine.Engine
{
    public class VertexArray
    {
        public int Handle { get; private set; }

        public VertexArray()
        {
            Handle = GL.GenVertexArray();
        }

        public void Bind() => GL.BindVertexArray(Handle);
        public void Unbind() => GL.BindVertexArray(0);

        public void Delete() => GL.DeleteVertexArray(Handle);
    }

    public class VertexBuffer<T> where T : struct
    {
        public int Handle { get; private set; }

        public VertexBuffer(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
        {
            Handle = GL.GenBuffer();
            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * Marshal.SizeOf<T>(), data, usage);
            Unbind();
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
        public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        public void Delete() => GL.DeleteBuffer(Handle);
    }

    public class SSBO
    {
        public int Handle { get; private set; }

        public SSBO(int sizeBytes)
        {
            Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public void Update<T>(T[] data) where T : struct
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, data.Length * Marshal.SizeOf<T>(), data);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public void Bind(int binding)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, Handle);
        }
    }

}
