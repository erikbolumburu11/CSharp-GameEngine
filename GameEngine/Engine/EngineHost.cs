using GameEngine.Engine.Components;
using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Engine
{
    public class EngineHost : IDisposable
    {

        Renderer renderer;
        public LightManager lightManager { get; private set; }
        public TextureManager textureManager { get; private set; }
        public ShaderManager shaderManager { get; private set; }
        public MaterialManager materialManager { get; private set; }
        public GameObjectManager gameObjectManager { get; private set; }
        public GltfInstantiator gltfInstantiator {get; private set; }
        public AssetManager assetManager { get; private set; }

        public Game game;

        bool glInitialized;

        public EngineHost()
        {
            RegisterComponents();

            game = new Game();

            textureManager = new TextureManager();
            shaderManager = new ShaderManager();
            gameObjectManager = new GameObjectManager();
            gltfInstantiator = new GltfInstantiator(gameObjectManager);
            materialManager = new MaterialManager();
            renderer = new Renderer();
            assetManager = new AssetManager(textureManager, materialManager);
        }

        public void InitializeGL()
        {
            Console.WriteLine("GL Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion));

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Multisample);

            textureManager.InitializeDefaultTextures();
            materialManager.InitializeDefaultMaterials();
            assetManager.RegisterBuiltInTextures();
            shaderManager.InitializeDefaultShaders();
            lightManager = new();

            glInitialized = true;
        }

        public void Render(Camera camera)
        {
            if(!glInitialized) return;

            GL.ClearColor(game.scene.skyboxColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderer.Render(
                materialManager, 
                textureManager,
                shaderManager,
                gameObjectManager,
                lightManager,
                game.scene,
                camera
            );
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

        public void RegisterComponents()
        {
            ComponentDtoRegistry.Register<Light, LightDto>(
                typeKey: ComponentTypeRegistry.Get(typeof(Light)),
                toDto: l => l.ToDto(),
                fromDto: (l, dto) => l.FromDto(dto)
            );

            ComponentDtoRegistry.Register<MeshRenderer, MeshRendererDto>(
                typeKey: ComponentTypeRegistry.Get(typeof(MeshRenderer)),
                toDto: mr => mr.ToDto(),
                fromDto: (mr, dto) => mr.FromDto(dto)
            );
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
