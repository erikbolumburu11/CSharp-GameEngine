using GameEngine.Engine.Components;
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class Renderer
    {
        Matrix4 view;
        Matrix4 projection;

        ShadowMap? shadowMap;
        Shader? shadowDepthShader;
        VertexArray? skyboxVao;
        VertexBuffer<float>? skyboxVbo;
        Shader? skyboxShader;
        Texture? brdfLut;
        VertexArray? brdfLutVao;
        VertexBuffer<float>? brdfLutVbo;
        Shader? brdfLutShader;
        const int BrdfLutSize = 256;

        public void Render
        (
            MaterialManager materialManager,
            TextureManager textureManager,
            ShaderManager shaderManager,
            GameObjectManager gameObjectManager,
            LightManager lightManager,
            Scene scene,
            Camera camera
        )
        {
            lightManager.lights = gameObjectManager.GetAllComponents<Light>();
            lightManager.UploadLights();

            Matrix4 lightSpaceMatrix = Matrix4.Identity;

            // Shadow Pass
            Light? dirLight = lightManager.lights.FirstOrDefault(l => l.type == LightType.Directional);
            bool hasShadowPass = dirLight != null;
            if (dirLight != null)
            {
                Vector3 lightDir = Vector3.Transform(-Vector3.UnitZ, dirLight.gameObject.transform.WorldRotation).Normalized();
                Vector3 sceneCenter = Vector3.Zero;
                lightSpaceMatrix = CreateDirectionalLightSpaceMatrix(lightDir, sceneCenter);

                shadowMap ??= new ShadowMap(2048);
                shadowMap.BindForWriting();
                GL.Clear(ClearBufferMask.DepthBufferBit);

                GL.Disable(EnableCap.CullFace);
                GL.CullFace(TriangleFace.Front);
                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(2.0f, 4.0f); // tweak these

                string depthVert = Util.GetProjectDir() + "/Shaders/Depth/shadow_depth.vert";
                string depthFrag = Util.GetProjectDir() + "/Shaders/Depth/shadow_depth.frag";
                shadowDepthShader ??= shaderManager.Get(depthVert, depthFrag);

                shadowDepthShader.Use();
                shadowDepthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);

                foreach (GameObject go in gameObjectManager.gameObjects)
                {
                    MeshRenderer? mr = go.GetComponent<MeshRenderer>();
                    if (mr == null || mr.vao == null || mr.vertexCount <= 0) continue;

                    Matrix4 model = CreateModelMatrix(go.transform);
                    shadowDepthShader.SetMatrix4("model", model);

                    mr.vao.Bind();
                    GL.DrawArrays(PrimitiveType.Triangles, 0, mr.vertexCount);
                    mr.vao.Unbind();
                }
            }

            // Restore GL
            GL.CullFace(TriangleFace.Back);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, camera.Width, camera.Height);


            if (hasShadowPass)
                shadowMap!.BindForReading(TextureUnit.Texture3);

            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix(camera.Width, camera.Height);
            RenderSkybox(shaderManager, textureManager, scene, view, projection);
            foreach (GameObject gameObject in gameObjectManager.gameObjects)
            {
                MeshRenderer? meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null || meshRenderer.vao == null || meshRenderer.vertexCount <= 0) continue;

                Matrix4 model = CreateModelMatrix(gameObject.transform);
                Shader shader = meshRenderer.shader ??= shaderManager.Get(
                    meshRenderer.vertexShaderPath,
                    meshRenderer.fragmentShaderPath
                );

                shader.Use();

                shader.SetMatrix4("model", model);
                shader.SetMatrix4("view", view);
                shader.SetMatrix4("projection", projection);

                if (hasShadowPass)
                {
                    shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
                    shader.SetInt("shadowMap", 3);
                }

                shader.SetInt("lightCount", lightManager.lights.Count);
                shader.SetFloat("ambientIntensity", scene.ambientLightIntensity);
                shader.SetFloat("iblSpecularIntensity", scene.iblSpecularIntensity);
                shader.SetVector3("viewPos", camera.position);

                Material material = materialManager.Get(meshRenderer.material);

                shader.SetVector2("uvTiling", material.uvTiling);
                shader.SetVector2("uvOffset", material.uvOffset);

                material.GetDiffuse(textureManager).Use(TextureUnit.Texture0);
                shader.SetInt("diffuseTexture", 0);

                shader.SetInt("useCombinedMR", material.useCombinedMR ? 1 : 0);

                material.GetMetallicRoughness(textureManager).Use(TextureUnit.Texture1);
                shader.SetInt("metallicRoughnessTexture", 1);

                material.GetAmbientOcclusion(textureManager).Use(TextureUnit.Texture2);
                shader.SetInt("aoTexture", 2);

                material.GetMetallic(textureManager).Use(TextureUnit.Texture4);
                shader.SetInt("metallicTexture", 4);

                material.GetRoughness(textureManager).Use(TextureUnit.Texture5);
                shader.SetInt("roughnessTexture", 5);

                material.GetNormal(textureManager).Use(TextureUnit.Texture6);
                shader.SetInt("normalTexture", 6);

                bool useEnvironmentMap = false;
                Texture environmentTexture = textureManager.Black;
                if (!string.IsNullOrWhiteSpace(scene.skyboxHdrPath) && ProjectContext.Current != null)
                {
                    string hdrPath = scene.skyboxHdrPath;
                    string absHdrPath = System.IO.Path.IsPathRooted(hdrPath)
                        ? hdrPath
                        : ProjectContext.Current.Paths.ToAbsolute(hdrPath);
                    if (System.IO.File.Exists(absHdrPath))
                    {
                        environmentTexture = textureManager.GetHdr(hdrPath);
                        useEnvironmentMap = true;
                    }
                }

                environmentTexture.Use(TextureUnit.Texture7);
                shader.SetInt("environmentMap", 7);
                shader.SetInt("useEnvironmentMap", useEnvironmentMap ? 1 : 0);
                if (useEnvironmentMap)
                {
                    EnsureBrdfLut(shaderManager);
                    brdfLut!.Use(TextureUnit.Texture8);
                    shader.SetInt("brdfLut", 8);
                }

                meshRenderer.vao.Bind();
                GL.DrawArrays(PrimitiveType.Triangles, 0, meshRenderer.vertexCount);
                meshRenderer.vao.Unbind();
            }
        }

        private void RenderSkybox(
            ShaderManager shaderManager,
            TextureManager textureManager,
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

            EnsureSkyboxResources(shaderManager);

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

        private void EnsureSkyboxResources(ShaderManager shaderManager)
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

        private void EnsureBrdfLut(ShaderManager shaderManager)
        {
            if (brdfLut != null)
                return;

            EnsureBrdfLutResources(shaderManager);

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
        }

        private void EnsureBrdfLutResources(ShaderManager shaderManager)
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

        private static float[] BuildSkyboxVertices()
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

        Matrix4 CreateModelMatrix(Transform transform)
        {
            Matrix4 translation = Matrix4.CreateTranslation(transform.WorldPosition);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(transform.WorldRotation);
            Matrix4 scale = Matrix4.CreateScale(transform.WorldScale);

            return scale * rotation * translation;
        }

        static Matrix4 CreateDirectionalLightSpaceMatrix(Vector3 lightDir, Vector3 sceneCenter)
        {
            // Choose a distance back from the scene so the light "sees" it
            float distanceBack = 20f;
            Vector3 lightPos = sceneCenter - lightDir.Normalized() * distanceBack;

            // Ortho bounds: tweak these to fit your scene
            float orthoSize = 7;
            float near = 1f;
            float far = 40f;

            var lightView = Matrix4.LookAt(lightPos, sceneCenter, Vector3.UnitY);
            var lightProj = Matrix4.CreateOrthographic(orthoSize, orthoSize, near, far);

            return lightView * lightProj; 
        }
    }
}
