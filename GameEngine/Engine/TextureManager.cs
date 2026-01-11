using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace GameEngine.Engine
{
    public class TextureManager : IDisposable
    {
        readonly Dictionary<string, Texture> textures;

        public Texture White { get; private set; }
        public Texture Grey { get; private set; }
        public Texture Black { get; private set; }
        public Texture FlatNormal { get; private set; }

        public TextureManager()
        {
            textures = new Dictionary<string, Texture>(StringComparer.OrdinalIgnoreCase);
        }

        public void InitializeDefaultTextures()
        {
            White = Create1x1(255, 255, 255, 255);
            Grey = Create1x1(127, 127, 127, 255);
            Black = Create1x1(0, 0, 0, 255);
            FlatNormal = Create1x1(128, 128, 255, 255);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Texture Get(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return White;
            if (ProjectContext.Current == null) return White;

            path = Path.GetFullPath(Path.Combine(ProjectContext.Current.RootPath, path));

            if (textures.TryGetValue(path, out var tex))
                return tex;

            tex = LoadFromFile(path);
            textures[path] = tex;
            return tex;
        }

        public static Texture LoadFromFile(string imagePath)
        {
            int handle = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            StbImage.stbi_set_flip_vertically_on_load(1);

            int width;
            int height;

            using (Stream stream = File.OpenRead(imagePath))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                width = image.Width;
                height = image.Height;
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle, width, height);
        }

        public static Texture Create1x1(byte r, byte g, byte b, byte a)
        {
            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            byte[] data = { r, g, b, a };
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return new Texture(handle, 1, 1);
        }

    }
}
