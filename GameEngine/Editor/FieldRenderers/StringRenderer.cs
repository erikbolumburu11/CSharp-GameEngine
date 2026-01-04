namespace GameEngine.Editor
{
    public class StringRenderer : IFieldRenderer
    {
        public Type ValueType => typeof(string);

        public event Action<object>? valueChanged;

        public Control CreateControl()
        {
            var control = new StringControl();
            control.ValueChanged += v => valueChanged?.Invoke(v);
            return control;
        }

        public void SetValue(Control control, object value)
        {
            if (control is StringControl sc)
                sc.Value = value as string ?? string.Empty;
        }
    }
}
