namespace GameEngine.Editor
{
    public class IntRenderer : IFieldRenderer
    {
        public Type ValueType => typeof(int);
        public event Action<object>? valueChanged;

        public Control CreateControl()
        {
            var control = new IntControl();
            control.ValueChanged += v => valueChanged?.Invoke(v);
            return control;
        }

        public void SetValue(Control control, object value)
        {
            if (control is IntControl ic && value is int i)
                ic.Value = i;
        }
    }
}
