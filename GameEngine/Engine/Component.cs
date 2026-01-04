namespace GameEngine.Engine
{
    public class Component
    {
        public virtual string name => GetType().Name;
        public GameObject gameObject { get; internal set; }

        public Transform transform => gameObject.transform;

        public virtual void Start() { }
        public virtual void Update(float deltaTime) { }
        public virtual void OnDestroy() { }

    }
}
