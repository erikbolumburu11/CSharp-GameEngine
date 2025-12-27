using OpenTK.Mathematics;
using System.Windows.Forms;

public class Vector3Control : FieldControlBase<Vector3>
{
    private TextBox xBox = new TextBox { Width = 50 };
    private TextBox yBox = new TextBox { Width = 50 };
    private TextBox zBox = new TextBox { Width = 50 };

    public Vector3Control()
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
        AddAxis("Z:", zBox);

        Margin = Padding.Empty;
        Padding = Padding.Empty;
        Controls.Add(layout);

        xBox.TextChanged += (s, e) => NotifyValueChanged();
        yBox.TextChanged += (s, e) => NotifyValueChanged();
        zBox.TextChanged += (s, e) => NotifyValueChanged();
    }

    protected override void SetControlValue(Vector3 value)
    {
        xBox.Text = value.X.ToString();
        yBox.Text = value.Y.ToString();
        zBox.Text = value.Z.ToString();
    }

    protected override Vector3 GetControlValue()
    {
        float.TryParse(xBox.Text, out float x);
        float.TryParse(yBox.Text, out float y);
        float.TryParse(zBox.Text, out float z);
        return new Vector3(x, y, z);
    }
}
