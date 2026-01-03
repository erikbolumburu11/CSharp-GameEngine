namespace GameEngine.Engine
{
    public interface IComponentSerializable
    {
        Dictionary<string, object> Save();
        void Load(Dictionary<string, object> data);
    }

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
