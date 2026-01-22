using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public static class RenderMath
    {
        public static Matrix4 CreateModelMatrix(Transform transform)
        {
            Matrix4 translation = Matrix4.CreateTranslation(transform.WorldPosition);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(transform.WorldRotation);
            Matrix4 scale = Matrix4.CreateScale(transform.WorldScale);

            return scale * rotation * translation;
        }
    }
}
