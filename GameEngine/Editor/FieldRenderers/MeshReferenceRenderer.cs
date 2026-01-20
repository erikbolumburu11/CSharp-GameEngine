namespace GameEngine.Editor
{
    public class MeshReferenceRenderer : IFieldRenderer
    {
        public Type ValueType => typeof(MeshReference);
        public event Action<object>? valueChanged;

        public Control CreateControl()
        {
            var control = new MeshControl();
            control.ValueChanged += v => valueChanged?.Invoke(v);
            return control;
        }

        public void SetValue(Control control, object value)
        {
            if (control is MeshControl meshControl && value is MeshReference meshReference)
                meshControl.Value = meshReference;
        }
    }
}
