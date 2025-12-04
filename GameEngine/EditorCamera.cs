using OpenTK.Mathematics;
using System;

namespace GameEngine
{
    public class EditorCamera
    {
        int width;
        int height;
        public int Width => width;
        public int Height => height;

        public Vector3 position;
        public float speed;

        public float sensitivity;

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

        Vector3 front = -Vector3.UnitZ;
        Vector3 up = Vector3.UnitY;
        Vector3 right = Vector3.UnitX;

        public Vector3 Front => front;
        public Vector3 Up => up;

        Vector2 lastMousePos;
        bool firstMouseMove = true;

        public EditorCamera(Vector3 position, float speed, float sensitivity, int width, int height, InputHandler input)
        {
            this.position = position;
            this.speed = speed;
            this.sensitivity = sensitivity;

            this.width = width;
            this.height = height;

            input.MouseReleased += (e) =>
            {
                if (e == MouseButtons.Right)
                {
                    firstMouseMove = true;
                }
            };
        }

        public void Update(InputHandler input, float deltaTime)
        {
            Movement(input, deltaTime);
            Look(input, deltaTime);
        }

        void Movement(InputHandler input, float deltaTime)
        {
            if (input.IsKeyDown(Keys.W)) position += front * speed * deltaTime;
            if (input.IsKeyDown(Keys.S)) position -= front * speed * deltaTime;
            if (input.IsKeyDown(Keys.A)) position -= Vector3.Normalize(Vector3.Cross(front, up)) * speed * deltaTime;
            if (input.IsKeyDown(Keys.D)) position += Vector3.Normalize(Vector3.Cross(front, up)) * speed * deltaTime;

            if (input.IsKeyDown(Keys.Space)) position += up * speed * deltaTime;
            if (input.IsKeyDown(Keys.ShiftKey)) position -= up * speed * deltaTime;
        }

        void Look(InputHandler input, float deltaTime)
        {
            if (input.IsMouseButtonDown(MouseButtons.Right))
            {
                if (firstMouseMove)
                {
                    lastMousePos = new Vector2(input.MousePos.X, input.MousePos.Y);
                    firstMouseMove = false;
                    return;
                }

                float deltaX = input.MousePos.X - lastMousePos.X;
                float deltaY = input.MousePos.Y - lastMousePos.Y;

                lastMousePos = new Vector2(input.MousePos.X, input.MousePos.Y);

                Yaw += deltaX * sensitivity;

                if (Pitch > 89f) Pitch = 89f;
                else if (Pitch < -89f) Pitch = -89f;
                else Pitch -= deltaY * sensitivity;

                UpdateVectors();
            }
        }

        private void UpdateVectors()
        {
            front.X = MathF.Cos(pitch) * MathF.Cos(yaw);
            front.Y = MathF.Sin(pitch);
            front.Z = MathF.Cos(pitch) * MathF.Sin(yaw);
            front = Vector3.Normalize(front);

            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(position, position + Front, Up);
        }

        public Matrix4 GetProjectionMatrix(int width, int height)
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), width / height, 0.01f, 100.0f);
        }
    }
}
