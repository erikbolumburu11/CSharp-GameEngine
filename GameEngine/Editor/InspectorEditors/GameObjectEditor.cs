using GameEngine.Engine;

public class GameObjectEditor : Editor<GameObject>
{
    public GameObjectEditor(GameObject gameObject) : base(gameObject)
    {
        fields.Add(new FieldDescriptor
        {
            label = "Name",
            valueType = typeof(string),
            getValue = () => gameObject.name,
            setValue = v => gameObject.SetName((string)v)
        });
    }
}
