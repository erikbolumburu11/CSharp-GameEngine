using GameEngine.Engine;
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
            TextBox ambientLightIntensityTextBox = new TextBox
            {
                Text = scene.ambientLightIntensity.ToString()
            };
            ambientLightIntensityTextBox.TextChanged += (s, e) =>
            {
                if (float.TryParse(ambientLightIntensityTextBox.Text, out float value)) ;
                scene.ambientLightIntensity = value;
            };

            Label skyboxColorLabel = new Label
            {
                Text = "Skybox Color:"
            };

            ColorDialog skyboxColorDialog = new();

            Button skyBoxColorButton = new Button
            {
                Text = "Change Color",
                Width = 150,
            };
            skyBoxColorButton.Click += (s, e) =>
            {
                if (skyboxColorDialog.ShowDialog() == DialogResult.OK)
                {
                    scene.skyboxColor = skyboxColorDialog.Color;
                }
            };

            Label skyboxHdrLabel = new Label
            {
                Text = "Skybox HDR:"
            };

            TextBox skyboxHdrTextBox = new TextBox
            {
                ReadOnly = true,
                Width = 260,
                Text = scene.skyboxHdrPath ?? string.Empty
            };

            Button skyboxHdrBrowseButton = new Button
            {
                Text = "Browse...",
                Width = 150
            };

            Button skyboxHdrClearButton = new Button
            {
                Text = "Clear",
                Width = 150
            };

            skyboxHdrBrowseButton.Click += (s, e) =>
            {
                if (ProjectContext.Current == null)
                    return;

                string texturesDir = Path.Combine(ProjectContext.Current.Paths.AssetRootAbsolute, "Textures");
                Directory.CreateDirectory(texturesDir);

                using var ofd = new OpenFileDialog
                {
                    Title = "Select Skybox HDR",
                    Filter = "HDR Image (*.hdr)|*.hdr|All files (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false,
                    InitialDirectory = texturesDir
                };

                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    string relPath = ProjectContext.Current.Paths.ToProjectRelative(ofd.FileName);
                    scene.skyboxHdrPath = relPath;
                    skyboxHdrTextBox.Text = relPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        ex.Message,
                        "Invalid HDR Path",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            };

            skyboxHdrClearButton.Click += (s, e) =>
            {
                scene.skyboxHdrPath = null;
                skyboxHdrTextBox.Text = string.Empty;
            };

            Label skyboxExposureLabel = new Label
            {
                Text = "Skybox Exposure:"
            };

            TextBox skyboxExposureTextBox = new TextBox
            {
                Text = scene.skyboxExposure.ToString()
            };
            skyboxExposureTextBox.TextChanged += (s, e) =>
            {
                if (float.TryParse(skyboxExposureTextBox.Text, out float value))
                    scene.skyboxExposure = Math.Max(0.0f, value);
            };

            Label iblSpecularIntensityLabel = new Label
            {
                Text = "IBL Specular Intensity:"
            };

            TextBox iblSpecularIntensityTextBox = new TextBox
            {
                Text = scene.iblSpecularIntensity.ToString()
            };
            iblSpecularIntensityTextBox.TextChanged += (s, e) =>
            {
                if (float.TryParse(iblSpecularIntensityTextBox.Text, out float value))
                    scene.iblSpecularIntensity = Math.Max(0.0f, value);
            };

            CheckBox skyboxFlipVCheckBox = new CheckBox
            {
                Text = "Flip Skybox V",
                Checked = scene.skyboxFlipV,
                AutoSize = true
            };
            skyboxFlipVCheckBox.CheckedChanged += (s, e) =>
            {
                scene.skyboxFlipV = skyboxFlipVCheckBox.Checked;
            };

            layout.Controls.Add(ambientLightIntensityLabel);
            layout.Controls.Add(ambientLightIntensityTextBox);
            layout.Controls.Add(skyboxColorLabel);
            layout.Controls.Add(skyBoxColorButton);
            layout.Controls.Add(skyboxHdrLabel);
            layout.Controls.Add(skyboxHdrTextBox);
            layout.Controls.Add(skyboxHdrBrowseButton);
            layout.Controls.Add(skyboxHdrClearButton);
            layout.Controls.Add(skyboxExposureLabel);
            layout.Controls.Add(skyboxExposureTextBox);
            layout.Controls.Add(iblSpecularIntensityLabel);
            layout.Controls.Add(iblSpecularIntensityTextBox);
            layout.Controls.Add(skyboxFlipVCheckBox);
        }
    }
}
