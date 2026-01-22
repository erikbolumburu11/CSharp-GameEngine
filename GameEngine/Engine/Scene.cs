namespace GameEngine.Engine
{
    public class Scene
    {
        public string relPath;

        public float ambientLightIntensity = 0.1f;
        public Color skyboxColor;
        public string? skyboxHdrPath;
        public float skyboxExposure = 1.0f;
        public bool skyboxFlipV;
        public float iblSpecularIntensity = 0.5f;

        public Scene(){}

        public Scene(string relPath)
        {
            this.relPath = relPath;
        }

    }
}
