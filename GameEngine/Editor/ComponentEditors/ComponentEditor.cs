using GameEngine.Engine;

public abstract class ComponentEditor<T>
{
    public T target;
    public List<FieldDescriptor> fields;

    protected ComponentEditor(T target)
    {
        fields = new();
        this.target = target;
    }
}