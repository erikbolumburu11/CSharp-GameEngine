using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;

namespace GameEngine.Engine
{
    public class Game
    {
        public GameObjectManager gameObjectManager { get; private set; }

        Stopwatch time;

        public Scene scene;

        public Game()
        {
            time = new Stopwatch();
            time.Start();

            gameObjectManager = new GameObjectManager();

            scene = new Scene();
        }

    }
}
