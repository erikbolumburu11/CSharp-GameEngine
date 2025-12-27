using System.Dynamic;
using GameEngine.Editor;
using OpenTK.Mathematics;

public class Vector3Renderer : IFieldRenderer
{
    public Type ValueType => typeof(Vector3);

    public event Action<object> valueChanged;

    public Control CreateControl()
    {
        Vector3Control control = new Vector3Control();
        control.ValueChanged += value => valueChanged?.Invoke(value);
        return control;
    }

    public void SetValue(Control control, object value)
    {
        if (control is Vector3Control vectorControl && value is Vector3 vec)
        {
            vectorControl.Value = vec;
        }
    }
}