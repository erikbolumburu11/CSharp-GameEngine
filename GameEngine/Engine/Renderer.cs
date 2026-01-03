using GameEngine.Engine.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class Renderer
    {
        Matrix4 view;
        Matrix4 projection;

        public void Render
        (
            TextureManager textureManager,
            GameObjectManager gameObjectManager,
            LightManager lightManager,
            Scene scene,
            Camera camera
        )
        {
            lightManager.lights = gameObjectManager.GetAllComponents<Light>();

            lightManager.UploadLights();

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

                Texture texture = textureManager.Get(meshRenderer.texturePath);
                texture.Use(TextureUnit.Texture0);

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
