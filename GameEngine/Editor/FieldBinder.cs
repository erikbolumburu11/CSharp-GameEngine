using OpenTK.Mathematics;

namespace GameEngine.Editor
{
    public static class FieldBinder
    {
        public static Dictionary<Type, Func<IFieldRenderer>> fieldRendererRegistry = new()
        {
            { typeof(Vector3), () => new Vector3Renderer() },
            { typeof(float), () => new FloatRenderer() },
            { typeof(int), () => new IntRenderer() },
            { typeof(string), () => new StringRenderer() },
            { typeof(MaterialReference), () => new MaterialRenderer() },
        };

        public static Control Bind(FieldDescriptor fieldDescriptor, Control parent)
        {
            var control = CreateBoundControl(fieldDescriptor);
            parent.Controls.Add(control);
            return control;
        }

        public static Control CreateBoundControl(FieldDescriptor fieldDescriptor)
        {
            var renderer = fieldRendererRegistry[fieldDescriptor.valueType]();
            var control = renderer.CreateControl();

            renderer.SetValue(control, fieldDescriptor.getValue());
            renderer.valueChanged += newValue => fieldDescriptor.setValue(newValue);

            return control;
        }
    }
}
