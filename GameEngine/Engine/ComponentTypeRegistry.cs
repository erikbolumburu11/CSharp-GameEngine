using GameEngine.Engine;

public static class ComponentTypeRegistry
{
    private static Dictionary<string, Type> types;
    public static Dictionary<string, Type> Types => types;

    static ComponentTypeRegistry()
    {
        types = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(Component).IsAssignableFrom(t) && !t.IsAbstract)
            .ToDictionary(t => t.FullName, t => t);
    }


    public static Type Get(string name)
    {
        return types[name];
    }

    public static string Get(Type type)
    {
        return types.FirstOrDefault(x => x.Value == type).Key;
    }
}