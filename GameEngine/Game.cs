using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace GameEngine
{
    public class Game
    {
        private int width;
        private int height;

        GameObjectManager gameObjectManager;
        Renderer renderer;

        Stopwatch time;

        public Game(int width, int height)
        {
            this.width = width;
            this.height = height;

            time = new Stopwatch();
            time.Start();

            gameObjectManager = new();
            renderer = new(width, height);
        }

        public void Initialize()
        {
            GameObject cube = gameObjectManager.CreateCube();
            cube.transform.rotation = Quaternion.FromEulerAngles(Util.DegToRad(55f), 0, 0);
            GL.Enable(EnableCap.DepthTest);
        }

        public void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            GL.Viewport(0, 0, width, height);
        }

        public void Update(float deltaTime){}

        public void Render()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderer.Render(gameObjectManager);
        }
    }
}
