using System.Globalization;

public class FloatControl : FieldControlBase<float>
{
    private readonly TextBox box = new TextBox { Width = 80 };
    private float lastValidValue;

    public FloatControl()
    {
        Controls.Add(box);

        box.TextChanged += OnTextChanged;
        box.Leave += OnLeave;
    }

    protected override void SetControlValue(float value)
    {
        lastValidValue = value;
        box.Text = value.ToString(CultureInfo.InvariantCulture);
    }

    protected override float GetControlValue()
    {
        return lastValidValue;
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(box.Text) ||
            box.Text == "-" ||
            box.Text == "." ||
            box.Text == "-.")
            return;

        if (float.TryParse(
            box.Text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float value))
        {
            lastValidValue = value;
            NotifyValueChanged();
        }
    }

    private void OnLeave(object? sender, EventArgs e)
    {
        box.Text = lastValidValue.ToString(CultureInfo.InvariantCulture);
    }
}