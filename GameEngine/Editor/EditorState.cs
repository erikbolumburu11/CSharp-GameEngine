using GameEngine.Engine;

namespace GameEngine.Editor
{
    public class EditorState
    {
        public GameObject SelectedObject { get; private set; }
        public event Action<GameObject> OnSelectionChanged;

        public Game game { get; private set; }

        public EditorState()
        {
            game = new Game();
        }

        public void Select(GameObject obj)
        {
            if (SelectedObject == obj) return;
            SelectedObject = obj;
            OnSelectionChanged?.Invoke(obj);
        }
    }
}
