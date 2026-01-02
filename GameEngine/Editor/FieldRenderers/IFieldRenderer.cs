namespace GameEngine.Editor
{
    public interface IFieldRenderer
    {
        Type ValueType { get; }

        public Control CreateControl();

        public void SetValue(Control control, object value);

        event Action<object> valueChanged;
    }
}
