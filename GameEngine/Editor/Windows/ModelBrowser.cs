using GameEngine.Engine;
using SharpGLTF.Schema2;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class ModelBrowser : DockContent
    {
        private static readonly HashSet<string> ModelExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".gltf",
            ".glb"
        };

        private readonly EditorState editorState;
        private readonly ListView modelList;
        private readonly Font headerFont;

        public ModelBrowser(EditorState editorState)
        {
            this.editorState = editorState ?? throw new ArgumentNullException(nameof(editorState));
            Text = "Models";

            modelList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                OwnerDraw = true
            };
            modelList.Columns.Add("Model", -2, HorizontalAlignment.Left);
            modelList.DoubleClick += OnModelDoubleClick;
            modelList.DrawColumnHeader += OnDrawColumnHeader;
            modelList.DrawItem += (s, e) => e.DrawDefault = true;
            modelList.DrawSubItem += (s, e) => e.DrawDefault = true;
            modelList.SizeChanged += (s, e) => UpdateColumnWidth();
            modelList.ColumnWidthChanging += OnColumnWidthChanging;

            Controls.Add(modelList);

            headerFont = new Font(modelList.Font, FontStyle.Bold);
            Disposed += (_, _) => headerFont.Dispose();

            RefreshModelList();
        }

        public void RefreshModelListFromEditor()
        {
            RefreshModelList();
        }

        private void RefreshModelList()
        {
            modelList.Items.Clear();

            if (!ProjectContext.HasProject || ProjectContext.Current == null)
                return;

            string assetRoot = ProjectContext.Current.Paths.AssetRootAbsolute;
            if (Directory.Exists(assetRoot))
                AssetDatabase.ScanAssets(assetRoot);
            else
                return;

            var items = Directory.EnumerateFiles(assetRoot, "*.*", SearchOption.AllDirectories)
                .Where(path => ModelExtensions.Contains(Path.GetExtension(path)))
                .Select(path =>
                {
                    try
                    {
                        return ProjectContext.Current.Paths.ToProjectRelative(path);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

            foreach (var path in items)
            {
                if (!TryGetGuidFromPath(path!, out var guid))
                    continue;

                var entry = new ModelEntry(guid, path!);
                string displayName = Path.GetFileName(entry.Path);
                var item = new ListViewItem(displayName)
                {
                    Tag = entry
                };
                modelList.Items.Add(item);
            }

            UpdateColumnWidth();
        }

        private void OnModelDoubleClick(object? sender, EventArgs e)
        {
            if (modelList.SelectedItems.Count == 0)
                return;

            if (modelList.SelectedItems[0].Tag is not ModelEntry entry)
                return;

            InstantiateModel(entry);
        }

        private void OnDrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            using var backBrush = new SolidBrush(SystemColors.ControlLight);
            e.Graphics.FillRectangle(backBrush, e.Bounds);

            using var borderPen = new Pen(SystemColors.ControlDark);
            e.Graphics.DrawLine(
                borderPen,
                e.Bounds.Left,
                e.Bounds.Bottom - 1,
                e.Bounds.Right,
                e.Bounds.Bottom - 1
            );

            var textBounds = new Rectangle(
                e.Bounds.X + 4,
                e.Bounds.Y,
                e.Bounds.Width - 4,
                e.Bounds.Height
            );
            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text,
                headerFont,
                textBounds,
                e.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
            );
        }

        private void InstantiateModel(ModelEntry entry)
        {
            if (!ProjectContext.HasProject || ProjectContext.Current == null)
            {
                var project = ProjectDialogs.CreateProjectWithDialog(this);
                if (project == null)
                    return;
            }

            if (!AssetDatabase.TryLoad(entry.Guid, out ModelRoot model))
            {
                MessageBox.Show(
                    this,
                    "Failed to load the selected model. Try rescanning assets or reimporting the file.",
                    "Model Load Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var instantiator = editorState.engineHost.gltfInstantiator;
            GameObject root = instantiator.Instantiate(model, entry.Guid, entry.Path);
            editorState.Select(root);
        }

        private void OnColumnWidthChanging(object? sender, ColumnWidthChangingEventArgs e)
        {
            if (modelList.Columns.Count == 0 || e.ColumnIndex != 0)
                return;

            e.NewWidth = modelList.Columns[0].Width;
            e.Cancel = true;
        }

        private void UpdateColumnWidth()
        {
            if (modelList.Columns.Count == 0)
                return;

            int width = Math.Max(0, modelList.ClientSize.Width - 2);
            modelList.Columns[0].Width = width;
        }

        private static bool TryGetGuidFromPath(string path, out Guid guid)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                guid = default;
                return false;
            }

            if (TryGetGuidByLookup(path, out guid))
                return true;

            string altPath = path.Replace('/', '\\');
            if (!string.Equals(altPath, path, StringComparison.Ordinal) && TryGetGuidByLookup(altPath, out guid))
                return true;

            if (!Path.IsPathRooted(path) && ProjectContext.Current != null)
            {
                string absPath = ProjectContext.Current.Paths.ToAbsolute(path);
                if (TryGetGuidByLookup(absPath, out guid))
                    return true;

                string altAbsPath = absPath.Replace('/', '\\');
                if (!string.Equals(altAbsPath, absPath, StringComparison.Ordinal) && TryGetGuidByLookup(altAbsPath, out guid))
                    return true;
            }

            guid = default;
            return false;
        }

        private static bool TryGetGuidByLookup(string path, out Guid guid)
        {
            try
            {
                guid = AssetDatabase.PathToGuid(path);
                return true;
            }
            catch
            {
                guid = default;
                return false;
            }
        }

        private sealed class ModelEntry
        {
            public Guid Guid { get; }
            public string Path { get; }

            public ModelEntry(Guid guid, string path)
            {
                Guid = guid;
                Path = path;
            }
        }
    }
}
