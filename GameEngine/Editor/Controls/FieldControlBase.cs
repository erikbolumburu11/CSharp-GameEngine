public abstract class FieldControlBase<T> : UserControl
{
    private bool suppressEvent = false;

    public event Action<T>? ValueChanged;

    protected FieldControlBase()
    {
        AutoSize = false;
        Height = 24;
        MinimumSize = new Size(0, 24);
        Margin = Padding.Empty;
        Padding = Padding.Empty;
    }

    protected abstract void SetControlValue(T value);
    protected abstract T GetControlValue();

    public T Value
    {
        get => GetControlValue();
        set
        {
            suppressEvent = true;
            SetControlValue(value);
            suppressEvent = false;
        }
    }

    // Call this from child events (TextChanged, CheckedChanged, etc.)
    protected void NotifyValueChanged()
    {
        if (suppressEvent) return;
        ValueChanged?.Invoke(Value);
    }
}
