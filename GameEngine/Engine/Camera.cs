using GameEngine.Editor;
using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class Camera
    {
        int width;
        int height;
        public float AspectRatio => width / (float)height;

        public int Width => width;
        public int Height => height;

        public Vector3 position;


        float yaw = -MathHelper.PiOver2;
        float pitch;
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(yaw);
            set
            {
                yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        internal Vector3 front = -Vector3.UnitZ;
        internal Vector3 up = Vector3.UnitY;
        internal Vector3 right = Vector3.UnitX;
        public Vector3 Front => front;
        public Vector3 Up => up;
        public Vector3 Right => right;


        public Camera(Vector3 position, float speed, float sensitivity, int width, int height)
        {
            this.position = position;
            this.width = width;
            this.height = height;

        }

        public virtual void Update(InputHandler input, float deltaTime)
        {
        }


        internal void UpdateVectors()
        {
            front.X = MathF.Cos(pitch) * MathF.Cos(yaw);
            front.Y = MathF.Sin(pitch);
            front.Z = MathF.Cos(pitch) * MathF.Sin(yaw);
            front = Vector3.Normalize(front);

            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        public void SetAspectRatio(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(position, position + Front, Up);
        }

        public Matrix4 GetProjectionMatrix(int width, int height)
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), AspectRatio, 0.01f, 100.0f);
        }
    }
}
