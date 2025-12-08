using GameEngine.Editor;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace GameEngine.Engine
{
    public class Game
    {
        GameObjectManager gameObjectManager;
        public GameObjectManager GameObjectManager => gameObjectManager;

        Renderer renderer;

        Stopwatch time;

        public Game()
        {
            time = new Stopwatch();
            time.Start();

            gameObjectManager = new GameObjectManager();
            renderer = new Renderer();
        }

        public void Initialize()
        {
            GL.Enable(EnableCap.DepthTest);
            GameObject cube = gameObjectManager.CreateCube();
        }

        public void Update(float deltaTime)
        {
        }

        public void Render(Camera camera)
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderer.Render(gameObjectManager, camera);
        }
    }
}
