using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Engine.Components
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LightData
    {
        public Vector3 position;
        public float intensity;
        public Vector3 color;
        public float radius;
    }

    public class Light : Component
    {
        [ExposeInInspector] public float intensity = 1f;
        public Vector3 color = new(255, 255, 255);
        [ExposeInInspector] public float radius = 3f;

        public LightData ToLightData()
        {
            return new LightData
            {
                position = gameObject.transform.position,
                intensity = intensity / 100,
                color = color,
                radius = radius
            };
        }
    }
}
