using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace GameEngine.Engine
{
    public class TextureManager : IDisposable
    {
        readonly Dictionary<TextureKey, Texture> textures;
        readonly Dictionary<string, Texture> hdrTextures;

        public Texture White { get; private set; }
        public Texture Grey { get; private set; }
        public Texture Black { get; private set; }
        public Texture FlatNormal { get; private set; }
        public Texture WhiteSrgb { get; private set; }
        public Texture GreySrgb { get; private set; }
        public Texture BlackSrgb { get; private set; }
        public Texture MetallicRoughnessDefault { get; private set; }
        public Texture AmbientOcclusionDefault { get; private set; }
        public Texture MetallicDefault { get; private set; }
        public Texture RoughnessDefault { get; private set; }

        public TextureManager()
        {
            textures = new Dictionary<TextureKey, Texture>();
            hdrTextures = new Dictionary<string, Texture>(StringComparer.OrdinalIgnoreCase);
        }

        public void InitializeDefaultTextures()
        {
            White = Create1x1(255, 255, 255, 255, TextureColorSpace.Linear);
            Grey = Create1x1(127, 127, 127, 255, TextureColorSpace.Linear);
            Black = Create1x1(0, 0, 0, 255, TextureColorSpace.Linear);
            FlatNormal = Create1x1(128, 128, 255, 255, TextureColorSpace.Linear);

            WhiteSrgb = Create1x1(255, 255, 255, 255, TextureColorSpace.Srgb);
            GreySrgb = Create1x1(127, 127, 127, 255, TextureColorSpace.Srgb);
            BlackSrgb = Create1x1(0, 0, 0, 255, TextureColorSpace.Srgb);

            MetallicRoughnessDefault = Create1x1(0, 128, 0, 255, TextureColorSpace.Linear);
            AmbientOcclusionDefault = Create1x1(255, 255, 255, 255, TextureColorSpace.Linear);
            MetallicDefault = Create1x1(0, 0, 0, 255, TextureColorSpace.Linear);
            RoughnessDefault = Create1x1(128, 128, 128, 255, TextureColorSpace.Linear);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Texture Get(string? path)
        {
            return Get(path, TextureColorSpace.Linear);
        }

        public Texture Get(string? path, TextureColorSpace colorSpace)
        {
            if (string.IsNullOrWhiteSpace(path))
                return colorSpace == TextureColorSpace.Srgb ? WhiteSrgb : White;

            if (ProjectContext.Current == null)
                return colorSpace == TextureColorSpace.Srgb ? WhiteSrgb : White;

            path = Path.GetFullPath(Path.Combine(ProjectContext.Current.RootPath, path));

            var key = new TextureKey(path, colorSpace);
            if (textures.TryGetValue(key, out var tex))
                return tex;

            tex = LoadFromFile(path, colorSpace);
            textures[key] = tex;
            return tex;
        }

        public static Texture LoadFromFile(string imagePath)
        {
            return LoadFromFile(imagePath, TextureColorSpace.Linear);
        }

        public static Texture LoadFromFile(string imagePath, TextureColorSpace colorSpace)
        {
            int handle = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            bool flip = ShouldFlipOnLoad(imagePath);
            StbImage.stbi_set_flip_vertically_on_load(flip ? 1 : 0);

            int width;
            int height;

            using (Stream stream = File.OpenRead(imagePath))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                PixelInternalFormat internalFormat = GetInternalFormat(colorSpace);
                GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
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

        public Texture GetHdr(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Black;

            if (ProjectContext.Current == null)
                return Black;

            string absPath = Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(ProjectContext.Current.RootPath, path));

            if (hdrTextures.TryGetValue(absPath, out var tex))
                return tex;

            tex = LoadHdrFromFile(absPath);
            hdrTextures[absPath] = tex;
            return tex;
        }

        public static Texture LoadHdrFromFile(string imagePath)
        {
            int handle = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            StbImage.stbi_set_flip_vertically_on_load(0);

            int width;
            int height;

            using (Stream stream = File.OpenRead(imagePath))
            {
                ImageResultFloat image = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlue);
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgb16f,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Rgb,
                    PixelType.Float,
                    image.Data
                );
                width = image.Width;
                height = image.Height;
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle, width, height);
        }

        private static bool ShouldFlipOnLoad(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return true;

            string normalized = imagePath.Replace('\\', '/');
            return normalized.IndexOf("/Assets/Textures/Imported/", StringComparison.OrdinalIgnoreCase) < 0;
        }

        public static Texture Create1x1(byte r, byte g, byte b, byte a, TextureColorSpace colorSpace = TextureColorSpace.Linear)
        {
            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            byte[] data = { r, g, b, a };
            PixelInternalFormat internalFormat = GetInternalFormat(colorSpace);
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, 1, 1, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return new Texture(handle, 1, 1);
        }

        private static PixelInternalFormat GetInternalFormat(TextureColorSpace colorSpace)
        {
            return colorSpace == TextureColorSpace.Srgb
                ? PixelInternalFormat.Srgb8Alpha8
                : PixelInternalFormat.Rgba;
        }

        private readonly struct TextureKey : IEquatable<TextureKey>
        {
            public readonly string Path;
            public readonly TextureColorSpace ColorSpace;

            public TextureKey(string path, TextureColorSpace colorSpace)
            {
                Path = path;
                ColorSpace = colorSpace;
            }

            public bool Equals(TextureKey other)
            {
                return ColorSpace == other.ColorSpace
                    && StringComparer.OrdinalIgnoreCase.Equals(Path, other.Path);
            }

            public override bool Equals(object? obj)
            {
                return obj is TextureKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    StringComparer.OrdinalIgnoreCase.GetHashCode(Path),
                    (int)ColorSpace
                );
            }
        }

    }
}
