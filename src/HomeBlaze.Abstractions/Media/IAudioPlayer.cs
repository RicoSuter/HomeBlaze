namespace HomeBlaze.Abstractions.Media
{
    public interface IAudioPlayer
    {
        bool IsAudioMuted { get; }

        bool? IsAudioPlaying { get; }
        
        int AudioVolume { get; }

        string? CurrentAudioTrackAlbum { get; }

        string? CurrentAudioTrackCreator { get; }

        string? CurrentAudioTrackImageUri { get; }

        string? CurrentAudioTrackTitle { get; }

        string? CurrentAudioTrackUri { get; }

        Task MuteAsync(CancellationToken cancellationToken = default);

        Task NextAsync(CancellationToken cancellationToken = default);

        Task PauseAsync(CancellationToken cancellationToken = default);

        Task PlayAsync(CancellationToken cancellationToken = default);

        Task PreviousAsync(CancellationToken cancellationToken = default);

        Task SetVolumeAsync(int volume, CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);

        Task UnmuteAsync(CancellationToken cancellationToken = default);
    }
}