using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace GameEngine.Engine
{
    public class Transform
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;

        public Transform()
        {
            position = Vector3.Zero;
            scale = new Vector3(1, 1, 1);
            rotation = Quaternion.Identity;
        }
    }

    public class GameObject
    {
        public string name { get; private set; }

        public Transform transform;

        public event Action<GameObject>? Changed;

        private List<Component> components;
        public IReadOnlyList<Component> Components => components;

        public GameObject(string name)
        {
            components = new();

            this.name = name;

            transform = new();
        }

        // TODO: Dont add copmonent if one of type already exists.
        public Component AddComponent(Component component)
        {
            component.gameObject = this;
            components.Add(component);
            component.Start();
            return component;
        }

        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T();
            AddComponent(component);
            return component;
        }

        public Component AddComponent(Type type)
        {
            if (!typeof(Component).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type must be a Component", nameof(type));
            }

            var component = (Component)Activator.CreateInstance(type)!;

            AddComponent(component);

            return component;
        }

        public T? GetComponent<T>() where T : Component
        {
            return components.OfType<T>().FirstOrDefault();
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            Changed?.Invoke(this);
        }

        public override string ToString()
        {
            return name;
        }
    }
}
