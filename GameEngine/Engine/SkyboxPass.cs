using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public sealed class SkyboxPass
    {
        VertexArray? skyboxVao;
        VertexBuffer<float>? skyboxVbo;
        Shader? skyboxShader;

        public void Render(
            TextureManager textureManager,
            ShaderManager shaderManager,
            Scene scene,
            Matrix4 view,
            Matrix4 projection
        )
        {
            if (string.IsNullOrWhiteSpace(scene.skyboxHdrPath) || ProjectContext.Current == null)
                return;

            string relPath = scene.skyboxHdrPath;
            string absPath = System.IO.Path.IsPathRooted(relPath)
                ? relPath
                : ProjectContext.Current.Paths.ToAbsolute(relPath);

            if (!System.IO.File.Exists(absPath))
                return;

            EnsureResources(shaderManager);

            Texture hdr = textureManager.GetHdr(relPath);
            Matrix4 viewNoTranslation = new Matrix4(new Matrix3(view));

            GL.DepthFunc(DepthFunction.Lequal);
            GL.DepthMask(false);

            skyboxShader!.Use();
            skyboxShader.SetMatrix4("view", viewNoTranslation);
            skyboxShader.SetMatrix4("projection", projection);
            skyboxShader.SetFloat("exposure", scene.skyboxExposure);
            skyboxShader.SetInt("flipV", scene.skyboxFlipV ? 1 : 0);

            hdr.Use(TextureUnit.Texture7);
            skyboxShader.SetInt("skyboxTexture", 7);

            skyboxVao!.Bind();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            skyboxVao.Unbind();

            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Less);
        }

        void EnsureResources(ShaderManager shaderManager)
        {
            if (skyboxVao == null)
            {
                float[] vertices = BuildSkyboxVertices();
                skyboxVao = new VertexArray();
                skyboxVbo = new VertexBuffer<float>(vertices);

                skyboxVao.Bind();
                skyboxVbo.Bind();
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(
                    0,
                    3,
                    VertexAttribPointerType.Float,
                    false,
                    3 * sizeof(float),
                    0
                );
                skyboxVbo.Unbind();
                skyboxVao.Unbind();
            }

            if (skyboxShader == null)
            {
                string skyVert = Util.GetProjectDir() + "/Shaders/Skybox/skybox.vert";
                string skyFrag = Util.GetProjectDir() + "/Shaders/Skybox/skybox.frag";
                skyboxShader = shaderManager.Get(skyVert, skyFrag);
            }
        }

        static float[] BuildSkyboxVertices()
        {
            const int stride = 8;
            int vertexCount = Util.cubeVertices.Length / stride;
            float[] vertices = new float[vertexCount * 3];
            int src = 0;
            int dst = 0;

            for (int i = 0; i < vertexCount; i++)
            {
                vertices[dst++] = Util.cubeVertices[src++];
                vertices[dst++] = Util.cubeVertices[src++];
                vertices[dst++] = Util.cubeVertices[src++];
                src += 5;
            }

            return vertices;
        }
    }
}
