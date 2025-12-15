using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.CodeDom;
using System.ComponentModel;

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

        public Component? AddComponent(Component component)
        {
            if (HasComponent(component.GetType()))
            {
                // TODO: Add to log when logging is implemented.
                Console.WriteLine($"Component of Type: {component.GetType()} already exists!");
                return null;
            }

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

        public bool HasComponent<T>() where T : Component
        {
            if (components.OfType<T>().Count() > 0) return true;

            return false;
        }

        public bool HasComponent(Type type)
        {
            if (!typeof(Component).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type must be a Component", nameof(type));
            }

            return components.Any(c => type.IsAssignableFrom(c.GetType()));
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

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
            Changed?.Invoke(this);
        }

        public void SetScale(Vector3 scale)
        {
            transform.scale = scale;
            Changed?.Invoke(this);
        }

        public override string ToString()
        {
            return name;
        }
    }
}
