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
                label = "Mesh",
                valueType = typeof(MeshReference),
                getValue = () => new MeshReference(target.mesh),
                setValue = value =>
                {
                    var meshReference = (MeshReference)value;
                    target.mesh = meshReference.Guid;
                }
            });

            fields.Add(new FieldDescriptor
            {
                label = "Material",
                valueType = typeof(MaterialReference),
                getValue = () => new MaterialReference(target.material),
                setValue = value =>
                {
                    var materialReference = (MaterialReference)value;
                    Guid guid = materialReference.Guid;
                    if (guid != Guid.Empty)
                        editorState.engineHost.materialManager.Get(guid);
                    target.material = guid;
                }
            });
        }
    }
}
