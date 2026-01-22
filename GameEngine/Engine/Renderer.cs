using GameEngine.Engine.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class Renderer
    {
        Matrix4 view;
        Matrix4 projection;

        readonly ShadowPass shadowPass = new ShadowPass();
        readonly SkyboxPass skyboxPass = new SkyboxPass();
        readonly BrdfLutCache brdfLutCache = new BrdfLutCache();

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

            bool hasShadowPass = shadowPass.Execute(
                shaderManager,
                gameObjectManager,
                lightManager,
                out Matrix4 lightSpaceMatrix
            );
            GL.Viewport(0, 0, camera.Width, camera.Height);


            if (hasShadowPass)
                shadowPass.BindForReading(TextureUnit.Texture3);

            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix(camera.Width, camera.Height);
            skyboxPass.Render(textureManager, shaderManager, scene, view, projection);
            foreach (GameObject gameObject in gameObjectManager.gameObjects)
            {
                MeshRenderer? meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null || meshRenderer.vao == null || meshRenderer.vertexCount <= 0) continue;

                Matrix4 model = RenderMath.CreateModelMatrix(gameObject.transform);
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

                material.GetHeight(textureManager).Use(TextureUnit.Texture9);
                shader.SetInt("heightTexture", 9);
                shader.SetFloat("heightScale", material.heightScale);

                bool useEnvironmentMap = false;
                Texture environmentTexture = textureManager.Black;
                if (scene.skyboxHdrGuid.HasValue && scene.skyboxHdrGuid.Value != Guid.Empty)
                {
                    Guid hdrGuid = scene.skyboxHdrGuid.Value;
                    if (AssetDatabase.TryGetPath(hdrGuid, out var absHdrPath)
                        && System.IO.File.Exists(absHdrPath))
                    {
                        environmentTexture = textureManager.GetHdr(absHdrPath);
                        useEnvironmentMap = true;
                    }
                }

                environmentTexture.Use(TextureUnit.Texture7);
                shader.SetInt("environmentMap", 7);
                shader.SetInt("useEnvironmentMap", useEnvironmentMap ? 1 : 0);
                if (useEnvironmentMap)
                {
                    Texture lut = brdfLutCache.GetOrCreate(shaderManager);
                    lut.Use(TextureUnit.Texture8);
                    shader.SetInt("brdfLut", 8);
                }

                meshRenderer.vao.Bind();
                GL.DrawArrays(PrimitiveType.Triangles, 0, meshRenderer.vertexCount);
                meshRenderer.vao.Unbind();
            }
        }

    }
}
