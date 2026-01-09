using System;
using System.IO;
using System.Windows.Forms;
using GameEngine.Engine;

namespace GameEngine.Editor
{
    public static class ProjectDialogs
    {
        public static Project? CreateProjectWithDialog(IWin32Window owner)
        {
            // Choose parent directory
            using var folder = new FolderBrowserDialog
            {
                Description = "Choose where to create the project folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (folder.ShowDialog(owner) != DialogResult.OK)
                return null;

            // Ask for project name
            string? name = Prompt("New Project", "Project name:");
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Create on disk
            var project = Project.CreateNew(folder.SelectedPath, name.Trim());
            ProjectContext.Set(project);
            return project;
        }

        public static Project? OpenProjectWithDialog(EditorState editorState, IWin32Window owner)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Open Project",
                Filter = $"GameEngine Project (*{Project.ProjectExtension})|*{Project.ProjectExtension}",
                CheckFileExists = true,
                Multiselect = false
            };

            if (ofd.ShowDialog(owner) != DialogResult.OK)
                return null;

            var project = Project.Open(editorState.engineHost, ofd.FileName);
            ProjectContext.Set(project);
            return project;
        }

        private static string? Prompt(string title, string label)
        {
            using var form = new Form
            {
                Width = 420,
                Height = 160,
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label { Left = 12, Top = 14, Width = 380, Text = label };
            var tb = new TextBox { Left = 12, Top = 40, Width = 380 };
            var ok = new Button { Text = "OK", Left = 232, Width = 80, Top = 74, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", Left = 312, Width = 80, Top = 74, DialogResult = DialogResult.Cancel };

            form.Controls.AddRange(new Control[] { lbl, tb, ok, cancel });
            form.AcceptButton = ok;
            form.CancelButton = cancel;

            return form.ShowDialog() == DialogResult.OK ? tb.Text : null;
        }
    }
}
