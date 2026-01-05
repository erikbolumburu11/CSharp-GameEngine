using GameEngine.Engine;
using OpenTK.Mathematics;
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
        private ComboBox diffuseTextureComboBox;
        private ComboBox specularTextureComboBox;
        private Button diffuseTextureBrowseButton;
        private Button specularTextureBrowseButton;
        private Vector2Control uvTilingControl;
        private Vector2Control uvOffsetControl;
        private Material? currentMaterial;
        private string? currentMaterialPath;
        private const string NoTextureLabel = "(None)";
        private static readonly HashSet<string> TextureExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".tga",
            ".bmp"
        };

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
            AddSection(BuildSpecularTextureEditor());
            AddSection(BuildUvEditor());
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
            return BuildTextureComboEditor(
                "Diffuse Texture",
                out diffuseTextureComboBox,
                out diffuseTextureBrowseButton,
                OnDiffuseTextureSelectionChanged,
                OnDiffuseTextureBrowseClicked
            );
        }

        private Control BuildSpecularTextureEditor()
        {
            return BuildTextureComboEditor(
                "Specular Texture",
                out specularTextureComboBox,
                out specularTextureBrowseButton,
                OnSpecularTextureSelectionChanged,
                OnSpecularTextureBrowseClicked
            );
        }

        private Control BuildTextureComboEditor(
            string header,
            out ComboBox comboBox,
            out Button browseButton,
            EventHandler selectionChanged,
            EventHandler browseClicked)
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
                Text = header,
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 2),
                Dock = DockStyle.Top
            };
            section.Controls.Add(headerLabel);

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

            var localComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 0, 4, 0)
            };
            localComboBox.SelectedIndexChanged += selectionChanged;
            row.Controls.Add(localComboBox, 0, 0);

            var localBrowseButton = new Button
            {
                Text = "Browse...",
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 0, 0, 0)
            };
            localBrowseButton.Click += browseClicked;
            row.Controls.Add(localBrowseButton, 1, 0);

            section.Controls.Add(row);
            section.Controls.SetChildIndex(row, 0);

            section.Resize += (s, e) => row.Width = section.ClientSize.Width;
            row.Width = section.ClientSize.Width;

            comboBox = localComboBox;
            browseButton = localBrowseButton;

            return section;
        }

        private Control BuildUvEditor()
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
                Text = "UV",
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 2),
                Dock = DockStyle.Top
            };
            section.Controls.Add(headerLabel);

            var table = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var tilingLabel = new Label
            {
                Text = "Tiling",
                AutoSize = true,
                Margin = new Padding(0, 3, 6, 0)
            };
            uvTilingControl = new Vector2Control { Dock = DockStyle.Fill };
            uvTilingControl.ValueChanged += v => OnUvTilingChanged((Vector2)v);

            var offsetLabel = new Label
            {
                Text = "Offset",
                AutoSize = true,
                Margin = new Padding(0, 3, 6, 0)
            };
            uvOffsetControl = new Vector2Control { Dock = DockStyle.Fill };
            uvOffsetControl.ValueChanged += v => OnUvOffsetChanged((Vector2)v);

            table.Controls.Add(tilingLabel, 0, 0);
            table.Controls.Add(uvTilingControl, 1, 0);
            table.Controls.Add(offsetLabel, 0, 1);
            table.Controls.Add(uvOffsetControl, 1, 1);

            section.Controls.Add(table);
            section.Controls.SetChildIndex(table, 0);

            section.Resize += (s, e) => table.Width = section.ClientSize.Width;
            table.Width = section.ClientSize.Width;

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
                RefreshTextureLists();
                SetUvControls(null);
                SetTextureSelection(diffuseTextureComboBox, null);
                SetTextureSelection(specularTextureComboBox, null);
                return;
            }

            currentMaterialPath = relPath;
            currentMaterial = editorState.engineHost.materialManager.Get(relPath);
            RefreshTextureLists();
            SetTextureSelection(diffuseTextureComboBox, currentMaterial.diffuseTex);
            SetTextureSelection(specularTextureComboBox, currentMaterial.specularTex);
            SetUvControls(currentMaterial);
        }

        private void OnDiffuseTextureSelectionChanged(object? sender, EventArgs e)
        {
            if (!TryGetCurrentMaterial(out var material, out var path))
                return;

            material.diffuseTex = GetSelectedTexture(diffuseTextureComboBox);
            MaterialSerializer.SaveMaterial(material, path);
        }

        private void OnDiffuseTextureBrowseClicked(object? sender, EventArgs e)
        {
            if (!TryGetCurrentMaterial(out var material, out var path))
                return;

            string? relPath = BrowseForTexture("Select Diffuse Texture");
            if (string.IsNullOrWhiteSpace(relPath))
                return;

            material.diffuseTex = relPath;
            MaterialSerializer.SaveMaterial(material, path);
            SetTextureSelection(diffuseTextureComboBox, relPath);
        }

        private void OnSpecularTextureSelectionChanged(object? sender, EventArgs e)
        {
            if (!TryGetCurrentMaterial(out var material, out var path))
                return;

            material.specularTex = GetSelectedTexture(specularTextureComboBox);
            MaterialSerializer.SaveMaterial(material, path);
        }

        private void OnSpecularTextureBrowseClicked(object? sender, EventArgs e)
        {
            if (!TryGetCurrentMaterial(out var material, out var path))
                return;

            string? relPath = BrowseForTexture("Select Specular Texture");
            if (string.IsNullOrWhiteSpace(relPath))
                return;

            material.specularTex = relPath;
            MaterialSerializer.SaveMaterial(material, path);
            SetTextureSelection(specularTextureComboBox, relPath);
        }

        public void RefreshMaterialListFromEditor()
        {
            RefreshMaterialList();
        }

        private void RefreshMaterialList()
        {
            materialComboBox.Items.Clear();

            if (!ProjectContext.HasProject || ProjectContext.Current == null)
            {
                RefreshTextureLists();
                return;
            }

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

            RefreshTextureLists();
        }

        private void RefreshTextureLists()
        {
            if (diffuseTextureComboBox == null || specularTextureComboBox == null)
                return;

            RefreshTextureCombo(diffuseTextureComboBox);
            RefreshTextureCombo(specularTextureComboBox);

            if (currentMaterial != null)
            {
                SetTextureSelection(diffuseTextureComboBox, currentMaterial.diffuseTex);
                SetTextureSelection(specularTextureComboBox, currentMaterial.specularTex);
                SetUvControls(currentMaterial);
            }
        }

        private void RefreshTextureCombo(ComboBox combo)
        {
            combo.Items.Clear();
            combo.Items.Add(NoTextureLabel);

            foreach (var path in EnumerateTexturePaths())
                combo.Items.Add(path);
        }

        private IEnumerable<string> EnumerateTexturePaths()
        {
            if (ProjectContext.Current == null)
                yield break;

            string texturesDir = Path.Combine(ProjectContext.Current.Paths.AssetRootAbsolute, "Textures");
            if (!Directory.Exists(texturesDir))
                yield break;

            var textures = Directory.EnumerateFiles(texturesDir, "*.*", SearchOption.AllDirectories)
                .Where(path => TextureExtensions.Contains(Path.GetExtension(path)))
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

            foreach (var path in textures)
                yield return path!;
        }

        private string? BrowseForTexture(string title)
        {
            if (ProjectContext.Current == null)
                return null;

            string texturesDir = Path.Combine(ProjectContext.Current.Paths.AssetRootAbsolute, "Textures");
            Directory.CreateDirectory(texturesDir);

            using var ofd = new OpenFileDialog
            {
                Title = title,
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.tga;*.bmp)|*.png;*.jpg;*.jpeg;*.tga;*.bmp|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false,
                InitialDirectory = texturesDir
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return null;

            try
            {
                return ProjectContext.Current.Paths.ToProjectRelative(ofd.FileName);
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
                return null;
            }
        }

        private void SetTextureSelection(ComboBox combo, string? relPath)
        {
            string selection = string.IsNullOrWhiteSpace(relPath) ? NoTextureLabel : relPath;
            int index = combo.Items.IndexOf(selection);
            if (index >= 0)
            {
                combo.SelectedIndex = index;
                return;
            }

            combo.Items.Add(selection);
            combo.SelectedItem = selection;
        }

        private string? GetSelectedTexture(ComboBox combo)
        {
            if (combo.SelectedItem is string selected)
            {
                if (string.Equals(selected, NoTextureLabel, StringComparison.Ordinal))
                    return null;
                return selected;
            }

            return null;
        }

        private bool TryGetCurrentMaterial(out Material material, out string relPath)
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
                material = null!;
                relPath = string.Empty;
                return false;
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
                material = null!;
                relPath = string.Empty;
                return false;
            }

            material = currentMaterial;
            relPath = currentMaterialPath;
            return true;
        }

        private void OnUvTilingChanged(Vector2 value)
        {
            if (!TryGetCurrentMaterial(out var material, out var path))
                return;

            material.uvTiling = value;
            MaterialSerializer.SaveMaterial(material, path);
        }

        private void OnUvOffsetChanged(Vector2 value)
        {
            if (!TryGetCurrentMaterial(out var material, out var path))
                return;

            material.uvOffset = value;
            MaterialSerializer.SaveMaterial(material, path);
        }

        private void SetUvControls(Material? material)
        {
            if (uvTilingControl == null || uvOffsetControl == null)
                return;

            if (material == null)
            {
                uvTilingControl.Enabled = false;
                uvOffsetControl.Enabled = false;
                uvTilingControl.Value = new Vector2(1f, 1f);
                uvOffsetControl.Value = Vector2.Zero;
                return;
            }

            uvTilingControl.Enabled = true;
            uvOffsetControl.Enabled = true;
            uvTilingControl.Value = material.uvTiling;
            uvOffsetControl.Value = material.uvOffset;
        }
    }
}
