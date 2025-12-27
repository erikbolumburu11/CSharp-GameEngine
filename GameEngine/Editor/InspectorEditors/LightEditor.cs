using GameEngine.Engine.Components;

public class LightEditor : Editor<Light>
{
    public LightEditor(Light target) : base(target)
    {
        fields.Add(new FieldDescriptor
            {
                label = "Intensity",
                valueType = typeof(float),
                getValue = () => target.intensity,
                setValue = value => target.intensity = (float)value
            }
        );

        fields.Add(new FieldDescriptor
            {
                label = "Radius",
                valueType = typeof(float),
                getValue = () => target.radius,
                setValue = value => target.radius = (float)value
            }
        );

        fields.Add(new FieldDescriptor
            {
                label = "Specular",
                valueType = typeof(float),
                getValue = () => target.specularStrength,
                setValue = value => target.specularStrength = (float)value
            }
        );
    }
}