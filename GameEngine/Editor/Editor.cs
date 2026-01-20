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
        Stopwatch timer;

        InputHandler inputHandler;
        EditorCamera camera;

        EditorState editorState;

        SceneView sceneView;
        ObjectHierarchy objectHierarchy;
        Inspector inspector;
        MaterialEditor materialEditor;
        ModelBrowser modelBrowser;

        public Editor()
        {
            InitializeComponent();

            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1280, 720); 

            dockPanel.Theme = new VS2012DarkTheme();
            dockPanel.DocumentTabStripLocation = DocumentTabStripLocation.Top;

            sceneView = new SceneView();
            sceneView.Show(dockPanel, DockState.Document);

            inputHandler = new();
            camera = new EditorCamera(new Vector3(0, 0, 6), 0.5f, 1.5f, inputHandler, sceneView.Width, sceneView.Height);
            editorState = new();

            objectHierarchy = new ObjectHierarchy(editorState.engineHost.gameObjectManager, editorState);
            objectHierarchy.Show(dockPanel, DockState.DockLeft);

            inspector = new Inspector(editorState, editorState.engineHost.gameObjectManager);
            inspector.Show(dockPanel, DockState.DockRight);

            materialEditor = new MaterialEditor(editorState);
            materialEditor.Show(dockPanel, DockState.DockRight);

            modelBrowser = new ModelBrowser(editorState);
            modelBrowser.Show(dockPanel, DockState.DockRight);

            inspector.Activate();

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

                editorState.engineHost.Update(dt);
                camera.Update(inputHandler, dt);
                editorState.engineHost.Render(camera);
                sceneView.glControl.SwapBuffers();
            }
        }

        private void glControl_Load(object? sender, EventArgs e)
        {
            sceneView.glControl.MakeCurrent();
            sceneView.glControl.Context.SwapInterval = 1;
            editorState.engineHost.InitializeGL();
        }

        private void glControl_Paint(object? sender, PaintEventArgs e)
        {
            sceneView.glControl.MakeCurrent();

            float dt = (float)timer.Elapsed.TotalSeconds;
            timer.Restart();

            editorState.engineHost.Update(dt);
            camera.Update(inputHandler, dt);
            editorState.engineHost.Render(camera);

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
            if (!ProjectContext.HasProject)
            {
                var created = ProjectDialogs.CreateProjectWithDialog(this);
                if (created == null)
                    return;
            }

            string? relPath = string.IsNullOrWhiteSpace(editorState.engineHost.game.scene.relPath)
                ? ProjectContext.Current?.StartSceneRelative
                : editorState.engineHost.game.scene.relPath;

            if (string.IsNullOrWhiteSpace(relPath))
            {
                MessageBox.Show(
                    this,
                    "No scene path is set. Set a start scene in project settings before saving.",
                    "Save Scene",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            SceneSerializer.SaveScene
            (
                editorState.engineHost.gameObjectManager,
                editorState.engineHost.game.scene,
                relPath,
                true
            );

            editorState.engineHost.game.scene.relPath = relPath;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var project = ProjectDialogs.OpenProjectWithDialog(editorState, this);
            if (project != null)
                materialEditor?.RefreshMaterialListFromEditor();
            if (project != null)
                modelBrowser?.RefreshModelListFromEditor();
        }

        private void sceneSettingsButton_Click(object sender, EventArgs e)
        {
            SceneSettings sceneSettings = new(editorState.engineHost.game.scene);
            sceneSettings.Show();
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var project = ProjectDialogs.CreateProjectWithDialog(this);
            if (project != null)
                materialEditor?.RefreshMaterialListFromEditor();
            if (project != null)
                modelBrowser?.RefreshModelListFromEditor();
        }
    }
}
