using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Engine
{
    public class EngineHost : IDisposable
    {
        public TextureManager textureManager;

        Renderer renderer;
        public LightManager lightManager { get; private set; }

        public Game game;

        bool glInitialized;

        public EngineHost()
        {
            game = new Game();

            textureManager = new TextureManager();
            renderer = new Renderer();
        }

        public void InitializeGL()
        {
            Console.WriteLine("GL Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion));

            GL.Enable(EnableCap.DepthTest);

            textureManager.InitializeTextures();
            lightManager = new();

            glInitialized = true;
        }

        public void Render(Camera camera)
        {
            if(!glInitialized) return;

            GL.ClearColor(game.scene.skyboxColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderer.Render(textureManager, game.gameObjectManager, lightManager, game.scene, camera);
        }

        public void Update(float deltaTime)
        {
            foreach (GameObject gameObject in game.gameObjectManager.gameObjects)
            {
                foreach (var component in gameObject.Components)
                {
                    component.Update(deltaTime);
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}