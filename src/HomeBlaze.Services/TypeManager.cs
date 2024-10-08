﻿using Microsoft.Extensions.Logging;

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Json;
using HomeBlaze.Abstractions.Services;

using Namotion.NuGetPlugins;
using Namotion.Storage;
using Namotion.Devices.Abstractions.Messages;

using System.Reflection;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBlaze.Services
{
    public class TypeManager : ITypeManager
    {
        private readonly JsonSerializerOptions _extensionSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        };

        private readonly IBlobContainer _blobContainer;
        private readonly IDynamicNuGetPackageLoader _nuGetPackageLoader;
        private readonly ILogger<TypeManager> _logger;

        private Task? _initializationTask;
        private Dictionary<Type, ThingSetupAttribute> _thingSetupAttributes = [];
        private Dictionary<Type, ThingComponentAttribute> _thingComponentAttributes = [];

        public NuGetPlugin[] Plugins { get; private set; } = Array.Empty<NuGetPlugin>();

        public Type[] ThingTypes { get; private set; } = Array.Empty<Type>();

        public Type[] ThingInterfaces { get; private set; } = Array.Empty<Type>();

        public Type[] EventTypes { get; private set; } = Array.Empty<Type>();

        public TypeManager(
            IBlobContainer blobContainer,
            IDynamicNuGetPackageLoader nuGetPackageLoader,
            ILogger<TypeManager> logger)
        {
            _blobContainer = blobContainer;
            _nuGetPackageLoader = nuGetPackageLoader;
            _logger = logger;
        }

        public ThingSetupAttribute? TryGetThingSetupAttribute(Type? thingType)
        {
            return thingType is not null ? _thingSetupAttributes.TryGetValue(thingType, out var value) ? value : null : null;
        }

        public ThingComponentAttribute? TryGetThingComponentAttribute(Type? thingType)
        {
            return thingType is not null ? _thingComponentAttributes.TryGetValue(thingType, out var value) ? value : null : null;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            lock (this)
            {
                if (_initializationTask == null)
                {
                    _initializationTask = Task.Run(async () =>
                    {
                        try
                        {
                            var json = await _blobContainer.ReadAllTextAsync("Plugins.json", cancellationToken);
                            Plugins = JsonSerializer.Deserialize<NuGetPlugin[]>(json, _extensionSerializerOptions) ?? Array.Empty<NuGetPlugin>();

                            foreach (var plugin in Plugins)
                            {
                                await plugin.LoadAsync(_nuGetPackageLoader, _logger, cancellationToken);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Failed to load plugins from Plugins.json.");
                        }

                        try
                        {
                            var exportedTypes = GetExportedTypes();

                            ThingInterfaces = GetThingInterfaces(exportedTypes);
                            ThingTypes = GetThingTypes(exportedTypes);
                            EventTypes = GetEventTypes(exportedTypes);

                            foreach (var type in ThingTypes)
                            {
                                var thingTypeAttribute = type.GetCustomAttribute<ThingTypeAttribute>(true);
                                var fullName =
                                    thingTypeAttribute?.FullName ??
                                    type.FullName ??
                                    throw new InvalidOperationException("No FullName.");

                                JsonInheritanceConverter<IThing>.AdditionalKnownTypes[fullName] = type;
                            }

                            FindTypesWithAttribute<ThingSetupAttribute>(exportedTypes, (type, attribute) =>
                            {
                                _thingSetupAttributes[attribute.ThingType] = attribute;
                                attribute.ComponentType = type;
                            });

                            FindTypesWithAttribute<ThingComponentAttribute>(exportedTypes, (type, attribute) =>
                            {
                                _thingComponentAttributes[attribute.ThingType] = attribute;
                                attribute.ComponentType = type;
                            });
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Failed to load types.");
                        }
                    }, cancellationToken);
                }
            }

            await _initializationTask;
        }

        private void FindTypesWithAttribute<TAttribute>(Type[] exportedTypes, Action<Type, TAttribute> action)
            where TAttribute : Attribute
        {
            foreach (var type in exportedTypes)
            {
                var attribute = type.GetCustomAttribute<TAttribute>(true);
                if (attribute is not null)
                {
                    action(type, attribute);
                }
            }
        }

        private Type[] GetExportedTypes()
        {
            var assemblies = new Assembly[] { Assembly.GetEntryAssembly()! }
               .Concat(Assembly
                   .GetEntryAssembly()!
                   .GetReferencedAssemblies()
                   .Select(name => AssemblyLoadContext.Default.LoadFromAssemblyName(name)))
               .Concat(Plugins!.Select(e => e.Assembly))
               .Where(a => a != null)
               .ToArray();

            var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            assemblies = assemblies
                .Concat(Directory
                    .GetFiles(appDirectory!, "*.dll")
                    .Where(f => assemblies.All(u => u?.GetName().Name != Path.GetFileNameWithoutExtension(f)))
                    .Select(f => AssemblyLoadContext.Default.LoadFromAssemblyPath(f)))
                .Where(a => a != null)
                .ToArray();

            return assemblies
                .SelectMany(a => a?.ExportedTypes ?? Enumerable.Empty<Type>())
                .ToArray();
        }

        private Type[] GetThingTypes(Type[] exportedTypes)
        {
            var types = exportedTypes
                .Where(t => t.IsAssignableTo(typeof(IThing)))
                .Where(t => t.IsClass && !t.IsAbstract)
                .OrderBy(t => t.FullName)
                .ToArray();

            return types;
        }

        private Type[] GetEventTypes(Type[] exportedTypes)
        {
            var types = exportedTypes
                .Where(t => t.IsAssignableTo(typeof(IEvent)))
                .Where(t => t.IsClass && !t.IsAbstract)
                .OrderBy(t => t.FullName)
                .ToArray();

            return types;
        }

        private Type[] GetThingInterfaces(Type[] exportedTypes)
        {
            var types = exportedTypes
                .Where(t => t.IsInterface && 
                            (t.IsAssignableTo(typeof(IThing)) || 
                             t.GetProperties().Any(p => p.IsDefined(typeof(StateAttribute))))&& 
                            !t.IsConstructedGenericType)
                .OrderBy(t => t.FullName)
                .ToArray();

            return types;
        }
    }
}