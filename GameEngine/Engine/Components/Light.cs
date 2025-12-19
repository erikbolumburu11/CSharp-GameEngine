using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
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
        public float specularStrength;
    }

    public class Light : Component, IComponentSerializable
    {
        [ExposeInInspector] public float intensity = 1f;
        public Vector3 color = new(255, 255, 255);
        [ExposeInInspector] public float radius = 3f;
        [ExposeInInspector] public float specularStrength = 0.5f;

        public Dictionary<string, object> Save()
        {
            return new Dictionary<string, object>()
            {
                ["intensity"] = intensity,
                ["colorR"] = color.X,
                ["colorG"] = color.Y,
                ["colorB"] = color.Z,
                ["radius"] = radius,
                ["specularStrength"] = specularStrength
            };
        }

        public void Load(Dictionary<string, object> data)
        {
            intensity = (float)Util.GetObjectValue(data["intensity"]);
            color = new Vector3
            (
                (float)Util.GetObjectValue(data["colorR"]),
                (float)Util.GetObjectValue(data["colorG"]),
                (float)Util.GetObjectValue(data["colorB"])
            );
            radius = (float)Util.GetObjectValue(data["radius"]);
            specularStrength = (float)Util.GetObjectValue(data["specularStrength"]);
        }

        public LightData ToLightData()
        {
            return new LightData
            {
                position = gameObject.transform.position,
                intensity = intensity / 100,
                color = color,
                radius = radius,
                specularStrength = specularStrength
            };
        }
    }
}
