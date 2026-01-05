using OpenTK.Mathematics;

namespace GameEngine.Editor
{
    public class Vector2Control : FieldControlBase<Vector2>
    {
        private TextBox xBox = new TextBox { Width = 50 };
        private TextBox yBox = new TextBox { Width = 50 };

        public Vector2Control()
        {
            var layout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            void AddAxis(string label, TextBox box)
            {
                var axisLabel = new Label
                {
                    Text = label,
                    AutoSize = true,
                    Margin = new Padding(0, 3, 2, 0)
                };
                box.Margin = new Padding(0, 0, 8, 0);
                layout.Controls.Add(axisLabel);
                layout.Controls.Add(box);
            }

            AddAxis("X:", xBox);
            AddAxis("Y:", yBox);

            Margin = Padding.Empty;
            Padding = Padding.Empty;
            Controls.Add(layout);

            xBox.TextChanged += (s, e) => NotifyValueChanged();
            yBox.TextChanged += (s, e) => NotifyValueChanged();
        }

        protected override void SetControlValue(Vector2 value)
        {
            xBox.Text = value.X.ToString();
            yBox.Text = value.Y.ToString();
        }

        protected override Vector2 GetControlValue()
        {
            float.TryParse(xBox.Text, out float x);
            float.TryParse(yBox.Text, out float y);
            return new Vector2(x, y);
        }
    }
}
