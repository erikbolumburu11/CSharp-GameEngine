using GameEngine.Engine;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class MaterialEditor : DockContent
    {
        private readonly EditorState editorState;
        private Panel scrollPanel;
        private TableLayoutPanel editorLayout;
        private ComboBox materialComboBox;
        private Button newMaterialButton;
        private Button deleteMaterialButton;
        private TextBox diffuseTextureTextBox;
        private Button diffuseTextureBrowseButton;
        private Material? currentMaterial;
        private string? currentMaterialPath;

        public MaterialEditor(EditorState editorState)
        {
            this.editorState = editorState ?? throw new ArgumentNullException(nameof(editorState));
            Text = "Material Editor";
            InitializeUI();
            RefreshMaterialList();
        }

        private void InitializeUI()
        {
            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            Controls.Add(scrollPanel);

            editorLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Margin = Padding.Empty,
                Padding = new Padding(6, 6, 6, 6)
            };
            editorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            scrollPanel.Controls.Add(editorLayout);
            scrollPanel.Resize += (s, e) => editorLayout.Width = scrollPanel.ClientSize.Width;

            AddSection(BuildMaterialSelector());
            AddSection(BuildMaterialActions());
            AddSection(BuildDiffuseTextureEditor());
            AddSeparator();
        }

        private Control BuildMaterialSelector()
        {
            var section = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2),
                Padding = Padding.Empty
            };

            var headerLabel = new Label
            {
                Text = "Material",
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 2),
                Dock = DockStyle.Top
            };
            section.Controls.Add(headerLabel);

            materialComboBox = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 0, 0, 4)
            };
            materialComboBox.SelectedIndexChanged += OnMaterialSelectionChanged;
            section.Controls.Add(materialComboBox);
            section.Controls.SetChildIndex(materialComboBox, 0);

            section.Resize += (s, e) => materialComboBox.Width = section.ClientSize.Width;
            materialComboBox.Width = section.ClientSize.Width;

            return section;
        }

        private Control BuildMaterialActions()
        {
            var section = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2),
                Padding = Padding.Empty
            };

            var actionsLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            newMaterialButton = new Button
            {
                Text = "New Material",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 4, 0)
            };
            newMaterialButton.Click += OnNewMaterialClicked;

            deleteMaterialButton = new Button
            {
                Text = "Delete Material",
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 0, 0, 0)
            };

            actionsLayout.Controls.Add(newMaterialButton, 0, 0);
            actionsLayout.Controls.Add(deleteMaterialButton, 1, 0);

            section.Controls.Add(actionsLayout);
            section.Resize += (s, e) => actionsLayout.Width = section.ClientSize.Width;
            actionsLayout.Width = section.ClientSize.Width;

            return section;
        }

        private Control BuildDiffuseTextureEditor()
        {
            var section = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2),
                Padding = Padding.Empty
            };

            var headerLabel = new Label
            {
                Text = "Diffuse Texture",
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 2),
                Dock = DockStyle.Top
            };
            var row = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            diffuseTextureTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Margin = new Padding(0, 0, 4, 0)
            };

            diffuseTextureBrowseButton = new Button
            {
                Text = "Browse...",
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 0, 0, 0)
            };
            diffuseTextureBrowseButton.Click += OnDiffuseTextureBrowseClicked;

            row.Controls.Add(diffuseTextureTextBox, 0, 0);
            row.Controls.Add(diffuseTextureBrowseButton, 1, 0);

            section.Controls.Add(row);
            section.Controls.Add(headerLabel);
            section.Resize += (s, e) => row.Width = section.ClientSize.Width;
            row.Width = section.ClientSize.Width;

            return section;
        }

        private void AddSection(Control control)
        {
            int row = editorLayout.RowCount++;
            editorLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            editorLayout.Controls.Add(control, 0, row);
        }

        private void AddSeparator()
        {
            var separator = new Panel
            {
                Height = 1,
                Dock = DockStyle.Top,
                BackColor = SystemColors.ControlDark,
                Margin = new Padding(0, 6, 0, 6)
            };
            AddSection(separator);
        }

        private void OnNewMaterialClicked(object? sender, EventArgs e)
        {
            if (!ProjectContext.HasProject)
            {
                var project = ProjectDialogs.CreateProjectWithDialog(this);
                if (project == null)
                    return;
            }

            if (ProjectContext.Current == null)
                return;

            RefreshMaterialList();

            string materialsDir = Path.Combine(ProjectContext.Current.Paths.AssetRootAbsolute, "Materials");
            Directory.CreateDirectory(materialsDir);

            using var sfd = new SaveFileDialog
            {
                Title = "Create Material",
                Filter = "Material (*.mat)|*.mat|All files (*.*)|*.*",
                DefaultExt = "mat",
                AddExtension = true,
                OverwritePrompt = true,
                InitialDirectory = materialsDir
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            string relPath;
            try
            {
                relPath = ProjectContext.Current.Paths.ToProjectRelative(sfd.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "Invalid Material Path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var material = new Material();
            MaterialSerializer.SaveMaterial(material, relPath);
            editorState.engineHost.materialManager.Add(relPath, material);

            RefreshMaterialList();
            materialComboBox.SelectedItem = relPath;
        }

        private void OnMaterialSelectionChanged(object? sender, EventArgs e)
        {
            if (materialComboBox.SelectedItem is not string relPath)
            {
                currentMaterial = null;
                currentMaterialPath = null;
                diffuseTextureTextBox.Text = string.Empty;
                return;
            }

            currentMaterialPath = relPath;
            currentMaterial = editorState.engineHost.materialManager.Get(relPath);
            diffuseTextureTextBox.Text = currentMaterial.diffuseTex ?? string.Empty;
        }

        private void OnDiffuseTextureBrowseClicked(object? sender, EventArgs e)
        {
            if (!ProjectContext.HasProject || ProjectContext.Current == null)
            {
                MessageBox.Show(
                    this,
                    "Open or create a project before editing materials.",
                    "No Project",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (currentMaterial == null || string.IsNullOrWhiteSpace(currentMaterialPath))
            {
                MessageBox.Show(
                    this,
                    "Select a material first.",
                    "No Material Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            string texturesDir = Path.Combine(ProjectContext.Current.Paths.AssetRootAbsolute, "Textures");
            Directory.CreateDirectory(texturesDir);

            using var ofd = new OpenFileDialog
            {
                Title = "Select Diffuse Texture",
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.tga;*.bmp)|*.png;*.jpg;*.jpeg;*.tga;*.bmp|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false,
                InitialDirectory = texturesDir
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            string relPath;
            try
            {
                relPath = ProjectContext.Current.Paths.ToProjectRelative(ofd.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "Invalid Texture Path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            currentMaterial.diffuseTex = relPath;
            MaterialSerializer.SaveMaterial(currentMaterial, currentMaterialPath);
            diffuseTextureTextBox.Text = relPath;
        }

        public void RefreshMaterialListFromEditor()
        {
            RefreshMaterialList();
        }

        private void RefreshMaterialList()
        {
            materialComboBox.Items.Clear();

            if (!ProjectContext.HasProject || ProjectContext.Current == null)
                return;

            string rootPath = ProjectContext.Current.RootPath;
            var materialPaths = Directory.EnumerateFiles(rootPath, "*.mat", SearchOption.AllDirectories)
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

            foreach (var path in materialPaths)
                materialComboBox.Items.Add(path);
        }
    }
}
