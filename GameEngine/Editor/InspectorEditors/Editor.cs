using GameEngine.Engine;

public abstract class Editor<T>
{
    public T target;
    public List<FieldDescriptor> fields;

    protected Editor(T target)
    {
        fields = new();
        this.target = target;
    }
}