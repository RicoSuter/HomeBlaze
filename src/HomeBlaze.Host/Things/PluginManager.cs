using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services;
using System.Collections.Generic;
using System.Linq;

namespace HomeBlaze.Things
{
    public class PluginManager : IThing, IIconProvider
    {
        private readonly SystemThing _systemThing;
        private readonly TypeManager _typeManager;

        private IEnumerable<string>? _plugins;
        private IEnumerable<string>? _thingTypes;
        private IEnumerable<string>? _tingInterfaces;

        public string Id => _systemThing.Id + "/plugins";

        public string Title => "Plugin Manager";

        public string IconName => "fa-solid fa-cubes";

        [State]
        public IEnumerable<string> Plugins => _plugins != null ? _plugins : _plugins = _typeManager
            .Plugins
            .Where(p => p.Name != null)
            .Select(p => p.Name!);

        [State]
        public IEnumerable<string> ThingTypes => (_thingTypes != null ? _thingTypes : _thingTypes = _typeManager
            .ThingTypes
            .Where(p => p.FullName != null)
            .Select(p => p.FullName!))
            .Distinct();

        [State]
        public IEnumerable<string> ThingInterfaces => (_tingInterfaces != null ? _tingInterfaces : _tingInterfaces = _typeManager
            .ThingInterfaces
            .Where(p => p.FullName != null)
            .Select(p => p.FullName!))
            .Distinct();

        public PluginManager(SystemThing systemThing, ITypeManager typeManager)
        {
            _systemThing = systemThing;
            _typeManager = (TypeManager)typeManager;
        }
    }
}