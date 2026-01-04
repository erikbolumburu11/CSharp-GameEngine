using System.Drawing;

namespace GameEngine.Editor
{
    public class ColorRenderer : IFieldRenderer
    {
        public Type ValueType => typeof(Color);

        public event Action<object>? valueChanged;

        public Control CreateControl()
        {
            var control = new ColorControl();
            control.ValueChanged += v => valueChanged?.Invoke(v);
            return control;
        }

        public void SetValue(Control control, object value)
        {
            if (control is ColorControl cc && value is Color color)
                cc.Value = color;
        }
    }
}
