using GameEngine.Editor;
using GameEngine.Engine.Components;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace GameEngine.Engine
{
    public class Game
    {
        public GameObjectManager gameObjectManager { get; private set; }

        LightManager lightManager;
        Renderer renderer;

        Stopwatch time;

        public Scene scene;

        public Game()
        {
            time = new Stopwatch();
            time.Start();

            gameObjectManager = new GameObjectManager();

            scene = new Scene
            {
                ambientLightIntensity = 1f
            };
        }

        public void Initialize()
        {
            lightManager = new();
            renderer = new Renderer();

            GL.Enable(EnableCap.DepthTest);
            GameObject cube = gameObjectManager.CreateCube();

            Console.WriteLine("GL Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion));
        }

        public void Update(float deltaTime)
        {
            foreach (GameObject gameObject in gameObjectManager.gameObjects)
                {
                    foreach (var component in gameObject.Components)
                    {
                        component.Update(deltaTime);
                    }
                }
        }

        public void Render(Camera camera)
        {
            GL.ClearColor(scene.skyboxColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderer.Render(gameObjectManager, lightManager, scene, camera);
        }
    }
}
