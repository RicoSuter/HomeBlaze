namespace Namotion.Proxy.SampleBlazor.Models
{
    [GenerateProxy]
    public abstract class GameBase
    {
        public virtual PlayerBase[] Players { get; protected set; } = [];

        public void AddPlayer(PlayerBase player)
        {
            lock (this)
                Players = [.. Players, player];
        }

        public void RemovePlayer(PlayerBase player)
        {
            lock (this)
                Players = Players.Where(p => p != player).ToArray();
        }
    }
}
