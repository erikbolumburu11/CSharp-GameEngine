using System.Runtime.InteropServices;
using GameEngine.Engine.Components;

namespace GameEngine.Engine
{
    public class LightManager
    {
        public List<Light> lights = new();
        public SSBO lightSSBO;

        public LightManager()
        {
            int maxLights = 256;
            lightSSBO = new SSBO(maxLights * Marshal.SizeOf<LightData>());
        }

        public void UploadLights()
        {
            LightData[] data = lights.Select(light => light.ToLightData()).ToArray();
            lightSSBO.Update(data);
            lightSSBO.Bind(0);
        }
    }
}
