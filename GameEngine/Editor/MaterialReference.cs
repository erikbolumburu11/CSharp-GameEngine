namespace GameEngine.Editor
{
    public readonly struct MaterialReference
    {
        public Guid Guid { get; }

        public MaterialReference(Guid guid)
        {
            Guid = guid;
        }

        public override string ToString()
            => Guid == Guid.Empty ? string.Empty : Guid.ToString();
    }
}
