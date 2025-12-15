using GameEngine.Editor;
using GameEngine.Engine.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class Renderer
    {
        Matrix4 view;
        Matrix4 projection;

        public void Render(
            GameObjectManager gameObjectManager, 
            LightManager lightManager,
            Scene scene,
            Camera camera
            )
        {
            lightManager.lights.Clear();
            lightManager.lights = gameObjectManager.GetAllComponents<Light>();

            lightManager.UploadLights();

            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix(camera.Width, camera.Height);
            foreach (GameObject gameObject in gameObjectManager.gameObjects)
            {
                MeshRenderer? meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null) return;

                Matrix4 model = CreateModelMatrix(gameObject.transform);

                Shader shader = meshRenderer.shader;
                int lightCountLocation = GL.GetUniformLocation(shader.Handle, "lightCount");
                GL.Uniform1(lightCountLocation, lightManager.lights.Count);

                int ambientLightIntensityLocation = GL.GetUniformLocation(shader.Handle, "ambientIntensity");
                GL.Uniform1(ambientLightIntensityLocation, scene.ambientLightIntensity);

                shader.SetMatrix4("model", model);
                shader.SetMatrix4("view", view);
                shader.SetMatrix4("projection", projection);
                shader.Use();

                meshRenderer.texture.Use(TextureUnit.Texture0);

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

            return translation * rotation * scale;
        }
    }
}
