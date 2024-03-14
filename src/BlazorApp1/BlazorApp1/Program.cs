using BlazorApp1.Components;
using System.Reactive.Subjects;

namespace BlazorApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services
                .AddScoped<Player>()
                .AddSingleton<Game>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }

    public class Player
    {
        public string Id { get; }

        public Player()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class Game : IObservable<object>
    {
        private Subject<object> _subject = new();
        private Dictionary<string, Player> _players = new();

        public IReadOnlyCollection<Player> Players => _players.Values;

        public void AddPlayer(Player player)
        {
            _players[player.Id] = player;
            _subject.OnNext(player);
        }

        public void RemovePlayer(Player player)
        {
            //_players.Remove(player.Id);
            _subject.OnNext(player);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
