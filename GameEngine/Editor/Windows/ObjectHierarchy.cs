using GameEngine.Engine;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class ObjectHierarchy : DockContent
    {
        ListBox listBox;

        public event Action<string>? GameObjectSelected;
        GameObjectManager gameObjectManager;
        EditorState editorState;

        public ObjectHierarchy(GameObjectManager gameObjectManager, EditorState editorState)
        {
            Text = "Object Hierarchy";
            this.gameObjectManager = gameObjectManager;
            this.editorState = editorState;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(layout);

            Button newCubeButton = new Button { Width = 150 };
            newCubeButton.Text = "New Cube";

            newCubeButton.Click += (s, e) =>
            {
                gameObjectManager.CreateCube();
            };

            Button newGameObjectButton = new Button { Width = 150 };
            newGameObjectButton.Text = "New GameObject";

            newGameObjectButton.Click += (s, e) =>
            {
                gameObjectManager.CreateGameObject();
            };

            listBox = new ListBox
            {
                Dock = DockStyle.Fill,
            };

            listBox.SelectedIndexChanged += (s, e) =>
            {
                SelectObject();
            };

            listBox.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Right)
                    return;

                int index = listBox.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    listBox.SelectedIndex = index;
                }
                else
                {
                    listBox.ClearSelected();
                }
            };

            ContextMenuStrip objectMenu = new ContextMenuStrip();
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Delete", null, (s, e) =>
            {
                DeleteSelectedObject();
            });
            objectMenu.Items.Add(deleteItem);
            objectMenu.Opening += (s, e) =>
            {
                if (!(listBox.SelectedItem is GameObject))
                    e.Cancel = true;
            };
            listBox.ContextMenuStrip = objectMenu;

            layout.Controls.Add(newCubeButton);
            layout.Controls.Add(newGameObjectButton);
            layout.Controls.Add(listBox);

            gameObjectManager.GameObjectAdded += GameObjectAdded;
            gameObjectManager.GameObjectRemoved += GameObjectRemoved;
            gameObjectManager.GameObjectChanged += GameObjectChanged;
        }

        public void RefreshList()
        {
            listBox.Items.Clear();
            foreach (GameObject obj in gameObjectManager.gameObjects)
                listBox.Items.Add(obj);
        }

        private void GameObjectRemoved(GameObject obj)
        {
            int removedIndex = listBox.Items.IndexOf(obj);
            if (removedIndex == ListBox.NoMatches)
            {
                SetGameObjects(gameObjectManager.gameObjects);
                return;
            }

            bool wasSelected = listBox.SelectedIndex == removedIndex;
            listBox.Items.RemoveAt(removedIndex);

            if (wasSelected)
            {
                if (listBox.Items.Count == 0)
                {
                    editorState.Select(null);
                    return;
                }

                int nextIndex = Math.Min(removedIndex, listBox.Items.Count - 1);
                listBox.SelectedIndex = nextIndex;
            }
        }

        private void GameObjectAdded(GameObject obj)
        {
            SetGameObjects(gameObjectManager.gameObjects);
        }

        private void GameObjectChanged(GameObject obj)
        {
            int index = listBox.Items.IndexOf(obj);
            if (index >= 0)
            {
                listBox.Items[index] = obj;
                listBox.Refresh();
            }
        }

        public void SetGameObjects(IEnumerable<GameObject> objects)
        {
            listBox.Items.Clear();
            foreach (GameObject obj in objects)
                listBox.Items.Add(obj);
        }

        public void SelectObject()
        {
            if (listBox.SelectedItem is GameObject gameObject)
            {
                editorState.Select(gameObject);
            }
        }

        private void DeleteSelectedObject()
        {
            if (listBox.SelectedItem is GameObject gameObject)
            {
                gameObjectManager.RemoveGameObject(gameObject);
            }
        }
    }
}
