using GameEngine.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                RowCount = 2,
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));         
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));    
            Controls.Add(layout);
      
            Button newCubeButton = new Button();
            newCubeButton.Text = "New Cube";

            newCubeButton.Click += (s, e) =>
            {
                gameObjectManager.CreateCube();
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
            layout.Controls.Add(listBox);

            gameObjectManager.GameObjectAdded += GameObjectAdded;
            gameObjectManager.GameObjectRemoved += GameObjectRemoved;
            gameObjectManager.GameObjectChanged += GameObjectChanged;
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
                listBox.Items[index] = listBox.Items[index]; 
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
            if(listBox.SelectedItem is GameObject gameObject)
            {
                editorState.Select(gameObject);
            }
        }
    }
}
