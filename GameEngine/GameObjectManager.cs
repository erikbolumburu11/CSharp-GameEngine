namespace GameEngine
{
    public class GameObjectManager
    {
        public List<GameObject> gameObjects;

        public event Action<GameObject> GameObjectAdded;
        public event Action<GameObject> GameObjectRemoved;
        public event Action<GameObject> GameObjectChanged;

        public GameObjectManager()
        {
            gameObjects = new();
        }

        public GameObject CreateCube()
        {
            GameObject cube = new("Cube");
            gameObjects.Add(cube);
            GameObjectAdded?.Invoke(cube);
            cube.Changed += OnObjectChanged;
            return cube;
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
