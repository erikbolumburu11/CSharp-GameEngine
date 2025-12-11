using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Engine.Components
{
    public class MeshRenderer : Component, IComponentSerializable
    {

        int VertexBufferObject;
        public int VertexArrayObject;

        public Shader shader;
        public Texture texture;

        [ExposeInInspector] float inspectorTestFloat;

        public override void Start()
        {
            base.Start();
            CreateBuffers();
            CreateShader();
            LoadTexture();
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

        // TODO: Shaders should be in a dictionary or something to avoid creating the same shader
        //       multiple times
        void CreateShader()
        {
            shader = new Shader(Util.GetDefaultVertPath(), Util.GetDefaultFragPath());

            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        }

        // TODO: Textures should be in a dictionary or something to avoid creating the same texture
        //       multiple times
        void LoadTexture()
        {
            texture = Texture.LoadFromFile(Util.GetProjectDir() + "/Resources/Textures/container.jpg");
        }

        public Dictionary<string, object> Save()
        {
            return new Dictionary<string, object>();
        }

        public void Load(Dictionary<string, object> data)
        {
        }
    }
}
