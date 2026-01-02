using GameEngine.Engine;
using GameEngine.Engine.Components;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class Inspector : DockContent
    {
        private readonly EditorState editorState;
        private readonly GameObjectManager gameObjectManager;

        private Panel scrollPanel;
        private TableLayoutPanel inspectorLayout;
        private Button addComponentButton;

        public Inspector(EditorState editorState, GameObjectManager gameObjectManager)
        {
            this.editorState = editorState;
            this.gameObjectManager = gameObjectManager;

            Text = "Inspector";

            InitializeUI();
            HookEvents();
        }

        private void InitializeUI()
        {
            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            Controls.Add(scrollPanel);

            inspectorLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Margin = Padding.Empty,
                Padding = new Padding(6, 6, 6, 6)
            };
            inspectorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            scrollPanel.Controls.Add(inspectorLayout);
            scrollPanel.Resize += (s, e) => inspectorLayout.Width = scrollPanel.ClientSize.Width;

            addComponentButton = CreateAddComponentButton();

            scrollPanel.Hide();
        }

        private Button CreateAddComponentButton()
        {
            ContextMenuStrip cms = new ContextMenuStrip();

            foreach (var componentType in ComponentTypeRegistry.Types.Values)
            {
                if (componentType == typeof(Component))
                    continue;

                cms.Items.Add(new ToolStripMenuItem(
                    componentType.Name,
                    null,
                    (s, e) =>
                    {
                        var selected = editorState.SelectedObject;
                        if (selected == null)
                            return;

                        selected.AddComponent(componentType);
                        UpdateInspectorFields(selected);
                    }
                ));
            }

            Button addComponentButton = new Button
            {
                Text = "Add Component",
                Width = 150,
            };

            addComponentButton.Click += (s, e) =>
            {
                cms.Show();
            };

            return addComponentButton;
        }

        private void HookEvents()
        {
            editorState.OnSelectionChanged += UpdateInspectorFields;
        }

        private void UpdateInspectorFields(GameObject obj)
        {
            if (obj == null)
            {
                scrollPanel.Hide();
                ClearInspector();
                return;
            }

            scrollPanel.Show();

            BuildInspector(obj);
        }

        private void BuildInspector(GameObject obj)
        {
            ClearInspector();

            AddSection(CreateEditorSection(null, new GameObjectEditor(obj)));
            AddSeparator();
            AddSection(CreateEditorSection("Transform", new TransformEditor(obj)));

            foreach (Component comp in obj.Components)
            {
                if (comp is Light light)
                {
                    AddSeparator();
                    AddSection(CreateEditorSection(comp.name, new LightEditor(light)));
                }
            }

            AddSeparator();
            addComponentButton.Margin = new Padding(0, 8, 0, 8);
            AddSection(addComponentButton);
        }

        private Control CreateEditorSection<T>(string? header, Editor<T> editor)
        {
            var section = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2),
                Padding = Padding.Empty
            };

            if (!string.IsNullOrWhiteSpace(header))
            {
                var headerLabel = new Label
                {
                    Text = header,
                    Font = new Font(Font, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 0, 2),
                    Dock = DockStyle.Top
                };
                section.Controls.Add(headerLabel);
            }

            var table = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            foreach (var field in editor.fields)
            {
                int row = table.RowCount++;
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var label = new Label
                {
                    Text = field.label,
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0, 2, 0, 0)
                };
                table.Controls.Add(label, 0, row++);

                table.RowCount = row + 1;
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var control = FieldBinder.CreateBoundControl(field);
                control.Dock = DockStyle.Fill;
                control.Margin = new Padding(0, 0, 0, 4);
                table.Controls.Add(control, 0, row);
            }

            section.Controls.Add(table);
            if (!string.IsNullOrWhiteSpace(header))
                section.Controls.SetChildIndex(table, 0);
            section.Resize += (s, e) => table.Width = section.ClientSize.Width;
            table.Width = section.ClientSize.Width;

            return section;
        }

        private void ClearInspector()
        {
            inspectorLayout.Controls.Clear();
            inspectorLayout.RowStyles.Clear();
            inspectorLayout.RowCount = 0;
        }

        private void AddSection(Control control)
        {
            int row = inspectorLayout.RowCount++;
            inspectorLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inspectorLayout.Controls.Add(control, 0, row);
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
    }
}
