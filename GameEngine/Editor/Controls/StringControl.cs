namespace GameEngine.Editor
{
    public class StringControl : FieldControlBase<string>
    {
        private readonly TextBox box = new TextBox { Width = 160 };
        private string lastValue = string.Empty;

        public StringControl()
        {
            Controls.Add(box);
            box.TextChanged += (s, e) =>
            {
                // Always valid for strings
                lastValue = box.Text;
                NotifyValueChanged();
            };
        }

        protected override void SetControlValue(string value)
        {
            lastValue = value ?? string.Empty;
            box.Text = lastValue;
        }

        protected override string GetControlValue()
        {
            // Always return the last known value (never null)
            return lastValue;
        }
    }
}
