using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class EditorState
    {
        public GameObject SelectedObject { get; private set; }
        public event Action<GameObject> OnSelectionChanged;

        public void Select(GameObject obj)
        {
            if (SelectedObject == obj) return;
            SelectedObject = obj;
            OnSelectionChanged?.Invoke(obj);
        }
    }
}
