using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine
{
    public class Inspector : DockContent
    {
        public Inspector()
        {
            Text = "Inspector";
            BackColor = Color.Gray;
        }

        private void InitializeComponent()
        {

        }
    }
}
