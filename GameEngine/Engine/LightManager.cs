using GameEngine.Engine.Components;

namespace GameEngine.Engine
{
    public class LightManager
    {
        public List<Light> lights = new();
        public SSBO lightSSBO;

        public LightManager()
        {
            lightSSBO = new SSBO(1024 * 16);
        }

        public void UploadLights()
        {
            LightData[] data = lights.Select(light => light.ToLightData()).ToArray();
            lightSSBO.Update(data);
            lightSSBO.Bind(0);
        }
    }
}
