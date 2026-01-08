using GameEngine.Engine;
using OpenTK.Mathematics;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class ObjectHierarchy : DockContent
    {
        ListBox listBox;
        readonly Dictionary<GameObject, int> depthByObject = new();
        int dragSourceIndex = ListBox.NoMatches;
        Point dragStartPoint = Point.Empty;

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
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
            };

            listBox.SelectedIndexChanged += (s, e) =>
            {
                SelectObject();
            };

            listBox.MouseDown += ListBoxMouseDown;
            listBox.MouseMove += ListBoxMouseMove;
            listBox.DragEnter += ListBoxDragEnter;
            listBox.DragOver += ListBoxDragOver;
            listBox.DragDrop += ListBoxDragDrop;
            listBox.DrawItem += ListBoxDrawItem;

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
            SetGameObjects(gameObjectManager.gameObjects);
        }

        private void GameObjectRemoved(GameObject obj)
        {
            int removedIndex = listBox.Items.IndexOf(obj);
            bool wasSelected = listBox.SelectedIndex == removedIndex;
            SetGameObjects(gameObjectManager.gameObjects);

            if (wasSelected)
            {
                if (listBox.Items.Count == 0)
                {
                    editorState.Select(null);
                    return;
                }

                int fallbackIndex = removedIndex == ListBox.NoMatches ? 0 : removedIndex;
                int nextIndex = Math.Min(fallbackIndex, listBox.Items.Count - 1);
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
            var selectedObject = listBox.SelectedItem as GameObject;

            listBox.Items.Clear();
            depthByObject.Clear();

            foreach (GameObject obj in BuildHierarchyList(objects))
                listBox.Items.Add(obj);

            if (selectedObject != null && listBox.Items.Contains(selectedObject))
                listBox.SelectedItem = selectedObject;
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

        private void ListBoxMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = listBox.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    listBox.SelectedIndex = index;
                }
                else
                {
                    listBox.ClearSelected();
                }

                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                dragSourceIndex = listBox.IndexFromPoint(e.Location);
                dragStartPoint = e.Location;
            }
        }

        private void ListBoxMouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (dragSourceIndex == ListBox.NoMatches)
                return;

            Size dragSize = SystemInformation.DragSize;
            if (Math.Abs(e.X - dragStartPoint.X) < dragSize.Width &&
                Math.Abs(e.Y - dragStartPoint.Y) < dragSize.Height)
            {
                return;
            }

            if (listBox.Items[dragSourceIndex] is GameObject gameObject)
            {
                listBox.DoDragDrop(gameObject, DragDropEffects.Move);
            }

            dragSourceIndex = ListBox.NoMatches;
        }

        private void ListBoxDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(GameObject)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void ListBoxDragOver(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(GameObject)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            var dragged = (GameObject)e.Data.GetData(typeof(GameObject))!;
            Point clientPoint = listBox.PointToClient(new Point(e.X, e.Y));
            int targetIndex = listBox.IndexFromPoint(clientPoint);
            GameObject? target = targetIndex != ListBox.NoMatches
                ? listBox.Items[targetIndex] as GameObject
                : null;

            if (!CanSetParent(dragged, target))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Move;
        }

        private void ListBoxDragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(GameObject)))
                return;

            var dragged = (GameObject)e.Data.GetData(typeof(GameObject))!;
            Point clientPoint = listBox.PointToClient(new Point(e.X, e.Y));
            int targetIndex = listBox.IndexFromPoint(clientPoint);
            GameObject? target = targetIndex != ListBox.NoMatches
                ? listBox.Items[targetIndex] as GameObject
                : null;

            if (!CanSetParent(dragged, target))
                return;

            SetParent(dragged, target);
        }

        private void ListBoxDrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= listBox.Items.Count)
                return;

            e.DrawBackground();

            var gameObject = listBox.Items[e.Index] as GameObject;
            if (gameObject == null)
                return;

            int depth = depthByObject.TryGetValue(gameObject, out int value) ? value : 0;
            int indent = depth * 16;
            var textBounds = new Rectangle(
                e.Bounds.X + indent,
                e.Bounds.Y,
                e.Bounds.Width - indent,
                e.Bounds.Height);

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color textColor = selected ? SystemColors.HighlightText : listBox.ForeColor;

            TextRenderer.DrawText(
                e.Graphics,
                gameObject.name,
                e.Font,
                textBounds,
                textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            e.DrawFocusRectangle();
        }

        private bool CanSetParent(GameObject child, GameObject? newParent)
        {
            if (newParent == null)
                return true;

            if (child == newParent)
                return false;

            Transform? walker = newParent.transform;
            while (walker != null)
            {
                if (walker == child.transform)
                    return false;

                walker = walker.parent;
            }

            return true;
        }

        private void SetParent(GameObject child, GameObject? newParent)
        {
            Transform? newParentTransform = newParent?.transform;
            if (child.transform.parent == newParentTransform)
                return;

            Vector3 worldPosition = child.transform.WorldPosition;
            Quaternion worldRotation = child.transform.WorldRotation;
            Vector3 worldScale = child.transform.WorldScale;

            child.transform.parent = newParentTransform;

            child.transform.WorldPosition = worldPosition;
            child.transform.WorldRotation = worldRotation;
            child.transform.WorldScale = worldScale;

            gameObjectManager.OnObjectChanged(child);
            SetGameObjects(gameObjectManager.gameObjects);
            listBox.SelectedItem = child;
        }

        private List<GameObject> BuildHierarchyList(IEnumerable<GameObject> objects)
        {
            var result = new List<GameObject>();
            var allObjects = objects.ToList();
            var objectSet = new HashSet<GameObject>(allObjects);
            var rootObjects = new List<GameObject>();
            var byParent = new Dictionary<GameObject, List<GameObject>>();

            foreach (var obj in allObjects)
            {
                GameObject? parent = obj.transform.parent?.GameObject;
                if (parent != null && !objectSet.Contains(parent))
                    parent = null;

                if (parent == null)
                {
                    rootObjects.Add(obj);
                    continue;
                }

                if (!byParent.TryGetValue(parent, out var children))
                {
                    children = new List<GameObject>();
                    byParent[parent] = children;
                }

                children.Add(obj);
            }

            var visited = new HashSet<GameObject>();

            void AddChildren(GameObject parent, int depth)
            {
                if (!byParent.TryGetValue(parent, out var children))
                    return;

                foreach (var child in children)
                {
                    if (!visited.Add(child))
                        continue;

                    depthByObject[child] = depth;
                    result.Add(child);
                    AddChildren(child, depth + 1);
                }
            }

            foreach (var root in rootObjects)
            {
                if (!visited.Add(root))
                    continue;

                depthByObject[root] = 0;
                result.Add(root);
                AddChildren(root, 1);
            }

            foreach (var obj in allObjects)
            {
                if (visited.Add(obj))
                {
                    depthByObject[obj] = 0;
                    result.Add(obj);
                }
            }

            return result;
        }
    }
}
