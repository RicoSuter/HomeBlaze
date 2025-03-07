using HomeBlaze2.Components;
using HomeBlaze2.Host;
using Namotion.Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var context = ProxyContext
    .CreateBuilder()
    .WithRegistry()
    .WithFullPropertyTracking()
    .WithProxyLifecycle()
    .WithDataAnnotationValidation()
    .Build();

var things = new Things(context);

// trackable
builder.Services.AddSingleton(things);

// trackable api controllers
builder.Services.AddProxyControllers<Things, ProxiesController<Things>>();

// trackable UPC UA
// builder.Services.AddOpcUaServerProxySource<Things>("opc", rootName: "Root");

// trackable mqtt
builder.Services.AddMqttServerProxySource<Things>("mqtt");

// trackable GraphQL
builder.Services
    .AddGraphQLServer()
    .AddInMemorySubscriptions()
    .AddTrackedGraphQL<Things>();

// other asp services
builder.Services.AddOpenApiDocument();
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseAuthorization();

app.MapGraphQL();

app.UseOpenApi();
app.UseSwaggerUi();

app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();