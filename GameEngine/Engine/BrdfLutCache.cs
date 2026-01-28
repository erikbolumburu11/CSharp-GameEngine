using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Engine
{
    public sealed class BrdfLutCache
    {
        const int BrdfLutSize = 256;

        Texture? brdfLut;
        VertexArray? brdfLutVao;
        VertexBuffer<float>? brdfLutVbo;
        Shader? brdfLutShader;

        public Texture GetOrCreate(ShaderManager shaderManager)
        {
            if (brdfLut != null)
                return brdfLut;

            EnsureResources(shaderManager);

            int lutTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, lutTexture);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rg16f,
                BrdfLutSize,
                BrdfLutSize,
                0,
                PixelFormat.Rg,
                PixelType.Float,
                IntPtr.Zero
            );
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            int fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                lutTexture,
                0
            );
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new InvalidOperationException($"BRDF LUT FBO incomplete: {status}");

            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            bool depthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);

            GL.Viewport(0, 0, BrdfLutSize, BrdfLutSize);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            brdfLutShader!.Use();
            brdfLutVao!.Bind();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            brdfLutVao.Unbind();

            if (depthTestEnabled)
                GL.Enable(EnableCap.DepthTest);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteFramebuffer(fbo);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Viewport(viewport[0], viewport[1], viewport[2], viewport[3]);

            brdfLut = new Texture(lutTexture, BrdfLutSize, BrdfLutSize);
            return brdfLut;
        }

        public void Invalidate()
        {
            brdfLut?.Dispose();
            brdfLut = null;
        }

        void EnsureResources(ShaderManager shaderManager)
        {
            if (brdfLutVao == null)
            {
                float[] quadVertices =
                {
                    -1f, -1f, 0f, 0f,
                     1f, -1f, 1f, 0f,
                    -1f,  1f, 0f, 1f,
                    -1f,  1f, 0f, 1f,
                     1f, -1f, 1f, 0f,
                     1f,  1f, 1f, 1f
                };

                brdfLutVao = new VertexArray();
                brdfLutVbo = new VertexBuffer<float>(quadVertices);

                brdfLutVao.Bind();
                brdfLutVbo.Bind();
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(
                    0,
                    2,
                    VertexAttribPointerType.Float,
                    false,
                    4 * sizeof(float),
                    0
                );
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(
                    1,
                    2,
                    VertexAttribPointerType.Float,
                    false,
                    4 * sizeof(float),
                    2 * sizeof(float)
                );
                brdfLutVbo.Unbind();
                brdfLutVao.Unbind();
            }

            if (brdfLutShader == null)
            {
                string lutVert = Util.GetProjectDir() + "/Shaders/IBL/brdf_lut.vert";
                string lutFrag = Util.GetProjectDir() + "/Shaders/IBL/brdf_lut.frag";
                brdfLutShader = shaderManager.Get(lutVert, lutFrag);
            }
        }
    }
}
