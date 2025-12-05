namespace GameEngine
{
    public class GameObjectManager
    {
        public List<GameObject> gameObjects;

        public event Action<GameObject> GameObjectAdded;
        public event Action<GameObject> GameObjectRemoved;

        public GameObjectManager()
        {
            gameObjects = new();
        }

        public GameObject CreateCube()
        {
            GameObject cube = new("Cube");
            gameObjects.Add(cube);
            GameObjectAdded?.Invoke(cube);
            return cube;
        }
    }
}
