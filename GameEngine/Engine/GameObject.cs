using OpenTK.Mathematics;

namespace GameEngine.Engine
{
    public class GameObject
    {
        public Guid Id { get; private set; } = Guid.Empty;
        public string name { get; private set; }

        public Transform transform;

        public event Action<GameObject>? Changed;
        public event Action<GameObject>? HierarchyChanged;

        private List<Component> components;
        public IReadOnlyList<Component> Components => components;

        public GameObject(string name, Guid id)
        {
            Id = id;
            components = new();

            this.name = name;

            transform = new(this);

            EnsureId();
        }

        public GameObject(string name)
        {
            components = new();

            this.name = name;

            transform = new(this);

            EnsureId();
        }

        private void EnsureId()
        {
            if(Id == Guid.Empty)
                Id = Guid.NewGuid();
        }

        internal void NotifyChanged()
        {
            Changed?.Invoke(this);
        }

        internal void NotifyHierarchyChanged()
        {
            HierarchyChanged?.Invoke(this);
        }

        public void RegenerateId()
        {
            Id = Guid.NewGuid();
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

        public bool RemoveComponent(Component component)
        {
            int index = components.IndexOf(component);
            if (index < 0)
                return false;

            components.RemoveAt(index);
            component.OnDestroy();
            return true;
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
            NotifyChanged();
        }

        public void SetPosition(Vector3 position)
        {
            transform.WorldPosition = position;
            NotifyChanged();
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.WorldRotation = rotation;
            NotifyChanged();
        }

        public void SetScale(Vector3 scale)
        {
            transform.WorldScale = scale;
            NotifyChanged();
        }

        public override string ToString()
        {
            return name;
        }
    }
}
