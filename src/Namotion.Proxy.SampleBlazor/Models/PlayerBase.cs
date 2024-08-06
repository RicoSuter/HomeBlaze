namespace Namotion.Proxy.SampleBlazor.Models
{
    [GenerateProxy]
    public class PlayerBase : IDisposable
    {
        private readonly Game _game;

        public virtual string Name { get; set; } = Guid.NewGuid().ToString();

        public PlayerBase(Game game)
        {
            _game = game;
            _game.AddPlayer(this);
        }

        public void Dispose()
        {
            _game.RemovePlayer(this);
        }
    }
}
