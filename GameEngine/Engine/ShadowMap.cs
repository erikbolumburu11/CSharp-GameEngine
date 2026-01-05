using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public sealed class ShadowMap : IDisposable
    {
        public int Fbo { get; private set; }
        public int DepthTexture { get; private set; }
        public int Size { get; private set; }

        public ShadowMap(int size)
        {
            Size = size;

            Fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);

            DepthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthTexture);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.DepthComponent24,
                Size, Size,
                0,
                PixelFormat.DepthComponent,
                PixelType.Float,
                IntPtr.Zero
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Crucial to avoid shadow edges sampling outside
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            // Border color = "not in shadow" when outside the map
            float[] border = { 1f, 1f, 1f, 1f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, border);

            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                DepthTexture,
                0
            );

            // No color buffer
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new InvalidOperationException($"Shadow FBO incomplete: {status}");

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void BindForWriting()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
            GL.Viewport(0, 0, Size, Size);
        }

        public void BindForReading(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
        }

        public void Dispose()
        {
            if (DepthTexture != 0) GL.DeleteTexture(DepthTexture);
            if (Fbo != 0) GL.DeleteFramebuffer(Fbo);
            DepthTexture = 0;
            Fbo = 0;
        }

        static Matrix4 CreateDirectionalLightSpaceMatrix(Vector3 lightDir, Vector3 sceneCenter)
        {
            float distanceBack = 20f;
            Vector3 lightPos = sceneCenter - lightDir.Normalized() * distanceBack;

            float orthoSize = 20f;
            float near = 1f;
            float far = 60f;

            var lightView = Matrix4.LookAt(lightPos, sceneCenter, Vector3.UnitY);
            var lightProj = Matrix4.CreateOrthographic(orthoSize, orthoSize, near, far);

            return lightView * lightProj; 
        }
    }
}