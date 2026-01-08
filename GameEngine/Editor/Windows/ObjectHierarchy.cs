using GameEngine.Engine;
using OpenTK.Mathematics;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public class ObjectHierarchy : DockContent
    {
        TreeView treeView;
        readonly Dictionary<GameObject, TreeNode> nodesByObject = new();

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

            treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                HideSelection = false,
            };

            treeView.AfterSelect += (s, e) =>
            {
                SelectObject();
            };
            treeView.NodeMouseClick += TreeViewNodeMouseClick;
            treeView.ItemDrag += TreeViewItemDrag;
            treeView.DragEnter += TreeViewDragEnter;
            treeView.DragOver += TreeViewDragOver;
            treeView.DragDrop += TreeViewDragDrop;

            ContextMenuStrip objectMenu = new ContextMenuStrip();
            ToolStripMenuItem moveUpItem = new ToolStripMenuItem("Move Up", null, (s, e) =>
            {
                MoveSelectedObject(-1);
            });
            ToolStripMenuItem moveDownItem = new ToolStripMenuItem("Move Down", null, (s, e) =>
            {
                MoveSelectedObject(1);
            });
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Delete", null, (s, e) =>
            {
                DeleteSelectedObject();
            });
            objectMenu.Items.Add(moveUpItem);
            objectMenu.Items.Add(moveDownItem);
            objectMenu.Items.Add(new ToolStripSeparator());
            objectMenu.Items.Add(deleteItem);
            objectMenu.Opening += (s, e) =>
            {
                if (!(treeView.SelectedNode?.Tag is GameObject selected))
                {
                    e.Cancel = true;
                    return;
                }

                int siblingCount;
                int siblingIndex = GetSiblingIndex(selected, out siblingCount);
                moveUpItem.Enabled = siblingIndex > 0;
                moveDownItem.Enabled = siblingIndex >= 0 && siblingIndex < siblingCount - 1;
            };
            treeView.ContextMenuStrip = objectMenu;

            layout.Controls.Add(newCubeButton);
            layout.Controls.Add(newGameObjectButton);
            layout.Controls.Add(treeView);

            gameObjectManager.GameObjectAdded += GameObjectAdded;
            gameObjectManager.GameObjectRemoved += GameObjectRemoved;
            gameObjectManager.GameObjectChanged += GameObjectChanged;
            gameObjectManager.GameObjectHierarchyChanged += GameObjectHierarchyChanged;
        }

        public void RefreshList()
        {
            SetGameObjects(gameObjectManager.gameObjects);
        }

        private void GameObjectRemoved(GameObject obj)
        {
            bool wasSelected = treeView.SelectedNode?.Tag == obj;
            SetGameObjects(gameObjectManager.gameObjects);

            if (!wasSelected)
                return;

            if (treeView.Nodes.Count == 0)
            {
                editorState.Select(null);
                return;
            }

            treeView.SelectedNode = treeView.Nodes[0];
        }

        private void GameObjectAdded(GameObject obj)
        {
            SetGameObjects(gameObjectManager.gameObjects);
        }

        private void GameObjectChanged(GameObject obj)
        {
            if (nodesByObject.TryGetValue(obj, out var node))
            {
                node.Text = obj.name;
            }
        }

        private void GameObjectHierarchyChanged(GameObject obj)
        {
            SetGameObjects(gameObjectManager.gameObjects);
        }

        public void SetGameObjects(IEnumerable<GameObject> objects)
        {
            var selectedObject = treeView.SelectedNode?.Tag as GameObject;

            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            nodesByObject.Clear();

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

            TreeNode MakeNode(GameObject obj)
            {
                var node = new TreeNode(obj.name) { Tag = obj };
                nodesByObject[obj] = node;
                visited.Add(obj);
                return node;
            }

            void AddChildren(GameObject parent, TreeNode parentNode)
            {
                if (!byParent.TryGetValue(parent, out var children))
                    return;

                foreach (var child in children)
                {
                    if (visited.Contains(child))
                        continue;

                    var childNode = MakeNode(child);
                    parentNode.Nodes.Add(childNode);
                    AddChildren(child, childNode);
                }
            }

            foreach (var root in rootObjects)
            {
                if (visited.Contains(root))
                    continue;

                var rootNode = MakeNode(root);
                treeView.Nodes.Add(rootNode);
                AddChildren(root, rootNode);
            }

            foreach (var obj in allObjects)
            {
                if (visited.Contains(obj))
                    continue;

                var node = MakeNode(obj);
                treeView.Nodes.Add(node);
                AddChildren(obj, node);
            }

            treeView.ExpandAll();

            if (selectedObject != null && nodesByObject.TryGetValue(selectedObject, out var selectedNode))
            {
                treeView.SelectedNode = selectedNode;
                selectedNode.EnsureVisible();
            }

            treeView.EndUpdate();
        }

        public void SelectObject()
        {
            if (treeView.SelectedNode?.Tag is GameObject gameObject)
            {
                editorState.Select(gameObject);
            }
        }

        private void DeleteSelectedObject()
        {
            if (treeView.SelectedNode?.Tag is GameObject gameObject)
            {
                gameObjectManager.RemoveGameObject(gameObject);
            }
        }

        private void MoveSelectedObject(int direction)
        {
            if (!(treeView.SelectedNode?.Tag is GameObject selected))
                return;

            if (!TryGetSibling(selected, direction, out var sibling))
                return;

            if (direction < 0)
                gameObjectManager.MoveGameObjectBefore(selected, sibling);
            else
                gameObjectManager.MoveGameObjectAfter(selected, sibling);
        }

        private int GetSiblingIndex(GameObject obj, out int siblingCount)
        {
            var siblings = GetSiblings(obj);

            siblingCount = siblings.Count;
            return siblings.IndexOf(obj);
        }

        private bool TryGetSibling(GameObject obj, int direction, out GameObject sibling)
        {
            sibling = null!;
            var siblings = GetSiblings(obj);
            int index = siblings.IndexOf(obj);
            int targetIndex = index + direction;
            if (index < 0 || targetIndex < 0 || targetIndex >= siblings.Count)
                return false;

            sibling = siblings[targetIndex];
            return true;
        }

        private List<GameObject> GetSiblings(GameObject obj)
        {
            GameObject? parent = obj.transform.parent?.GameObject;
            return gameObjectManager.gameObjects
                .Where(go => go.transform.parent?.GameObject == parent)
                .ToList();
        }

        private void TreeViewNodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            treeView.SelectedNode = e.Node;
        }

        private void TreeViewItemDrag(object? sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode node && node.Tag is GameObject gameObject)
            {
                treeView.DoDragDrop(gameObject, DragDropEffects.Move);
            }
        }

        private void TreeViewDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(GameObject)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TreeViewDragOver(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(GameObject)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            var dragged = (GameObject)e.Data.GetData(typeof(GameObject))!;
            Point clientPoint = treeView.PointToClient(new Point(e.X, e.Y));
            TreeNode? targetNode = treeView.GetNodeAt(clientPoint);
            GameObject? target = targetNode?.Tag as GameObject;

            if (!CanSetParent(dragged, target))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            treeView.SelectedNode = targetNode;
            e.Effect = DragDropEffects.Move;
        }

        private void TreeViewDragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(GameObject)))
                return;

            var dragged = (GameObject)e.Data.GetData(typeof(GameObject))!;
            Point clientPoint = treeView.PointToClient(new Point(e.X, e.Y));
            TreeNode? targetNode = treeView.GetNodeAt(clientPoint);
            GameObject? target = targetNode?.Tag as GameObject;

            if (!CanSetParent(dragged, target))
                return;

            SetParent(dragged, target);

            if (targetNode != null)
                targetNode.Expand();
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
        }
    }
}
