using OpenTK.GLControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine
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
                BackColor = Color.Black
            };

            CloseButton = false;
            CloseButtonVisible = false;

            Controls.Add(glControl);
        }
    }
}
