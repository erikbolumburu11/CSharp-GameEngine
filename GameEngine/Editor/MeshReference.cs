namespace GameEngine.Editor
{
    public readonly struct MeshReference
    {
        public Guid Guid { get; }

        public MeshReference(Guid guid)
        {
            Guid = guid;
        }

        public override string ToString()
            => Guid == Guid.Empty ? string.Empty : Guid.ToString();
    }
}
