using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine
{
    public class ObjectHierarchy : DockContent
    {
        private ListBox listBox;

        public event Action<string>? GameObjectSelected;
        GameObjectManager gameObjectManager;

        public ObjectHierarchy(GameObjectManager gameObjectManager)
        {
            Text = "Object Hierarchy";
            this.gameObjectManager = gameObjectManager;

            listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
            };

            listBox.SelectedIndexChanged += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                    GameObjectSelected?.Invoke(listBox.SelectedItem.ToString());
            };

            Controls.Add(listBox);

            gameObjectManager.GameObjectAdded += GameObjectAdded;
            gameObjectManager.GameObjectRemoved += GameObjectRemoved;
        }

        private void GameObjectRemoved(GameObject @object)
        {
            throw new NotImplementedException();
        }

        private void GameObjectAdded(GameObject @object)
        {
            SetGameObjects(gameObjectManager.gameObjects.Select(gameObject => gameObject.Name));
        }

        public void SetGameObjects(IEnumerable<string> objects)
        {
            listBox.Items.Clear();
            foreach (var obj in objects)
                listBox.Items.Add(obj);
        }

        public void SelectObject(string name)
        {
            listBox.SelectedItem = name;
        }
    }
}
