namespace GameEngine.Editor
{
    public class MaterialRenderer : IFieldRenderer
    {
        public Type ValueType => typeof(MaterialReference);
        public event Action<object>? valueChanged;

        public Control CreateControl()
        {
            var control = new MaterialControl();
            control.ValueChanged += v => valueChanged?.Invoke(v);
            return control;
        }

        public void SetValue(Control control, object value)
        {
            if (control is MaterialControl materialControl && value is MaterialReference materialReference)
                materialControl.Value = materialReference;
        }
    }
}
