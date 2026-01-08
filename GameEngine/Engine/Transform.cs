using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class Transform
    {
        public Vector3 localPosition;
        public Vector3 localScale;
        public Quaternion localRotation;

        private Transform? _parent;

        public Transform? parent
        {
            get => _parent;
            set
            {
                if (_parent == value)
                    return;

                _parent = value;
                GameObject.NotifyHierarchyChanged();
            }
        }

        public GameObject GameObject { get; private set; }

        public Matrix4 LocalMatrix =>
                Matrix4.CreateScale(localScale) *
                Matrix4.CreateFromQuaternion(localRotation) *
                Matrix4.CreateTranslation(localPosition);

        public Matrix4 WorldMatrix
        {
            get
            {
                if (parent == null)
                    return LocalMatrix;

                return LocalMatrix * parent.WorldMatrix; 
            }
        }

        public Transform(GameObject gameObject)
        {
            GameObject = gameObject;

            localPosition = Vector3.Zero;
            localScale = Vector3.One;
            localRotation = Quaternion.Identity;
        }

        public Vector3 WorldPosition
        {
            get => WorldMatrix.ExtractTranslation();
            set
            {
                if (parent == null) localPosition = value;
                else
                {
                    Matrix4 parentWorld = parent.WorldMatrix;
                    Matrix4 invParentWorld = parentWorld.Inverted();

                    Vector4 localPos4 = new Vector4(value, 1.0f) * invParentWorld;
                    localPosition = localPos4.Xyz;
                }
            }
        }

        public Quaternion WorldRotation
        {
            get => WorldMatrix.ExtractRotation();
            set
            {
                if (parent == null) localRotation = value;
                else
                {
                    Matrix4 parentWorldRotation = Matrix4.CreateFromQuaternion(parent.WorldRotation);
                    Matrix4 invParentWorldRotation = parentWorldRotation.Inverted();

                    Matrix4 localRotationMatrix = Matrix4.CreateFromQuaternion(value) * invParentWorldRotation;
                    localRotation = localRotationMatrix.ExtractRotation();
                }
            }
        }

        public Vector3 WorldScale
        {
            get => WorldMatrix.ExtractScale();
            set
            {
                if (parent == null) localScale = value;
                else
                {
                    Vector3 parentWorldScale = parent.WorldScale;
                    localScale = new Vector3(
                        value.X / parentWorldScale.X,
                        value.Y / parentWorldScale.Y,
                        value.Z / parentWorldScale.Z
                    );
                }
            }
        }
    }
}
