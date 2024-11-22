namespace Namotion.Proxy.SampleBlazor.Models
{
    [GenerateProxy]
    public partial class Game
    {
        public partial Player[] Players { get; protected set; }

        public Game()
        {
            Players = [];
        }

        public void AddPlayer(Player player)
        {
            lock (this)
                Players = [.. Players, player];
        }

        public void RemovePlayer(Player player)
        {
            lock (this)
                Players = Players.Where(p => p != player).ToArray();
        }
    }
}
