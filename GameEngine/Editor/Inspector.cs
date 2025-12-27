using GameEngine.Engine;
using GameEngine.Engine.Components;
using OpenTK.Mathematics;
using System.Reflection;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class Inspector : DockContent
    {
        private readonly EditorState editorState;
        private readonly GameObjectManager gameObjectManager;

        private Panel scrollPanel;

        private TableLayoutPanel transformTable;
        private TableLayoutPanel componentTable;

        private Button addComponentButton;

        private TextBox nameTextBox;
        private Vector3Control positionControl;
        private Vector3Control rotationControl;
        private Vector3Control scaleControl;

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

            transformTable = CreateTable();
            scrollPanel.Controls.Add(transformTable);

            nameTextBox = CreateTextBox();
            AddRow(transformTable, "Name", nameTextBox);

            positionControl = new Vector3Control { Margin = Padding.Empty };
            AddRow(transformTable, "Position", positionControl);

            rotationControl = new Vector3Control { Margin = Padding.Empty };
            AddRow(transformTable, "Rotation", rotationControl);

            scaleControl = new Vector3Control { Margin = Padding.Empty };
            AddRow(transformTable, "Scale", scaleControl);

            addComponentButton = CreateAddComponentButton();
            addComponentButton.Margin = new Padding(0, 8, 0, 8);
            scrollPanel.Controls.Add(addComponentButton);

            componentTable = CreateTable();
            scrollPanel.Controls.Add(componentTable);

            scrollPanel.Hide();
        }

        private TableLayoutPanel CreateTable()
        {
            return new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
        }

        private void AddRow(TableLayoutPanel table, string label, Control field)
        {
            int row = table.RowCount++;
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lbl = new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 3, 6, 3)
            };

            field.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            field.Margin = Padding.Empty;

            table.Controls.Add(lbl, 0, row);
            table.Controls.Add(field, 1, row);
        }

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Width = 120,
                Margin = Padding.Empty
            };
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
                    (s, e) => editorState.SelectedObject?.AddComponent(componentType)
                ));
            }

            return new Button
            {
                Text = "Add Component",
                Width = 150
            };
        }

        private void HookEvents()
        {
            nameTextBox.TextChanged += (s, e) =>
            {
                var obj = editorState.SelectedObject;
                if (obj == null) return;

                gameObjectManager.RenameGameObject(obj, nameTextBox.Text);
            };

            positionControl.ValueChanged += v =>
            {
                editorState.SelectedObject?.SetPosition(v);
            };

            rotationControl.ValueChanged += v =>
            {
                var obj = editorState.SelectedObject;
                if (obj == null) return;

                obj.SetRotation(
                    Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(v.X)) *
                    Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(v.Y)) *
                    Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(v.Z))
                );
            };

            scaleControl.ValueChanged += v =>
            {
                editorState.SelectedObject?.SetScale(v);
            };

            editorState.OnSelectionChanged += UpdateInspectorFields;
        }

        private void UpdateInspectorFields(GameObject obj)
        {
            if (obj == null)
            {
                scrollPanel.Hide();
                ClearComponentTable();
                return;
            }

            scrollPanel.Show();

            nameTextBox.Text = obj.name;
            positionControl.SetValues(obj.transform.position);

            Quaternion.ToEulerAngles(obj.transform.rotation, out Vector3 euler);
            rotationControl.SetValues(euler);

            scaleControl.SetValues(obj.transform.scale);

            PopulateComponentTable(obj);
        }

        private void ClearComponentTable()
        {
            componentTable.Controls.Clear();
            componentTable.RowStyles.Clear();
            componentTable.RowCount = 0;
        }

        private void PopulateComponentTable(GameObject obj)
        {
            ClearComponentTable();

            foreach (Component comp in obj.Components)
            {
                int headerRow = componentTable.RowCount++;
                componentTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var header = new Label
                {
                    Text = comp.name,
                    Font = new Font(Font, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 8, 0, 4)
                };

                componentTable.Controls.Add(header, 0, headerRow);
                componentTable.SetColumnSpan(header, 2);

                foreach (var member in InspectorInfo.GetInspectableMembers(comp))
                {
                    if (member is FieldInfo fi)
                        DrawFieldRow(componentTable, member.Name, fi.GetValue(comp), v => fi.SetValue(comp, v));
                    else if (member is PropertyInfo pi && pi.CanRead && pi.CanWrite)
                        DrawFieldRow(componentTable, member.Name, pi.GetValue(comp), v => pi.SetValue(comp, v));
                }
            }
        }

        private void DrawFieldRow(
            TableLayoutPanel table,
            string label,
            object value,
            Action<object> setValue
        )
        {
            if (value is float f)
            {
                var tb = CreateTextBox();
                tb.Text = f.ToString();

                tb.TextChanged += (s, e) =>
                {
                    if (float.TryParse(tb.Text, out float v))
                        setValue(v);
                };

                AddRow(table, label, tb);
            }
        }
    }
}
