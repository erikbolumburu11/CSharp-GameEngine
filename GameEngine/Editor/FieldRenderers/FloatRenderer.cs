namespace GameEngine.Editor
{
    public class FloatRenderer : IFieldRenderer
    {
        public Type ValueType => typeof(float);
        public event Action<object>? valueChanged;

        public Control CreateControl()
        {
            var control = new FloatControl();
            control.ValueChanged += v => valueChanged?.Invoke(v);
            return control;
        }

        public void SetValue(Control control, object value)
        {
            if (control is FloatControl fc && value is float f)
                fc.Value = f;
        }
    }
}
