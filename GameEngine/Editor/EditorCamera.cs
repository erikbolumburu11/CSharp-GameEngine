using GameEngine.Engine;
using OpenTK.Mathematics;

namespace GameEngine.Editor
{
    public class EditorCamera : Camera
    {
        public float sensitivity;
        public float speed;

        Vector2 lastMousePos;
        bool firstMouseMove = true;

        public EditorCamera
        (
            Vector3 position,
            float sensitivity,
            float speed,
            InputHandler inputHandler,
            int width,
            int height
        ) : base(position, speed, sensitivity, width, height)
        {
            this.sensitivity = sensitivity;
            this.speed = speed;
            this.position = position;

            inputHandler.MouseReleased += (e) =>
            {
                if (e == MouseButtons.Right)
                {
                    firstMouseMove = true;
                }
            };
        }

        public override void Update(InputHandler input, float deltaTime)
        {
            base.Update(input, deltaTime);
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
    }
}
