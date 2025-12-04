namespace GameEngine
{
    public class InputHandler
    {
        private readonly Dictionary<Keys, bool> keyStates = new();
        private readonly Dictionary<MouseButtons, bool> mouseButtonStates = new();

        public event Action<Keys>? KeyPressed;
        public event Action<Keys>? KeyReleased;

        public event Action<MouseButtons>? MouseClicked;
        public event Action<MouseButtons>? MouseReleased;

        Point mousePos;
        public Point MousePos => mousePos;

        public void OnKeyDown(KeyEventArgs e)
        {
            if (!keyStates.ContainsKey(e.KeyCode) || !keyStates[e.KeyCode])
            {
                keyStates[e.KeyCode] = true;
                KeyPressed?.Invoke(e.KeyCode);
            }
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            keyStates[e.KeyCode] = false;
            KeyReleased?.Invoke(e.KeyCode);
        }

        public bool IsKeyDown(Keys key)
        {
            return keyStates.ContainsKey(key) && keyStates[key];
        }

        public void OnMouseClick(MouseEventArgs e)
        {
            if (!mouseButtonStates.ContainsKey(e.Button) || !mouseButtonStates[e.Button])
            {
                mouseButtonStates[e.Button] = true;
                MouseClicked?.Invoke(e.Button);
            }
        }

        public void OnMouseRelease(MouseEventArgs e)
        {
            mouseButtonStates[e.Button] = false;
            MouseReleased?.Invoke(e.Button);
        }

        public bool IsMouseButtonDown(MouseButtons button)
        {
            return mouseButtonStates.ContainsKey(button) && mouseButtonStates[button];
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            mousePos = e.Location;
        }
    }
}
