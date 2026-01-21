using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace GameEngine.Engine
{
    public enum TextureColorSpace
    {
        Srgb,
        Linear
    }

    public class Texture : IDisposable
    {
        private readonly int handle;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture(int glHandle, int width, int height)
        {
            handle = glHandle;
            Width = width;
            Height = height;
        }

        public void Dispose()
        {
            if (handle != 0)
                GL.DeleteTexture(handle);
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }
    }
}
