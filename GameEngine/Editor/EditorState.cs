using GameEngine.Engine;

namespace GameEngine.Editor
{
    public class EditorState
    {
        public GameObject? SelectedObject { get; private set; }
        public event Action<GameObject?> OnSelectionChanged;

        public EngineHost engineHost;

        public EditorState()
        {
            engineHost = new EngineHost();
        }

        public void Select(GameObject? obj)
        {
            if (SelectedObject == obj) return;
            SelectedObject = obj;
            OnSelectionChanged?.Invoke(obj);
        }
    }
}
