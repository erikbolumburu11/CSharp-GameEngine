using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Engine.Components
{
    public record MeshRendererDto
    (
        Guid material
    );

    public class MeshRenderer : Component
    {
        public VertexArray vao { get; private set; }
        public VertexBuffer<float> vbo { get; private set; }

        public Guid material;
        public Shader? shader;
        public string? vertexShaderPath;
        public string? fragmentShaderPath;

        public override void Start()
        {
            base.Start();
            CreateBuffers();
            SetupMesh();

            if (string.IsNullOrWhiteSpace(vertexShaderPath))
                vertexShaderPath = Util.GetDefaultVertPath();
            if (string.IsNullOrWhiteSpace(fragmentShaderPath))
                fragmentShaderPath = Util.GetDefaultFragPath();
        }

        void CreateBuffers()
        {
            vao = new VertexArray();
            vbo = new VertexBuffer<float>(Util.cubeVertices);
        }

        void SetupMesh()
        {
            vao.Bind();
            vbo.Bind();

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
            material: material
        );

        public void FromDto(MeshRendererDto dto)
        {
            material = dto.material;
        }
    }
}
