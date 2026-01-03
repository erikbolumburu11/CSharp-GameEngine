using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Engine.Components
{
    public record MeshRendererDto
    (
    );

    public class MeshRenderer : Component
    {
        public VertexArray vao { get; private set; }
        public VertexBuffer<float> vbo { get; private set; }

        public Material material;
        public Shader shader;
        public string texturePath;

        public override void Start()
        {
            base.Start();
            CreateBuffers();
            CreateShader();
        }

        void CreateBuffers()
        {
            vao = new VertexArray();
            vbo = new VertexBuffer<float>(Util.cubeVertices);
        }

        // TODO: Shaders should be in a dictionary or something to avoid creating the same shader
        //       multiple times
        void CreateShader()
        {
            vao.Bind();
            vbo.Bind();

            shader = new Shader(Util.GetDefaultVertPath(), Util.GetDefaultFragPath());

            int stride = 8 * sizeof(float);

            // position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(
                0,
                3,
                VertexAttribPointerType.Float,
                false,
                stride,
                0
            );

            // texcoord
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(
                1,
                2,
                VertexAttribPointerType.Float,
                false,
                stride,
                3 * sizeof(float)
            );

            // normal
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(
                2,
                3,
                VertexAttribPointerType.Float,
                false,
                stride,
                5 * sizeof(float)
            );

            vbo.Unbind();
            vao.Unbind();
        }

        public MeshRendererDto ToDto() => new
        (
        );

        public void FromDto(MeshRendererDto dto)
        {
        }

        public void SetTexture(string path) => texturePath = path;
    }
}
