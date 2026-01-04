using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GameEngine.Engine.Components
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LightData
    {
        public Vector4 positionIntensity;
        public Vector4 colorRadius;
        public Vector4 specularPadding;
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
                positionIntensity = new Vector4(gameObject.transform.position, intensity / 100),
                colorRadius = new Vector4(color, radius),
                specularPadding = new Vector4(specularStrength, 0f, 0f, 0f)
            };
        }
    }
}
