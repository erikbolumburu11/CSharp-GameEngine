using GameEngine.Engine.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class Renderer
    {
        Matrix4 view;
        Matrix4 projection;

        public ShadowMap shadowMap;

        public void Render
        (
            ShaderManager shaderManager,
            MaterialManager materialManager,
            TextureManager textureManager,
            GameObjectManager gameObjectManager,
            LightManager lightManager,
            Scene scene,
            Camera camera
        )
        {
            lightManager.lights = gameObjectManager.GetAllComponents<Light>();

            lightManager.UploadLights();

            bool hasDirLight = false;
            Matrix4 lightSpaceMatrix = Matrix4.Identity;

            // Shadow Map Pass
            Light dirLight = lightManager.lights.FirstOrDefault(l => l.type == LightType.Directional);
            if(dirLight != null)
            {
                hasDirLight = true;

                Vector3 lightDir = Vector3.Transform(-Vector3.UnitZ, dirLight.gameObject.transform.rotation).Normalized();

                Vector3 sceneCenter = Vector3.Zero;

                Matrix4 lightView = Matrix4.LookAt(sceneCenter - lightDir * 20f, sceneCenter, Vector3.UnitY);
                Matrix4 lightProj = Matrix4.CreateOrthographic(20f, 20f, 1f, 60f);
                lightSpaceMatrix = lightProj * lightView;

                shadowMap.BindForWriting();
                GL.Clear(ClearBufferMask.DepthBufferBit);

                GL.Enable(EnableCap.CullFace);
                GL.CullFace(TriangleFace.Front);

                Shader shadowShader = shaderManager.Get("GameEngine/Shaders/Depth/shadow_depth.vert", "GameEngine/Shaders/Depth/shadow_depth.frag");
                shadowShader.Use();
                shadowShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);

                foreach (GameObject go in gameObjectManager.gameObjects)
                {
                    MeshRenderer mr = go.GetComponent<MeshRenderer>();
                    if (mr == null) continue;

                    Matrix4 model = CreateModelMatrix(go.transform);
                    shadowShader.SetMatrix4("model", model);

                    mr.vao.Bind();
                    GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
                    mr.vao.Unbind();
                }
            }

            // Restore GL State
            GL.CullFace(TriangleFace.Back);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, camera.Width, camera.Height);

            // Main Pass
            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix(camera.Width, camera.Height);
            foreach (GameObject gameObject in gameObjectManager.gameObjects)
            {
                MeshRenderer? meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null) continue;

                Matrix4 model = CreateModelMatrix(gameObject.transform);
                Shader shader = meshRenderer.shader;

                shader.Use();

                shader.SetMatrix4("model", model);
                shader.SetMatrix4("view", view);
                shader.SetMatrix4("projection", projection);

                shader.SetInt("lightCount", lightManager.lights.Count);
                shader.SetFloat("ambientIntensity", scene.ambientLightIntensity);
                shader.SetVector3("viewPos", camera.position);

<<<<<<< Updated upstream
=======
                shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);

                shadowMap.BindForReading(TextureUnit.Texture2);
                shader.SetInt("shadowMap", 2);

>>>>>>> Stashed changes
                Material material = materialManager.Get(meshRenderer.material);

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
    }
}
