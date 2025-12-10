using GameEngine.Engine;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class Inspector : DockContent
    {
        private readonly EditorState editorState;
        private readonly GameObjectManager gameObjectManager;

        FlowLayoutPanel layout;

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
            // Main vertical layout
            layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
            };
            Controls.Add(layout);

            // NAME FIELD
            nameTextBox = new TextBox { Width = 150 };
            Control nameRow = CreateLabeledRow("Name:", nameTextBox);
            layout.Controls.Add(nameRow);

            // POSITION FIELD
            positionControl = new Vector3Control();
            Control posRow = CreateLabeledRow("Position:", positionControl);
            layout.Controls.Add(posRow);

            // ROTATION FIELD
            rotationControl = new Vector3Control();
            Control rotRow = CreateLabeledRow("Rotation:", rotationControl);
            layout.Controls.Add(rotRow);

            // SCALE FIELD
            scaleControl = new Vector3Control();
            Control scaleRow = CreateLabeledRow("Scale:", scaleControl);
            layout.Controls.Add(scaleRow);

            //TODO: Components and Fields

            layout.Controls.Add(CreateAddComponentButton());

            layout.Hide();
        }

        private Button CreateAddComponentButton()
        {
            ContextMenuStrip cms = new ContextMenuStrip();

            foreach (Type componentType in ComponentTypeRegistry.Types.Values)
            {
                if (Equals(typeof(Component), componentType)) continue;

                ToolStripMenuItem componentMenuItem = new ToolStripMenuItem
                (
                    componentType.Name,
                    null,
                    (s, e) =>
                    {
                        editorState.SelectedObject.AddComponent(componentType);
                    },
                    componentType.Name
               );

                cms.Items.Add(componentMenuItem);
            }

            Button button = new Button
            {
                Text = "Add Component",
                Width = 150
            };

            button.Click += (s, e) =>
            {
                cms.Show(button.Location);
            };

            return button;
        }

        private Control CreateLabeledRow(string labelText, Control input)
        {
            var panel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            panel.Controls.Add(new Label { Text = labelText, AutoSize = true });
            panel.Controls.Add(input);

            return panel;
        }

        private void HookEvents()
        {
            nameTextBox.TextChanged += (s, e) =>
            {
                var obj = editorState.SelectedObject;
                if (obj == null) return;

                gameObjectManager.RenameGameObject(obj, nameTextBox.Text);
            };

            positionControl.ValueChanged += (vector) =>
            {
                var obj = editorState.SelectedObject;
                if (obj == null) return;

                obj.SetPosition(vector);
            };

            rotationControl.ValueChanged += (vector) =>
            {
                var obj = editorState.SelectedObject;
                if (obj == null) return;

                obj.SetRotation(vector);
            };

            scaleControl.ValueChanged += (vector) =>
            {
                var obj = editorState.SelectedObject;
                if (obj == null) return;

                obj.SetScale(vector);
            };

            editorState.OnSelectionChanged += UpdateInspectorFields;
        }

        private void UpdateInspectorFields(GameObject obj)
        {
            if (obj == null)
            {
                layout.Hide();

                nameTextBox.Text = "";

                positionControl.SetValues(new Vector3());
                rotationControl.SetValues(new Vector3());
                scaleControl.SetValues(new Vector3());

                return;
            }

            layout.Show();

            nameTextBox.Text = obj.name;

            positionControl.SetValues(obj.transform.position);

            Vector3 euler;
            Quaternion.ToEulerAngles(obj.transform.rotation, out euler);
            rotationControl.SetValues(euler);

            scaleControl.SetValues(obj.transform.scale);
        }
    }
}
