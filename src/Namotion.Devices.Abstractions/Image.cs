using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions
{
    public record Image
    {
        [State]
        required public byte[] Data { get; init; }

        [State]
        required public string MimeType { get; init; }
    }
}
