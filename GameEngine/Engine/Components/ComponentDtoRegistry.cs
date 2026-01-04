using System.Text.Json;

namespace GameEngine.Engine
{
    public static class ComponentDtoRegistry
    {
        private static readonly Dictionary<string, Type> componentTypes = new();
        private static readonly Dictionary<string, Type> dtoTypes = new();
        private static readonly Dictionary<string, Func<Component>> factory = new();

        // Component -> DTO object
        private static readonly Dictionary<string, Func<Component, object>> _toDto = new();

        // DTO object -> Component
        private static readonly Dictionary<string, Action<Component, object>> _fromDto = new();

        public static void Register<TComponent, TDto>(
            string typeKey,
            Func<TComponent, TDto> toDto,
            Action<TComponent, TDto> fromDto,
            Func<TComponent>? factory = null)
            where TComponent : Component
            where TDto : class
        {
            componentTypes[typeKey] = typeof(TComponent);
            dtoTypes[typeKey] = typeof(TDto);

            ComponentDtoRegistry.factory[typeKey] = factory != null
                ? () => factory()
                : () => (TComponent)global::System.Activator.CreateInstance(typeof(TComponent))!;

            _toDto[typeKey] = c => toDto((TComponent)c)!;
            _fromDto[typeKey] = (c, dto) => fromDto((TComponent)c, (TDto)dto);
        }

        public static bool TryCreate(string typeKey, out Component component)
        {
            if (factory.TryGetValue(typeKey, out var f))
            {
                component = f();
                return true;
            }

            component = null!;
            return false;
        }

        public static Type GetDtoType(string typeKey)
            => dtoTypes[typeKey];

        public static object ToDto(Component component, string typeKey)
            => _toDto[typeKey](component);

        public static void FromDto(Component component, string typeKey, object dto)
            => _fromDto[typeKey](component, dto);
    }
}