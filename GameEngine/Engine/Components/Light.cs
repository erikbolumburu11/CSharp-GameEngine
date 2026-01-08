using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GameEngine.Engine.Components
{
    
    public enum LightType : int
    {
        Point = 0,
        Directional = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LightData
    {
        public Vector4 positionIntensity; 
        public Vector4 colorRadius;       
        public Vector4 directionType;     
        public Vector4 specularPadding;   
    }

    public record LightDto
    (
        float intensity,
        float[] color,
        float radius,
        float specularStrength,
        int type
    );

    public class Light : Component
    {
        public float intensity = 1f;
        public Vector3 color = new(255, 255, 255);
        public float radius = 3f;
        public float specularStrength = 0.5f;
        public LightType type = LightType.Point;

        public LightDto ToDto() => new
        (
            intensity: intensity,
            color: new[] {color.X, color.Y, color.Z},
            radius: radius,
            specularStrength: specularStrength,
            type: (int)type
        );

        public void FromDto(LightDto dto)
        {
            type = Enum.IsDefined(typeof(LightType), dto.type)
                ? (LightType)dto.type
                : LightType.Point; 

            intensity = dto.intensity;

            if (dto.color is { Length: >= 3 })
                color = new Vector3(dto.color[0], dto.color[1], dto.color[2]);

            radius = dto.radius;
            specularStrength = dto.specularStrength;
        }

        public LightData ToLightData()
        {
            Vector3 dir = Vector3.Transform(-Vector3.UnitZ, gameObject.transform.WorldRotation);
            if (dir.LengthSquared > 0) dir = dir.Normalized();

            float packedRadius = (type == LightType.Point) ? radius : 0f;

            return new LightData
            {
                positionIntensity = new Vector4(gameObject.transform.WorldPosition, intensity / 100f),
                colorRadius       = new Vector4(color / 255f, packedRadius),
                directionType     = new Vector4(dir, (float)type),
                specularPadding   = new Vector4(specularStrength, 0f, 0f, 0f)
            };
        }
    }
}
