using OpenTK.Graphics.OpenGL4;

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

        float[] vertices =
        {
            //Position          Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        public Game(int width, int height)
        {
            this.width = width;
            this.height = height;


        }

        public void Initialize()
        {
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float),
                vertices,
                BufferUsageHint.StaticDraw
            );

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            shader = new Shader(Util.GetProjectDir() + "/Shaders/shader.vert", Util.GetProjectDir() + "/Shaders/shader.frag");
            shader.Use();

            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            texture = Texture.LoadFromFile(Util.GetProjectDir() + "/Resources/Textures/container.jpg");
            texture.Use(TextureUnit.Texture0);
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
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(VertexArrayObject);

            texture.Use(TextureUnit.Texture0);
            shader.Use();

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
