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

            glControl = new GLControl
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
