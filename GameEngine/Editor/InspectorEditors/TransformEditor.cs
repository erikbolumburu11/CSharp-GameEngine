using GameEngine.Engine;
using OpenTK.Mathematics;

namespace GameEngine.Editor
{
    public class TransformEditor : Editor<GameObject>
    {
        Transform transform;

        public TransformEditor(GameObject gameObject) : base(gameObject)
        {
            fields.Add(new FieldDescriptor
            {
                label = "Position",
                valueType = typeof(Vector3),
                getValue = () => gameObject.transform.position,
                setValue = value => gameObject.transform.position = (Vector3)value
            }
            );

            fields.Add(new FieldDescriptor
            {
                label = "Rotation",
                valueType = typeof(Vector3),
                getValue = () => QuaternionToEulerDegrees(gameObject.transform.rotation),
                setValue = value =>
                {
                    gameObject.SetRotation(
                        Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(((Vector3)value).X)) *
                        Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(((Vector3)value).Y)) *
                        Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(((Vector3)value).Z))
                    );
                }
            }
            );

            fields.Add(new FieldDescriptor
            {
                label = "Scale",
                valueType = typeof(Vector3),
                getValue = () => gameObject.transform.scale,
                setValue = value => gameObject.transform.scale = (Vector3)value
            }
            );
        }

        private static Vector3 QuaternionToEulerDegrees(Quaternion rotation)
        {
            Vector3 eulerRadians = rotation.ToEulerAngles();
            return new Vector3(
                MathHelper.RadiansToDegrees(eulerRadians.X),
                MathHelper.RadiansToDegrees(eulerRadians.Y),
                MathHelper.RadiansToDegrees(eulerRadians.Z)
            );
        }
    }
}
