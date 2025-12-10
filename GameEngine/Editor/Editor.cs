using GameEngine.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WeifenLuo.WinFormsUI.Docking;

namespace GameEngine.Editor
{
    public partial class Editor : Form
    {

        Game game;
        Stopwatch timer;

        InputHandler inputHandler;
        EditorCamera camera;

        EditorState editorState;


        SceneView sceneView;
        ObjectHierarchy objectHierarchy;
        Inspector inspector;

        public Editor()
        {
            InitializeComponent();

            dockPanel.Theme = new VS2012DarkTheme();

            sceneView = new SceneView();
            sceneView.Show(dockPanel, DockState.Document);

            game = new Game();

            inputHandler = new();
            camera = new EditorCamera(new Vector3(0, 0, 6), 0.5f, 1.5f, inputHandler, sceneView.Width, sceneView.Height);
            editorState = new();

            objectHierarchy = new ObjectHierarchy(game.gameObjectManager, editorState);
            objectHierarchy.Show(dockPanel, DockState.DockLeft);

            inspector = new Inspector(editorState, game.gameObjectManager);
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

            sceneView.glControl.KeyDown += (s, e) => inputHandler.OnKeyDown(e);
            sceneView.glControl.KeyUp += (s, e) => inputHandler.OnKeyUp(e);

            sceneView.glControl.MouseDown += (s, e) => inputHandler.OnMouseClick(e);
            sceneView.glControl.MouseUp += (s, e) => inputHandler.OnMouseRelease(e);
            sceneView.glControl.MouseMove += (s, e) => inputHandler.OnMouseMove(e);
        }


        private void Application_Idle(object? sender, EventArgs e)
        {
            NativeMessage msg;
            while (!PeekMessage(out msg, IntPtr.Zero, 0, 0, 0))
            {
                float dt = (float)timer.Elapsed.TotalSeconds;
                timer.Restart();

                game.Update(dt);
                camera.Update(inputHandler, dt);
                game.Render(camera);
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
            camera.Update(inputHandler, dt);
            game.Render(camera);

            sceneView.glControl.SwapBuffers();
        }

        private void glControl_Resize(object? sender, EventArgs e)
        {
            sceneView.glControl.MakeCurrent();
            GL.Viewport(0, 0, sceneView.Width, sceneView.Height);
            camera.SetAspectRatio(sceneView.Width, sceneView.Height);
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

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog();
            dialog.Filter = "Scene Files (*.scene)|*.scene";
            if (dialog.ShowDialog() == DialogResult.OK)
                SceneSerializer.SaveScene(game.gameObjectManager, dialog.FileName);
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog();
            dialog.Filter = "Scene Files (*.scene)|*.scene";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SceneSerializer.LoadScene(game.gameObjectManager, dialog.FileName);
                objectHierarchy.RefreshList();
                sceneView.Refresh();
            }
        }
    }
}