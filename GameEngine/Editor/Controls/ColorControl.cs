using System.Drawing;

namespace GameEngine.Editor
{
    public class ColorControl : FieldControlBase<Color>
    {
        private readonly Panel swatch = new Panel
        {
            Width = 18,
            Height = 18,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 3, 6, 0)
        };

        private readonly Button changeButton = new Button
        {
            Text = "Change Color",
            Width = 120,
            Height = 22,
            Margin = new Padding(0, 0, 0, 0)
        };

        private readonly ColorDialog dialog = new ColorDialog();
        private Color currentColor = Color.White;

        public ColorControl()
        {
            var layout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            layout.Controls.Add(swatch);
            layout.Controls.Add(changeButton);
            Controls.Add(layout);

            changeButton.Click += (s, e) =>
            {
                dialog.Color = currentColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Value = dialog.Color;
                    NotifyValueChanged();
                }
            };
        }

        protected override void SetControlValue(Color value)
        {
            currentColor = value;
            swatch.BackColor = value;
        }

        protected override Color GetControlValue()
        {
            return currentColor;
        }
    }
}
