using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameEngine
{
    public partial class Editor : Form
    {

        private Game game;
        private Stopwatch timer;

        public Editor()
        {
            InitializeComponent();

            game = new Game(glControl1.Width, glControl1.Height);

            timer = new Stopwatch();
            timer.Start();

            glControl1.Load += GlControl1_Load;
            glControl1.Paint += GlControl1_Paint;
            glControl1.Resize += GlControl1_Resize;
            Application.Idle += Application_Idle;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [DllImport("user32.dll")]
        static extern bool PeekMessage(
           out NativeMessage msg,
           IntPtr hWnd,
           uint wMsgFilterMin,
           uint wMsgFilterMax,
           uint wRemoveMsg
       );

        private void Application_Idle(object? sender, EventArgs e)
        {
            NativeMessage msg;
            while (!PeekMessage(out msg, IntPtr.Zero, 0, 0, 0))
            {
                float dt = (float)timer.Elapsed.TotalSeconds;
                timer.Restart();

                game.Update(dt);
                game.Render();
                glControl1.SwapBuffers();
            }
        }

        private void GlControl1_Load(object? sender, EventArgs e)
        {
            glControl1.MakeCurrent();
            game.Initialize();
        }

        private void GlControl1_Paint(object? sender, PaintEventArgs e)
        {
            glControl1.MakeCurrent();

            float dt = (float)timer.Elapsed.TotalSeconds;
            timer.Restart();

            game.Update(dt);
            game.Render();

            glControl1.SwapBuffers();
        }

        private void GlControl1_Resize(object? sender, EventArgs e)
        {
            glControl1.MakeCurrent();
            game.Resize(glControl1.Width, glControl1.Height);
        }

        private void Editor_Load(object sender, EventArgs e)
        {

        }
    }
}