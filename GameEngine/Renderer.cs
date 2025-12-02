using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine
{
    public class Renderer
    {
        Matrix4 view;
        Matrix4 projection;

        public Renderer(int width, int height)
        {
            view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), width / height, 0.1f, 100.0f);
        }

        public void Render(GameObjectManager gameObjectManager)
        {
            foreach (GameObject gameObject in gameObjectManager.gameObjects)
            {
                Matrix4 model = CreateModelMatrix(gameObject.transform);

                Shader shader = gameObject.shader;

                shader.SetMatrix4("model", model);
                shader.SetMatrix4("view", view);
                shader.SetMatrix4("projection", projection);
                shader.Use();

                Texture texture = gameObject.texture;
                texture = Texture.LoadFromFile(Util.GetProjectDir() + "/Resources/Textures/container.jpg");
                texture.Use(TextureUnit.Texture0);

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
