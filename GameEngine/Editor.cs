using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine
{
    public partial class Editor : Form
    {

        Game game;
        Stopwatch timer;

        EditorState editorState;

        DockPanel dockPanel;

        SceneView sceneView;
        ObjectHierarchy objectHierarchy;
        Inspector inspector;

        public Editor()
        {
            InitializeComponent();

            editorState = new();

            dockPanel = new DockPanel {
                Dock = DockStyle.Fill,
                DocumentStyle = DocumentStyle.DockingWindow
            };
            dockPanel.Theme = new VS2012DarkTheme(); 
            Controls.Add(dockPanel);

            sceneView = new SceneView();
            sceneView.Show(dockPanel, DockState.Document);

            game = new Game(sceneView.glControl);

            objectHierarchy = new ObjectHierarchy(game.GameObjectManager, editorState);
            objectHierarchy.Show(dockPanel, DockState.DockLeft);

            inspector = new Inspector(editorState, game.GameObjectManager);
            inspector.Show(dockPanel, DockState.DockRight);


            timer = new Stopwatch();
            timer.Start();

            sceneView.glControl.TabStop = true;

            Load += (s, e) => sceneView.glControl.Focus();

            sceneView.glControl.GotFocus += (s, e) => Console.WriteLine("GLControl focused");
            sceneView.glControl.LostFocus += (s, e) => Console.WriteLine("GLControl lost focus");

            sceneView.glControl.Load += glControl_Load;
            sceneView.glControl.Paint += glControl_Paint;
            sceneView.glControl.Resize += glControl_Resize;
            Application.Idle += Application_Idle;

            sceneView.glControl.KeyDown += (s, e) => game.InputHandler.OnKeyDown(e);
            sceneView.glControl.KeyUp += (s, e) => game.InputHandler.OnKeyUp(e);

            sceneView.glControl.MouseDown += (s, e) => game.InputHandler.OnMouseClick(e);
            sceneView.glControl.MouseUp += (s, e) => game.InputHandler.OnMouseRelease(e);
            sceneView.glControl.MouseMove += (s, e) => game.InputHandler.OnMouseMove(e);
        }


        private void Application_Idle(object? sender, EventArgs e)
        {
            NativeMessage msg;
            while (!PeekMessage(out msg, IntPtr.Zero, 0, 0, 0))
            {
                float dt = (float)timer.Elapsed.TotalSeconds;
                timer.Restart();

                game.Update(dt);
                game.Render();
                sceneView.glControl.SwapBuffers();
            }
        }

        private void glControl_Load(object? sender, EventArgs e)
        {
            sceneView.glControl.MakeCurrent();
            game.Initialize();
        }

        private void glControl_Paint(object? sender, PaintEventArgs e)
        {
            sceneView.glControl.MakeCurrent();

            float dt = (float)timer.Elapsed.TotalSeconds;
            timer.Restart();

            game.Update(dt);
            game.Render();

            sceneView.glControl.SwapBuffers();
        }

        private void glControl_Resize(object? sender, EventArgs e)
        {
            sceneView.glControl.MakeCurrent();
            game.Resize(sceneView.glControl.Width, sceneView.glControl.Height);
        }

        private void glControl_Click(object sender, EventArgs e)
        {
            sceneView.glControl.Focus();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }

        [DllImport("user32.dll")]
        static extern bool PeekMessage(
           out NativeMessage msg,
           IntPtr hWnd,
           uint wMsgFilterMin,
           uint wMsgFilterMax,
           uint wRemoveMsg
       );

    }
}