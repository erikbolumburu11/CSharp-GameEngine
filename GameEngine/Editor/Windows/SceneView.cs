using OpenTK.GLControl;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class SceneView : DockContent
    {
        public GLControl glControl;

        public SceneView()
        {
            Text = "Scene View";

            var settings = new GLControlSettings
            {
                NumberOfSamples = 4
            };

            glControl = new GLControl(settings)
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                APIVersion = new Version(4, 6)
            };

            CloseButton = false;
            CloseButtonVisible = false;

            Controls.Add(glControl);
        }
    }
}
