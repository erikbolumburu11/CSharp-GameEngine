using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class Vector3Control : UserControl
    {
        private TextBox xBox;
        private TextBox yBox;
        private TextBox zBox;

        public event Action<Vector3>? ValueChanged;


        public Vector3Control()
        {

            xBox = new TextBox { Width = 60 };
            yBox = new TextBox { Width = 60 };
            zBox = new TextBox { Width = 60 };

            FlowLayoutPanel mainLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };

            // Create X row
            FlowLayoutPanel xPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            xBox = new TextBox { Width = 60 };
            xPanel.Controls.Add(new Label { Text = "X:", AutoSize = true });
            xPanel.Controls.Add(xBox);
            mainLayout.Controls.Add(xPanel);

            // Create Y row
            FlowLayoutPanel yPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            yBox = new TextBox { Width = 60 };
            yPanel.Controls.Add(new Label { Text = "Y:", AutoSize = true });
            yPanel.Controls.Add(yBox);
            mainLayout.Controls.Add(yPanel);

            // Create Z row
            FlowLayoutPanel zPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            zBox = new TextBox { Width = 60 };
            zPanel.Controls.Add(new Label { Text = "Z:", AutoSize = true });
            zPanel.Controls.Add(zBox);
            mainLayout.Controls.Add(zPanel);

            Controls.Add(mainLayout);

            xBox.TextChanged += OnChanged;
            yBox.TextChanged += OnChanged;
            zBox.TextChanged += OnChanged;
        }

        private void OnChanged(object? sender, EventArgs e)
        {
            ValueChanged?.Invoke(Value);
        }

        public void SetValues(Vector3 vector)
        {
            xBox.Text = vector.X.ToString();
            yBox.Text = vector.Y.ToString();
            zBox.Text = vector.Z.ToString();
        }

        public Vector3 Value
        {
            get
            {
                float.TryParse(xBox.Text, out var x);
                float.TryParse(yBox.Text, out var y);
                float.TryParse(zBox.Text, out var z);
                return new Vector3(x, y, z);
            }
            set
            {
                xBox.Text = value.X.ToString();
                yBox.Text = value.Y.ToString();
                zBox.Text = value.Z.ToString();
            }
        }
    }
}
