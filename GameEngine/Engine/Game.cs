using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;

namespace GameEngine.Engine
{
    public class Game
    {
        Stopwatch time;

        public Scene scene;

        public Game()
        {
            time = new Stopwatch();
            time.Start();


            scene = new Scene();
        }
    }
}
