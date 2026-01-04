using OpenTK.Mathematics;
using System.Runtime.InteropServices;

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

    public record LightDto
    (
        float intensity,
        float[] color,
        float radius,
        float specularStrength
    );

    public class Light : Component
    {
        [ExposeInInspector] public float intensity = 1f;
        public Vector3 color = new(255, 255, 255);
        [ExposeInInspector] public float radius = 3f;
        [ExposeInInspector] public float specularStrength = 0.5f;

        public LightDto ToDto() => new
        (
            intensity: intensity,
            color: new[] {color.X, color.Y, color.Z},
            radius: radius,
            specularStrength: specularStrength
        );

        public void FromDto(LightDto dto)
        {
            intensity = dto.intensity;
            if (dto.color is { Length: >= 3 })
                color = new Vector3(dto.color[0], dto.color[1], dto.color[2]);
            radius = dto.radius;
            specularStrength = dto.specularStrength;
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
