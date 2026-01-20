using OpenTK.Graphics.OpenGL4;
using SharpGLTF.Schema2;
using System.Numerics;

namespace GameEngine.Engine.Components
{
    public record MeshRendererDto
    (
        Guid material,
        Guid mesh
    );

    public class MeshRenderer : Component, IDisposable
    {
        private const int VertexStrideFloats = 8;

        public VertexArray? vao { get; private set; }
        public VertexBuffer<float>? vbo { get; private set; }

        public Guid material;
        public Guid mesh
        {
            get => meshGuid;
            set
            {
                if (meshGuid == value)
                    return;

                meshGuid = value;
                if (started)
                    RebuildMesh();
            }
        }
        public int vertexCount { get; private set; }
        public Shader? shader;
        public string? vertexShaderPath;
        public string? fragmentShaderPath;

        private Guid meshGuid;
        private bool started;
        private bool disposed;

        public override void Start()
        {
            base.Start();
            started = true;
            RebuildMesh();

            if (string.IsNullOrWhiteSpace(vertexShaderPath))
                vertexShaderPath = Util.GetDefaultVertPath();
            if (string.IsNullOrWhiteSpace(fragmentShaderPath))
                fragmentShaderPath = Util.GetDefaultFragPath();
        }

        void CreateBuffers(float[] vertices)
        {
            vao = new VertexArray();
            vbo = new VertexBuffer<float>(vertices);
            vertexCount = vertices.Length / VertexStrideFloats;
        }

        void RebuildMesh()
        {
            DisposeBuffers();

            float[] vertices = BuildSelectedMeshVertices();
            CreateBuffers(vertices);
            SetupMesh();
        }

        void SetupMesh()
        {
            vao.Bind();
            vbo.Bind();

            int stride = VertexStrideFloats * sizeof(float);

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

        float[] BuildSelectedMeshVertices()
        {
            if (TryBuildPrimitiveVertices(meshGuid, out var vertices))
                return vertices;

            return Util.cubeVertices;
        }

        static bool TryBuildPrimitiveVertices(Guid meshGuid, out float[] vertices)
        {
            vertices = Array.Empty<float>();

            if (!AssetDatabase.TryGetMeshPrimitiveRef(meshGuid, out var primitiveRef))
                return false;

            if (!AssetDatabase.TryLoad(primitiveRef.ModelGuid, out ModelRoot model))
                return false;

            if (primitiveRef.MeshIndex < 0 || primitiveRef.MeshIndex >= model.LogicalMeshes.Count)
                return false;

            var mesh = model.LogicalMeshes[primitiveRef.MeshIndex];
            if (primitiveRef.PrimitiveIndex < 0 || primitiveRef.PrimitiveIndex >= mesh.Primitives.Count)
                return false;

            var primitive = mesh.Primitives[primitiveRef.PrimitiveIndex];
            if (primitive.DrawPrimitiveType != SharpGLTF.Schema2.PrimitiveType.TRIANGLES
                && primitive.DrawPrimitiveType != SharpGLTF.Schema2.PrimitiveType.TRIANGLE_STRIP
                && primitive.DrawPrimitiveType != SharpGLTF.Schema2.PrimitiveType.TRIANGLE_FAN)
                return false;

            if (!primitive.VertexAccessors.TryGetValue("POSITION", out var positionAccessor))
                return false;

            var positions = positionAccessor.AsVector3Array();
            if (positions.Count == 0)
                return false;

            SharpGLTF.Memory.IAccessorArray<Vector3>? normals = null;
            if (primitive.VertexAccessors.TryGetValue("NORMAL", out var normalAccessor))
                normals = normalAccessor.AsVector3Array();

            SharpGLTF.Memory.IAccessorArray<Vector2>? uvs = null;
            if (primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out var texAccessor))
                uvs = texAccessor.AsVector2Array();

            var indices = new List<int>();
            foreach (var (a, b, c) in primitive.GetTriangleIndices())
            {
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
            }

            if (indices.Count == 0)
                return false;

            vertices = new float[indices.Count * VertexStrideFloats];
            int offset = 0;
            foreach (int index in indices)
            {
                Vector3 position = index < positions.Count ? positions[index] : Vector3.Zero;
                Vector2 uv = (uvs != null && index < uvs.Count) ? uvs[index] : Vector2.Zero;
                Vector3 normal = (normals != null && index < normals.Count) ? normals[index] : Vector3.UnitZ;

                vertices[offset++] = position.X;
                vertices[offset++] = position.Y;
                vertices[offset++] = position.Z;

                vertices[offset++] = uv.X;
                vertices[offset++] = uv.Y;

                vertices[offset++] = normal.X;
                vertices[offset++] = normal.Y;
                vertices[offset++] = normal.Z;
            }

            return true;
        }

        public MeshRendererDto ToDto() => new
        (
            material: material,
            mesh: mesh
        );

        public void FromDto(MeshRendererDto dto)
        {
            material = dto.material;
            mesh = dto.mesh;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            DisposeBuffers();
        }

        void DisposeBuffers()
        {
            vertexCount = 0;

            if (vbo != null)
            {
                vbo.Dispose();
                vbo = null;
            }

            if (vao != null)
            {
                vao.Dispose();
                vao = null;
            }
        }
    }
}
