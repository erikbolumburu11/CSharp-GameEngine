namespace GameEngine.Editor
{
    public readonly struct MaterialReference
    {
        public string Path { get; }

        public MaterialReference(string path)
        {
            Path = path ?? string.Empty;
        }

        public override string ToString() => Path;
    }
}
