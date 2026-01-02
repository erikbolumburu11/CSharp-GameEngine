using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class MaterialEditor : DockContent
    {
        private Panel scrollPanel;
        private TableLayoutPanel editorLayout;
        private ComboBox materialComboBox;
        private Button newMaterialButton;
        private Button deleteMaterialButton;

        public MaterialEditor()
        {
            Text = "Material Editor";
            InitializeUI();
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
    }
}
