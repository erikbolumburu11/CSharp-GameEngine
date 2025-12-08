using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace GameEngine
{

    public class Transform
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;

        public Transform()
        {
            position = Vector3.Zero;
            scale = new Vector3(1, 1, 1);
            rotation = Quaternion.Identity;
        }
    }

    public class GameObject
    {
        string name;
        public string Name => name;

        public Transform transform;

        int VertexBufferObject;
        public int VertexArrayObject;

        public Shader shader;
        public Texture texture;

        public event Action<GameObject>? Changed;

        public GameObject(string name)
        {
            this.name = name;

            transform = new();

            CreateBuffers();

            shader = new Shader(Util.GetDefaultVertPath(), Util.GetDefaultFragPath());

            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        }

        void CreateBuffers()
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
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            Changed?.Invoke(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
