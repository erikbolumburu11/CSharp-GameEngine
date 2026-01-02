using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    partial class Editor
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Editor));
            toolStrip1 = new ToolStrip();
            File = new ToolStripDropDownButton();
            saveToolStripMenuItem = new ToolStripMenuItem();
            loadToolStripMenuItem = new ToolStripMenuItem();
            sceneSettingsButton = new ToolStripButton();
            dockPanel = new DockPanel();
            newProjectToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { File, sceneSettingsButton });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(800, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // File
            // 
            File.DisplayStyle = ToolStripItemDisplayStyle.Text;
            File.DropDownItems.AddRange(new ToolStripItem[] { newProjectToolStripMenuItem, saveToolStripMenuItem, loadToolStripMenuItem });
            File.Image = (Image)resources.GetObject("File.Image");
            File.ImageTransparentColor = Color.Magenta;
            File.Name = "File";
            File.Size = new Size(38, 22);
            File.Text = "File";
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(180, 22);
            saveToolStripMenuItem.Text = "Save Project";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new Size(180, 22);
            loadToolStripMenuItem.Text = "Load Project";
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            // 
            // sceneSettingsButton
            // 
            sceneSettingsButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            sceneSettingsButton.Image = (Image)resources.GetObject("sceneSettingsButton.Image");
            sceneSettingsButton.ImageTransparentColor = Color.Magenta;
            sceneSettingsButton.Name = "sceneSettingsButton";
            sceneSettingsButton.Size = new Size(87, 22);
            sceneSettingsButton.Text = "Scene Settings";
            sceneSettingsButton.ToolTipText = "Scene Settings";
            sceneSettingsButton.Click += sceneSettingsButton_Click;
            // 
            // dockPanel
            // 
            dockPanel.Dock = DockStyle.Fill;
            dockPanel.Location = new Point(0, 25);
            dockPanel.Name = "dockPanel";
            dockPanel.Size = new Size(800, 425);
            dockPanel.TabIndex = 1;
            // 
            // newProjectToolStripMenuItem
            // 
            newProjectToolStripMenuItem.Name = "newProjectToolStripMenuItem";
            newProjectToolStripMenuItem.Size = new Size(180, 22);
            newProjectToolStripMenuItem.Text = "New Project";
            newProjectToolStripMenuItem.Click += newProjectToolStripMenuItem_Click;
            // 
            // Editor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dockPanel);
            Controls.Add(toolStrip1);
            Name = "Editor";
            Text = "Editor";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripDropDownButton File;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem loadToolStripMenuItem;
        private DockPanel dockPanel;
        private ToolStripButton sceneSettingsButton;
        private ToolStripMenuItem newProjectToolStripMenuItem;
    }
}