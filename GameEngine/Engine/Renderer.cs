using GameEngine.Editor;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class Renderer
    {
        Matrix4 view;
        Matrix4 projection;

        public void Render(GameObjectManager gameObjectManager, Camera camera)
        {
            view = camera.GetViewMatrix();
            projection = camera.GetProjectionMatrix(camera.Width, camera.Height);
            foreach (GameObject gameObject in gameObjectManager.gameObjects)
            {
                Matrix4 model = CreateModelMatrix(gameObject.transform);

                Shader shader = gameObject.shader;

                shader.SetMatrix4("model", model);
                shader.SetMatrix4("view", view);
                shader.SetMatrix4("projection", projection);
                shader.Use();

                gameObject.texture.Use(TextureUnit.Texture0);

                GL.BindVertexArray(gameObject.VertexArrayObject);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
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
