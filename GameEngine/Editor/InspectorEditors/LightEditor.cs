using GameEngine.Engine.Components;
using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace GameEngine.Editor
{
    public class LightEditor : Editor<Light>
    {
        public LightEditor(Light target) : base(target)
        {
            fields.Add(new FieldDescriptor
            {
                label = "Type (0=Point, 1=Directional)",
                valueType = typeof(int),
                getValue = () => (int)target.type,
                setValue = value => target.type = (LightType)(int)value
            });

            fields.Add(new FieldDescriptor
            {
                label = "Intensity",
                valueType = typeof(float),
                getValue = () => target.intensity,
                setValue = value => target.intensity = (float)value
            });

            fields.Add(new FieldDescriptor
            {
                label = "Radius",
                valueType = typeof(float),
                getValue = () => target.radius,
                setValue = value => target.radius = (float)value
            });

            fields.Add(new FieldDescriptor
            {
                label = "Specular",
                valueType = typeof(float),
                getValue = () => target.specularStrength,
                setValue = value => target.specularStrength = (float)value
            });

            fields.Add(new FieldDescriptor
            {
                label = "Color",
                valueType = typeof(Color),
                getValue = () => VectorToColor(target.color),
                setValue = value => target.color = ColorToVector((Color)value)
            });
        }

        private static Color VectorToColor(Vector3 color)
        {
            int r = (int)Math.Clamp(color.X, 0f, 255f);
            int g = (int)Math.Clamp(color.Y, 0f, 255f);
            int b = (int)Math.Clamp(color.Z, 0f, 255f);
            return Color.FromArgb(255, r, g, b);
        }

        private static Vector3 ColorToVector(Color color)
        {
            return new Vector3(color.R, color.G, color.B);
        }
    }
}
