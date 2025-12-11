using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Engine.Components
{
    public static class InspectorInfo
    {
        public static IEnumerable<MemberInfo> GetInspectableMembers(object component)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var fields = component.GetType()
                .GetFields(flags)
                .Where(f => f.GetCustomAttribute<ExposeInInspectorAttribute>() != null);

            var properties = component.GetType()
                .GetProperties(flags)
                .Where(p => p.GetCustomAttribute<ExposeInInspectorAttribute>() != null);

            return fields.Cast<MemberInfo>().Concat(properties);
        }
    }
}
