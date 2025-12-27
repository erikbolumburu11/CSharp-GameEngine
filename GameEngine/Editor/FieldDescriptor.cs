public struct FieldDescriptor
{
    public string label;
    public Type valueType;
    public Func<object> getValue;
    public Action<object> setValue;
}