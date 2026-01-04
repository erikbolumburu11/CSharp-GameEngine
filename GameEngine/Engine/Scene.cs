namespace GameEngine.Engine
{
    public class Scene
    {
        public string relPath;

        public float ambientLightIntensity = 0.1f;
        public Color skyboxColor;

        public Scene(){}

        public Scene(string relPath)
        {
            this.relPath = relPath;
        }

    }
}
