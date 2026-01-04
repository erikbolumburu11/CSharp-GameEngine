using GameEngine.Engine.Components;

namespace GameEngine.Editor
{
    public class MeshRendererEditor : Editor<MeshRenderer>
    {
        private readonly EditorState editorState;

        public MeshRendererEditor(MeshRenderer target, EditorState editorState) : base(target)
        {
            this.editorState = editorState ?? throw new ArgumentNullException(nameof(editorState));

            fields.Add(new FieldDescriptor
            {
                label = "Material",
                valueType = typeof(MaterialReference),
                getValue = () => new MaterialReference(target.material ?? string.Empty),
                setValue = value =>
                {
                    var materialReference = (MaterialReference)value;
                    string? path = string.IsNullOrWhiteSpace(materialReference.Path)
                        ? null
                        : materialReference.Path;
                    if (!string.IsNullOrWhiteSpace(path))
                        editorState.engineHost.materialManager.Get(path);
                    target.material = path;
                }
            });
        }
    }
}