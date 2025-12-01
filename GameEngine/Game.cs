using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace GameEngine
{
    public class Game
    {
        private int width;
        private int height;

        int VertexBufferObject;
        int VertexArrayObject;
        int ElementBufferObject;

        Shader shader;
        Texture texture;

        Matrix4 model;
        Matrix4 view;
        Matrix4 projection;

        Stopwatch time;

        public Game(int width, int height)
        {
            this.width = width;
            this.height = height;

            time = new Stopwatch();
            time.Start();
        }

        public void Initialize()
        {
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                Util.cubeVertices.Length * sizeof(float),
                Util.cubeVertices,
                BufferUsageHint.StaticDraw
            );

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            shader = new Shader(Util.GetProjectDir() + "/Shaders/shader.vert", Util.GetProjectDir() + "/Shaders/shader.frag");

            model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-55.0f));
            view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), width / height, 0.1f, 100.0f);

            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            shader.Use();

            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            texture = Texture.LoadFromFile(Util.GetProjectDir() + "/Resources/Textures/container.jpg");
            texture.Use(TextureUnit.Texture0);

            GL.Enable(EnableCap.DepthTest);
        }

        public void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            GL.Viewport(0, 0, width, height);
        }

        public void Update(float deltaTime)
        {

        }

        public void Render()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            GL.BindVertexArray(VertexArrayObject);

            model = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(time.Elapsed.TotalSeconds * 20.0f));
            model = model * Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(time.Elapsed.TotalSeconds * 20.0f));

            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }
    }
}
