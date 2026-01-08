using GameEngine.Engine.Components;

namespace GameEngine.Engine
{
    public class GameObjectManager
    {
        public List<GameObject> gameObjects;

        public event Action<GameObject>? GameObjectAdded;
        public event Action<GameObject>? GameObjectRemoved;
        public event Action<GameObject>? GameObjectChanged;
        public event Action<GameObject>? GameObjectHierarchyChanged;

        public GameObjectManager()
        {
            gameObjects = new();
        }

        public GameObject CreateGameObject()
        {
            return CreateGameObject("New GameObject");
        }

        public GameObject CreateGameObject(string name)
        {
            GameObject gameObject = new GameObject(name);
            gameObjects.Add(gameObject);
            GameObjectAdded?.Invoke(gameObject);
            gameObject.Changed += OnObjectChanged;
            gameObject.HierarchyChanged += OnHierarchyChanged;
            return gameObject;
        }

        public GameObject CreateGameObject(string name, Guid id)
        {
            GameObject gameObject = new GameObject(name, id);
            gameObjects.Add(gameObject);
            GameObjectAdded?.Invoke(gameObject);
            gameObject.Changed += OnObjectChanged;
            gameObject.HierarchyChanged += OnHierarchyChanged;
            return gameObject;
        }

        public GameObject CreateCube()
        {
            GameObject cube = CreateGameObject("Cube");
            cube.AddComponent<MeshRenderer>();
            return cube;
        }

        public List<T> GetAllComponents<T>() where T : Component
        {
            List<T> results = new List<T>();

            foreach (var go in gameObjects)
            {
                var component = go.GetComponent<T>();
                if (component != null)
                    results.Add(component);
            }

            return results;
        }

        public void Clear()
        {
            gameObjects = new();
        }

        public void RenameGameObject(GameObject gameObject, string newName)
        {
            gameObject.SetName(newName);
            GameObjectChanged?.Invoke(gameObject);
        }

        public bool RemoveGameObject(GameObject gameObject)
        {
            if (!gameObjects.Remove(gameObject))
                return false;

            gameObject.Changed -= OnObjectChanged;
            gameObject.HierarchyChanged -= OnHierarchyChanged;
            GameObjectRemoved?.Invoke(gameObject);
            return true;
        }

        public void OnObjectChanged(GameObject gameObject)
        {
            GameObjectChanged?.Invoke(gameObject);
        }

        public void OnHierarchyChanged(GameObject gameObject)
        {
            GameObjectHierarchyChanged?.Invoke(gameObject);
        }

        public bool MoveGameObjectBefore(GameObject gameObject, GameObject before)
        {
            if (gameObject == before)
                return false;

            int oldIndex = gameObjects.IndexOf(gameObject);
            int beforeIndex = gameObjects.IndexOf(before);
            if (oldIndex < 0 || beforeIndex < 0)
                return false;

            gameObjects.RemoveAt(oldIndex);
            if (oldIndex < beforeIndex)
                beforeIndex--;

            gameObjects.Insert(beforeIndex, gameObject);
            OnHierarchyChanged(gameObject);
            return true;
        }

        public bool MoveGameObjectAfter(GameObject gameObject, GameObject after)
        {
            if (gameObject == after)
                return false;

            int oldIndex = gameObjects.IndexOf(gameObject);
            int afterIndex = gameObjects.IndexOf(after);
            if (oldIndex < 0 || afterIndex < 0)
                return false;

            gameObjects.RemoveAt(oldIndex);
            if (oldIndex < afterIndex)
                afterIndex--;

            gameObjects.Insert(afterIndex + 1, gameObject);
            OnHierarchyChanged(gameObject);
            return true;
        }

        public GameObject? TryGetFromGuid(Guid id)
        {
            if(id == Guid.Empty) return null;
            return gameObjects.FirstOrDefault(go => go.Id == id, null);
        }
    }
}
