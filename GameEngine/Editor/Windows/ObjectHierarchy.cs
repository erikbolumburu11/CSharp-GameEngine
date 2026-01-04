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
            throw new NotImplementedException();
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
    }
}
