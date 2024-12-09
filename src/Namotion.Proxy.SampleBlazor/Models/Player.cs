namespace Namotion.Proxy.SampleBlazor.Models
{
    [GenerateProxy]
    public partial class Player : IDisposable
    {
        private readonly Game _game;

        public partial string Name { get; set; }

        public Player(Game game)
        {
            _game = game;
            _game.AddPlayer(this);

            Name = Guid.NewGuid().ToString();
        }

        public void Dispose()
        {
            _game.RemovePlayer(this);
        }
    }
}
