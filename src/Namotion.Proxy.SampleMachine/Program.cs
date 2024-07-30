using Microsoft.AspNetCore.Mvc;
using Namotion.Proxy;
using Namotion.Proxy.AspNetCore.Controllers;
using Namotion.Proxy.OpcUa.Annotations;
using NSwag.Annotations;

namespace Namotion.Trackable.SampleMachine
{
    [GenerateProxy]
    public class RootBase
    {
        [OpcUaNode("Machines", "http://opcfoundation.org/UA/Machinery/")]
        [OpcUaNodeReferenceType("Organizes")]
        [OpcUaNodeItemReferenceType("Organizes")]
        public virtual IReadOnlyDictionary<string, Machine> Machines { get; set; } = new Dictionary<string, Machine>();
    }

    [GenerateProxy]
    [OpcUaTypeDefinition("BaseObjectType")]
    public class MachineBase
    {
        [OpcUaNode("Identification", "http://opcfoundation.org/UA/DI/")]
        [OpcUaNodeReferenceType("HasAddIn")]
        public virtual Identification Identification { get; }

        [OpcUaNode("MachineryBuildingBlocks", "http://opcfoundation.org/UA/")]
        [OpcUaNodeReferenceType("HasComponent")]
        public virtual MachineryBuildingBlocks MachineryBuildingBlocks { get; }

        [OpcUaNode("Monitoring", "http://opcfoundation.org/UA/")]
        [OpcUaNodeReferenceType("HasComponent")]
        [OpcUaNodeItemReferenceType("HasComponent")]
        public virtual IReadOnlyDictionary<string, ProcessValueType> Monitoring { get; set; } = new Dictionary<string, ProcessValueType>();

        public MachineBase()
        {
            Identification = new Identification();
            MachineryBuildingBlocks = new MachineryBuildingBlocks(Identification);
        }
    }

    [GenerateProxy]
    [OpcUaTypeDefinition("ProcessValueType", "http://opcfoundation.org/UA/Machinery/ProcessValues/")]
    public class ProcessValueTypeBase
    {
        [OpcUaVariable("AnalogSignal", "http://opcfoundation.org/UA/PADIM/")]
        public virtual AnalogSignalVariable AnalogSignal { get; } = new AnalogSignalVariable();

        [OpcUaVariable("SignalTag", "http://opcfoundation.org/UA/PADIM/")]
        public virtual string? SignalTag { get; set; }
    }

    [GenerateProxy]
    [OpcUaTypeDefinition("AnalogSignalVariableType", "http://opcfoundation.org/UA/PADIM/")]
    public class AnalogSignalVariableBase
    {
        [OpcUaVariable("ActualValue", "http://opcfoundation.org/UA/")]
        public virtual object? ActualValue { get; set; } = "My Manufacturer";

        [OpcUaVariable("EURange", "http://opcfoundation.org/UA/")]
        public virtual object? EURange { get; set; } = "My Manufacturer";

        [OpcUaVariable("EngineeringUnits", "http://opcfoundation.org/UA/")]
        public virtual object? EngineeringUnits { get; set; } = "My Manufacturer";
    }

    [GenerateProxy]
    [OpcUaTypeDefinition("FolderType")]
    public class MachineryBuildingBlocksBase
    {
        [OpcUaNode("Identification", "http://opcfoundation.org/UA/DI/")]
        [OpcUaNodeReferenceType("HasAddIn")]
        public virtual Identification Identification { get; }

        public MachineryBuildingBlocksBase(Identification identification)
        {
            Identification = identification;
        }
    }

    [GenerateProxy]
    [OpcUaTypeDefinition("MachineIdentificationType", "http://opcfoundation.org/UA/Machinery/")]
    public class IdentificationBase
    {
        [OpcUaVariable("Manufacturer", "http://opcfoundation.org/UA/DI/")]
        public virtual string? Manufacturer { get; set; } = "My Manufacturer";

        [OpcUaVariable("SerialNumber", "http://opcfoundation.org/UA/DI/")]
        public virtual string? SerialNumber { get; set; } = "My Serial Number";
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var context = ProxyContext
                .CreateBuilder()
                .WithRegistry()
                .WithFullPropertyTracking()
                .WithProxyLifecycle()
                .WithDataAnnotationValidation()
                .Build();

            var root = new Root(context)
            {
                Machines = new Dictionary<string, Machine>
                {
                    {
                        "MyMachine", new Machine
                        {
                            Identification =
                            {
                                SerialNumber = "Hello world!"
                            },
                            Monitoring = new Dictionary<string, ProcessValueType>
                            {
                                {
                                    "MyProcess", 
                                    new ProcessValueType
                                    {
                                        SignalTag = "MySignal",
                                        AnalogSignal =
                                        {
                                            ActualValue = 42.0,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // trackable
            builder.Services.AddSingleton(root);

            // trackable api controllers
            builder.Services.AddProxyControllers<Root, TrackablesController<Root>>();

            // OPC UA server
            builder.Services.AddOpcUaServerProxySource<Root>("opc");

            //builder.Services.AddOpcUaClientProxySource<Root>("opc", "opc.tcp://localhost:4840");

            // trackable GraphQL
            builder.Services
                .AddGraphQLServer()
                .AddInMemorySubscriptions()
                .AddTrackedGraphQL<Root>();

            // other asp services
            builder.Services.AddOpenApiDocument();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapGraphQL();

            app.UseOpenApi();
            app.UseSwaggerUi();

            app.MapControllers();
            app.Run();
        }

        [OpenApiTag("Root")]
        [Route("/api/root")]
        public class TrackablesController<TProxy> : ProxyControllerBase<TProxy> where TProxy : IProxy
        {
            public TrackablesController(TProxy proxy) : base(proxy)
            {
            }
        }
    }
}