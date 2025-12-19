using GameEngine.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class SceneSettings : DockContent
    {
        FlowLayoutPanel layout;
        public SceneSettings(Scene scene)
        {
            layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
            };
            Controls.Add(layout);

            Label ambientLightIntensityLabel = new Label
            {
                Text = "Ambient Light Intensity:"
            };
            // TODO: This is broken!
            TextBox ambientLightIntensityTextBox = new TextBox
            {
                Text = scene.ambientLightIntensity.ToString()
            };
            ambientLightIntensityTextBox.TextChanged += (s, e) =>
            {
                if(float.TryParse(ambientLightIntensityTextBox.Text, out float value));
                    scene.ambientLightIntensity = value; 
            };

            Label skyboxColorLabel = new Label
            {
                Text = "skyboxColorLabel"
            };

            ColorDialog skyboxColorDialog = new();

            Button skyBoxColorButton = new Button
            {
                Text = "Change Color",
                Width = 150,
            };
            skyBoxColorButton.Click += (s, e) =>
            {
                if(skyboxColorDialog.ShowDialog() == DialogResult.OK)
                {
                    scene.skyboxColor = skyboxColorDialog.Color;
                }
            };

            layout.Controls.Add(ambientLightIntensityLabel);
            layout.Controls.Add(ambientLightIntensityTextBox);
            layout.Controls.Add(skyboxColorLabel);
            layout.Controls.Add(skyBoxColorButton);
        }
    }
}
