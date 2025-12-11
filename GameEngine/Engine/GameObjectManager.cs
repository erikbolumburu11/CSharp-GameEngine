using GameEngine.Engine.Components;

namespace GameEngine.Engine
{
    public class GameObjectManager
    {
        public List<GameObject> gameObjects;

        public event Action<GameObject>? GameObjectAdded;
        public event Action<GameObject>? GameObjectRemoved;
        public event Action<GameObject>? GameObjectChanged;

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

        public void OnObjectChanged(GameObject gameObject)
        {
            GameObjectChanged?.Invoke(gameObject);
        }
    }
}
