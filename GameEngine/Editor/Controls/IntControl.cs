using System.Globalization;

public class IntControl : FieldControlBase<int>
{
    private readonly TextBox box = new TextBox { Width = 80 };
    private int lastValidValue;

    public IntControl()
    {
        Controls.Add(box);

        box.TextChanged += OnTextChanged;
        box.Leave += OnLeave;
    }

    protected override void SetControlValue(int value)
    {
        lastValidValue = value;
        box.Text = value.ToString(CultureInfo.InvariantCulture);
    }

    protected override int GetControlValue()
    {
        return lastValidValue;
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        // Allow intermediate "-"
        if (box.Text == "-" || string.IsNullOrWhiteSpace(box.Text))
            return;

        if (int.TryParse(
            box.Text,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int value))
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