using GameEngine.Engine.Components;
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
                Vector3 lightDir = Vector3.Transform(-Vector3.UnitZ, dirLight.gameObject.transform.rotation).Normalized();
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
                    if (mr == null) continue;

                    Matrix4 model = CreateModelMatrix(go.transform);
                    shadowDepthShader.SetMatrix4("model", model);

                    mr.vao.Bind();
                    GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
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
                shadowMap!.BindForReading(TextureUnit.Texture2);

            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix(camera.Width, camera.Height);
            foreach (GameObject gameObject in gameObjectManager.gameObjects)
            {
                MeshRenderer? meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null) continue;

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
                    shader.SetInt("shadowMap", 2);
                }

                shader.SetInt("lightCount", lightManager.lights.Count);
                shader.SetFloat("ambientIntensity", scene.ambientLightIntensity);
                shader.SetVector3("viewPos", camera.position);


                Material material = materialManager.Get(meshRenderer.material);

                shader.SetVector2("uvTiling", material.uvTiling);
                shader.SetVector2("uvOffset", material.uvOffset);

                material.GetDiffuse(textureManager).Use(TextureUnit.Texture0);
                shader.SetInt("diffuseTexture", 0);

                material.GetSpecular(textureManager).Use(TextureUnit.Texture1);
                shader.SetInt("specularTexture", 1);

                meshRenderer.vao.Bind();
                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
                meshRenderer.vao.Unbind();
            }
        }

        Matrix4 CreateModelMatrix(Transform transform)
        {
            Matrix4 translation = Matrix4.CreateTranslation(transform.position);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(transform.rotation);
            Matrix4 scale = Matrix4.CreateScale(transform.scale);

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
