using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace GameEngine
{
    public class Game
    {
        GLControl glControl;

        private int width;
        private int height;

        InputHandler input;
        public InputHandler InputHandler => input;

        GameObjectManager gameObjectManager;
        Renderer renderer;
        EditorCamera camera;

        Stopwatch time;

        public Game(GLControl glControl)
        {
            this.glControl = glControl;

            width = glControl.Width;
            height = glControl.Height;

            time = new Stopwatch();
            time.Start();

            gameObjectManager = new GameObjectManager();
            input = new InputHandler();
            camera = new EditorCamera(new Vector3(0, 0, 3), 1f, 0.2f, width, height, input);
            renderer = new Renderer(camera, width, height);
        }

        public void Initialize()
        {
            GameObject cube = gameObjectManager.CreateCube();
            cube.transform.rotation = Quaternion.FromEulerAngles(0, 0, 0);
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
            camera.Update(input, deltaTime);
        }

        public void Render()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderer.Render(gameObjectManager, camera);
        }
    }
}
